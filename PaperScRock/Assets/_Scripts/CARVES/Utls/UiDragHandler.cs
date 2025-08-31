using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace CARVES.Utls
{
    public class UiDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public UnityEvent<PointerEventData> BeginDrag;
        public UnityEvent<PointerEventData> Drag;
        public UnityEvent<PointerEventData> EndDrag;

        public void OnBeginDrag(PointerEventData eventData) => BeginDrag?.Invoke(eventData);
        public void OnDrag(PointerEventData eventData) => Drag?.Invoke(eventData);
        public void OnEndDrag(PointerEventData eventData) => EndDrag?.Invoke(eventData);
    }
}