using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class HoverEvent : MonoBehaviour, IPointerEnterHandler
{
    public UnityEvent onPointerEnter;

    public void OnPointerEnter(PointerEventData pointerEventData) {
        onPointerEnter.Invoke();
    }
}