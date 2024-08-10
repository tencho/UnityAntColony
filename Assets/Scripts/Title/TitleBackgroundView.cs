using AntColony.Title;
using UnityEngine;
using UnityEngine.EventSystems;

public class TitleBackgroundView : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TitleSceneView sceneView;
    public void OnPointerClick(PointerEventData eventData)
    {
        sceneView.Input.Invoke(GameSceneInput.Click(TitleSceneInputClickKind.Background));
    }
}
