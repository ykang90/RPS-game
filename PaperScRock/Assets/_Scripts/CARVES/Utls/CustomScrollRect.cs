using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CARVES.Utls
{
    public class CustomScrollRect : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        ScrollRect _parentScrollRect;
        ScrollRect _ownScrollRect;
        bool dragHorizontal = false;

        public void Init(ScrollRect parentScrollRect)
        {
            _ownScrollRect = GetComponent<ScrollRect>();
            if(!_ownScrollRect)
                throw new System.Exception("CustomScrollRect Init failed, no ScrollRect component found");
            _parentScrollRect = parentScrollRect;
        }

        ScrollRect GetControlledScrollRect(bool isHorizontal)=> isHorizontal ? _ownScrollRect : _parentScrollRect;
        public void OnBeginDrag(PointerEventData eventData)
        {
            dragHorizontal = Mathf.Abs(eventData.delta.x) > Mathf.Abs(eventData.delta.y);
            GetControlledScrollRect(dragHorizontal).OnBeginDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            GetControlledScrollRect(dragHorizontal).OnDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            GetControlledScrollRect(dragHorizontal).OnEndDrag(eventData);
        }
    }
}

