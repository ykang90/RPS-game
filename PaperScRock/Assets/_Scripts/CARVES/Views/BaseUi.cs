using UnityEngine;
using UnityEngine.Events;

namespace CARVES.Views
{
    /// <summary>
    /// UiBase 接口，主要是为了分离<see cref="MonoBehaviour"/>的各种方法封装起来，避免外部调用到
    /// </summary>
    public interface IUiBase
    {
        event UnityAction OnDestroyEvent;
        void Show();
        void Hide();
        void ResetUi();
    }

    /// <summary>
    /// 所有Ui必须继承这个类，
    /// 分别预设了可扩展的显示<see cref="Show"/>与隐藏<see cref="Hide"/>方法。
    /// 和<see cref="Display{T}"/>控制<see cref="Component"/>类型。
    /// 并且子类型需要实现<see cref="ResetUi"/>作为整个控件的重置作用。
    /// </summary>
    public abstract class BaseUi : MonoBehaviour, IUiBase
    {
        public event UnityAction OnDestroyEvent;
        public virtual void Show() => Display(true, this);
        public virtual void Hide() => Display(false, this);

        /// <summary>
        /// 把本地位置归位到 <see cref="Vector3.zero"/>
        /// </summary>
        protected void ResetLocalPos() => transform.localPosition = Vector3.zero;

        /// <summary>
        /// Hide or Display object，uses gameObject.SetActive method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="display"></param>
        /// <param name="objs"></param>
        public void Display<T>(bool display, params T[] objs) where T : Component
        {
            foreach (var o in objs) o.gameObject.SetActive(display);
        }
        protected void DestroyObj(Component com) => Destroy(com.gameObject);
        public abstract void ResetUi();
        void OnDestroy() => OnDestroyEvent?.Invoke();
    }
}