using System;
using AntColony.Game.Colonies.Ants.States;
using AntColony.Game.Colonies.Items;
using AntColony.Game.Colonies.Structures;
using Omoch.Framework;
using Omoch.Flows;
using UnityEngine;
using Omoch.Randoms;

#nullable enable

namespace AntColony.Game.Colonies.Ants
{
    public class AntLogic : LogicBase<IAntViewOrder>, IAntPeek
    {
        private const float RotationEasingMin = 0.15f;
        private const float RotationEasingMax = 0.5f;
        private const float SpeedMin = 0.24f;
        private const float SpeedMax = 0.36f;
        private const float SwayAngle = 20f * Mathf.Deg2Rad;
        public const float SatietyMax = 100f;
        private const float HungryRatio = 0.5f;
        private const float SatietyDecrease = 0.03f;

        private OmStateMachine<AntLogic> stateMachine;
        private int randomSwayAngleCounter;
        private int stamina;
        private int waitFrame;

        private readonly ColonyLogic colony;
        public ColonyLogic Colony => colony;
        private float randomSwayAngle;
        public float RandomSwayAngle => randomSwayAngle;
        public int GridX => (int)X;
        public int GridY => (int)Y;
        public AntKind Kind { get; set; }

        /// <summary>運搬アイテム</summary>
        public ItemLogic? CarryingItem { get; set; }

        /// <summary>足元のセル</summary>
        private Cell? currentCell;
        public Cell? CurrentCell
        {
            get => currentCell;
            private set => currentCell = value;
        }

        /// <summary>寿命</summary>
        public float Life { get; set; }

        /// <summary>満腹度</summary>
        public float Satiety { get; set; }

        /// <summary>歩行速度</summary>
        public float Speed { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Rotation { get; set; }
        public bool IsHungry => Satiety < SatietyMax * HungryRatio;

        public AntLogic(ColonyLogic colony, AntKind kind, float px, float py)
        {
            this.colony = colony;
            Kind = kind;
            X = px;
            Y = py;

            Life = Kind switch
            {
                AntKind.Worker => Randomizer.Next(1800 * 30, 1800 * 50),
                AntKind.Queen => Randomizer.Next(1800 * 120, 1800 * 150),
                _ => throw new NotImplementedException(),
            };
            Satiety = SatietyMax;
            CarryingItem = null;
            Speed = Mathf.Lerp(SpeedMin, SpeedMax, Randomizer.NextFloat());
            waitFrame = 10;
            Rotation = 90f;

            randomSwayAngle = 0f;
            randomSwayAngleCounter = 0;
            stamina = 0;

            colony.TryGetCell((int)X, (int)Y, out currentCell);

            stateMachine = new OmStateMachine<AntLogic>(this);
            switch (Kind)
            {
                case AntKind.Worker:
                    stateMachine.ChangeState<WorkerStateThink>();
                    break;
                case AntKind.Queen:
                    stateMachine.ChangeState<QueenStateThink>();
                    break;
                default:
                    throw new NotImplementedException();
            }


        }

        public void Simulate()
        {
            if (IsDisposed)
            {
                return;
            }

            Satiety = Mathf.Max(Satiety - SatietyDecrease, 0);
            Life -= Satiety > 0 ? 1 : 2;

            if (Life <= 0)
            {
                Kill();
                return;
            }

            if (waitFrame > 0)
            {
                waitFrame--;
                return;
            }

            // 時々立ち止まる
            if (--stamina < 0)
            {
                stamina = Randomizer.Next(80, 160);
                Wait(Randomizer.Next(7, 15));
                return;
            }

            // 定期的に進む角度をゆらす
            if (++randomSwayAngleCounter % 30 == 0)
            {
                randomSwayAngle = Randomizer.NextFloat(-SwayAngle, SwayAngle);
            }

            colony.TryGetCell(GridX, GridY, out currentCell);

            stateMachine.Update();
        }

        public void Wait(int frame)
        {
            waitFrame += frame;
        }

        /// <summary>
        /// 指定角度へゆっくり回転させて少し前に進む
        /// </summary>
        /// <param name="targetRotation">この角度に向く</param>
        /// <param name="turningIntensity">小回りが利く度合いを0~1で指定</param>
        public void RotateAndMove(float targetRotation, float turningIntensity)
        {
            float easing = Mathf.Lerp(RotationEasingMin, RotationEasingMax, turningIntensity);
            // 進む角度の決定
            Rotation = ToNearbyAngle(Rotation, targetRotation);
            Rotation += (targetRotation - Rotation) * easing;
            X += Mathf.Cos(Rotation) * Speed;
            Y += Mathf.Sin(Rotation) * Speed;

            ApplyCarriedPosition();
        }

        /// <summary>
        /// 足元に死骸と運んでいたものを落として自分を消去
        /// </summary>
        public void Kill()
        {
            if (colony.TryGetNearbyEmptyCell(GridX, GridY, out Cell? cell))
            {
                cell.Item = colony.AddItem(ItemKind.Body);
                cell.Item.SetPosition(cell.CenterX, cell.CenterY);
            }

            if (CarryingItem is not null)
            {
                if (!TryDropCarryingItem())
                {
                    DiscardCarryingItem();
                }
            }

            Dispose();
        }

        /// <summary>
        /// 運んでいるアイテムを口の位置に移動
        /// </summary>
        public void ApplyCarriedPosition()
        {
            if (CarryingItem is null)
            {
                return;
            }

            var px = X + Mathf.Cos(Rotation) * 1.5f;
            var py = Y + Mathf.Sin(Rotation) * 1.5f;
            CarryingItem.SetPosition(px, py);
        }

        /// <summary>
        /// targetの角度に最も近くなるようにrotationの角度を360°ずつ周回させる
        /// </summary>
        private float ToNearbyAngle(float rotation, float target)
        {
            float delta = (rotation - target) % (Mathf.PI * 2);
            if (delta < -Mathf.PI)
            {
                delta += Mathf.PI * 2;
            }
            else if (delta > Mathf.PI)
            {
                delta -= Mathf.PI * 2;
            }
            return delta + target;
        }

        /// <summary>
        /// 指定セルに食料があれば食べる
        /// </summary>
        public bool TryEat(Cell cell)
        {
            ItemLogic? item = cell.Item;

            if (item is null || item.Kind != ItemKind.Food)
            {
                return false;
            }

            int eatAmount = Mathf.Min(Mathf.CeilToInt(SatietyMax - Satiety), item.Amount);
            item.Amount -= eatAmount;
            Satiety = Mathf.Min(Satiety + eatAmount, SatietyMax);

            if (item.Amount <= 0)
            {
                colony.RemoveItem(item);
                cell.Item = null;
            }

            return true;
        }

        /// <summary>
        /// 足元に運んでいるアイテムを置く
        /// </summary>
        public bool TryDropCarryingItem()
        {
            if (CarryingItem == null)
            {
                return false;
            }

            if (colony.TryGetNearbyEmptyCell(GridX, GridY, out Cell? nearbyCell))
            {
                CarryingItem.SetPosition(nearbyCell.CenterX, nearbyCell.CenterY);
                nearbyCell.Item = CarryingItem;
                CarryingItem = null;

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 運んでいるアイテムを消滅させる
        /// </summary>
        public void DiscardCarryingItem()
        {
            if (CarryingItem == null)
            {
                return;
            }

            colony.RemoveItem(CarryingItem);
            CarryingItem = null;
        }
    }

    public interface IAntPeek
    {
        public AntKind Kind { get; }
        public float X { get; }
        public float Y { get; }
        public float Rotation { get; }
    }
}