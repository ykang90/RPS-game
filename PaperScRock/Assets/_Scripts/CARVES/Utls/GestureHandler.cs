using UnityEngine;
using UnityEngine.Events;

namespace CARVES.Utls
{
    public class GestureHandler : MonoBehaviour
    {
        public UnityEvent<Vector2> OnDrag;          // 拖动事件
        public UnityEvent<float> OnZoom;            // 缩放事件
        public UnityEvent OnDragStart;              // 拖动开始事件
        public UnityEvent OnDragEnd;                // 拖动结束事件
        public UnityEvent OnZoomStart;              // 缩放开始事件
        public UnityEvent OnZoomEnd;                // 缩放结束事件

        float _initialTouchDistance;
        bool _isZooming;
        bool _isDragging;

        void Update()
        {
            var touchCount = Input.touchCount;

            switch (touchCount)
            {
                case 1:
                    HandleSingleTouch(Input.GetTouch(0));
                    break;
                case 2:
                    HandlePinchZoom(Input.GetTouch(0), Input.GetTouch(1));
                    break;
                default:
                {
                    if (_isDragging)
                    {
                        OnDragEnd?.Invoke();
                        _isDragging = false;
                    }
                    if (_isZooming)
                    {
                        OnZoomEnd?.Invoke();
                        _isZooming = false;
                    }
                    break;
                }
            }
        }

        void HandleSingleTouch(Touch touch)
        {
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    OnDragStart?.Invoke();
                    _isDragging = true;
                    break;
                case TouchPhase.Moved:
                    if (_isDragging) OnDrag?.Invoke(touch.deltaPosition);
                    break;
                case TouchPhase.Ended:
                    if (_isDragging)
                    {
                        OnDragEnd?.Invoke();
                        _isDragging = false;
                    }
                    break;
            }
        }

        void HandlePinchZoom(Touch touch0, Touch touch1)
        {
            if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
            {
                _initialTouchDistance = Vector2.Distance(touch0.position, touch1.position);
                OnZoomStart?.Invoke();
                _isZooming = true;
            }
            else if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
            {
                if (!_isZooming) return;
                var currentDistance = Vector2.Distance(touch0.position, touch1.position);
                var scaleFactor = (currentDistance - _initialTouchDistance) / _initialTouchDistance;
                OnZoom?.Invoke(scaleFactor);
            }
            else if (_isZooming && (touch0.phase == TouchPhase.Ended || touch1.phase == TouchPhase.Ended))
            {
                OnZoomEnd?.Invoke();
                _isZooming = false;
            }
        }
    }
}
