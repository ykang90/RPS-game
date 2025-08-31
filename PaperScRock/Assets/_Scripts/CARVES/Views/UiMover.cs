using System;
using System.Collections;
using UnityEngine;

namespace CARVES.Views
{
    /// <summary>
    /// 移动UI, 主要用于<see cref="InputFieldUi"/>>,
    /// 或是其它对位置调整有需求的Ui,
    /// 针对硬件把当屏幕键盘弹出时, InputField 会被遮挡的问题.
    /// </summary>
    public class UiMover : MonoBehaviour
    {
        [SerializeField] float _shake = 0.1f;
        RectTransform rectTransform;
        Vector2 _originalPosition;
        bool _isMoving;
        public bool IsOriginalPosition => rectTransform.anchoredPosition == _originalPosition;

        void Start()
        {
            rectTransform = (RectTransform)transform;
            _originalPosition = rectTransform.anchoredPosition;
        }

        /// <summary>
        /// 注意<see cref="callbackAction"/>返回是否执行成功, 如果是false, 则表示当前正在移动中, 无法执行移动.
        /// </summary>
        /// <param name="movePosition"></param>
        /// <param name="duration"></param>
        /// <param name="forceNow"></param>
        /// <param name="callbackAction">true = 执行成功, false = 取消执行</param>
        public void Move(Vector2 movePosition, float duration, bool forceNow = false, Action<bool> callbackAction = null)
        {
            if (forceNow) ForceStopCoroutine();
            StartCoroutine(Shake(movePosition, duration, callbackAction));
        }

        void ForceStopCoroutine()
        {
            StopAllCoroutines();
            _isMoving = false;
        }
        /// <summary>
        /// 注意<see cref="callbackAction"/>返回是否执行成功, 如果是false, 则表示当前正在移动中, 无法执行移动.
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="forceNow"></param>
        /// <param name="callbackAction">true = 执行成功, false = 取消执行</param>
        public void MoveOrigin(float duration, bool forceNow = false, Action<bool> callbackAction = null)
        {
            if (forceNow) ForceStopCoroutine();
            StartCoroutine(Shake(_originalPosition, duration, callbackAction));
        }

        public void ResetPosition()
        {
            StopAllCoroutines();
            _isMoving = false;
            rectTransform.anchoredPosition = _originalPosition;
        }

        //抖动(0.1秒)后才判断需不需要移动
        IEnumerator Shake(Vector2 movePosition, float duration, Action<bool> callbackAction)
        {
            if (rectTransform.anchoredPosition == movePosition || _isMoving)
            {
                callbackAction?.Invoke(false);
                yield break;
            }
            var time = 0f;
            while (time < _shake)
            {
                time += Time.fixedDeltaTime;
                yield return null;
            }
            yield return MoveRectTransform(movePosition, duration, () => callbackAction?.Invoke(true));
        }

        IEnumerator MoveRectTransform(Vector2 pos, float duration, Action callbackAction)
        {
            _isMoving = true;
            var time = 0f;
            var originalPosition = rectTransform.anchoredPosition;
            while (time < duration)
            {
                time += Time.fixedDeltaTime;
                rectTransform.anchoredPosition = Vector2.Lerp(originalPosition, pos, time / duration);
                yield return null;
            }

            rectTransform.anchoredPosition = pos;
            _isMoving = false;
            callbackAction?.Invoke();
        }
    }
}