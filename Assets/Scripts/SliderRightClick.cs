using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SliderRightClick : MonoBehaviour, IPointerClickHandler
{
    public UnityEvent onRightClick;
    public Slider slider;
    public float defaultValue;
    public void OnPointerClick(PointerEventData data) {
        if (data.button == PointerEventData.InputButton.Right) {
            slider.value = defaultValue;
            onRightClick.Invoke();
        }
    }
}