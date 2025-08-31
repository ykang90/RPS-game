using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CARVES.Utls;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CARVES.Views
{
    /// <summary>
    /// 挂在Ui父件的身上的整合插件
    /// </summary>
    public class View : MonoBehaviour, IView
    {
        [SerializeField] GameObject[] _components;
        [SerializeField] ResObj Resources;
        RectTransform _rectTransform;
        public event Action OnDisableEvent;
        public event Action OnEnableEvent;

        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform is null)
                {
                    _rectTransform = _rectTransform = GetComponent<RectTransform>();
                }

                return _rectTransform;
            }
        }

        public View GetView() => this;
        public Object GetRes(string resName) => Resources.GetRes(resName);
        public T GetRes<T>(int index) where T : Object => Resources.GetRes<T>(index);
        public T GetRes<T>(string resName) where T : Object => Resources.GetRes<T>(resName);
        public void PrintComponents()
        {
            foreach (var component in _components)
                $"Com = {component?.name}".Log(this);
        }

        public IReadOnlyDictionary<string, GameObject> GetMap() => _components.ToDictionary(c => c.name, c => c);
        public GameObject GameObject => gameObject;
        public GameObject[] GetObjects() => _components.ToArray();
        public GameObject Get(string objName)
        {
            var obj = FindComponent(objName);
            if (!obj) LogException($"找不到物件名：{objName}");
            return obj;

            GameObject FindComponent(string comName)
            {
                if (_components.Any(c => c.IsUnityNull()))
                    LogException("存在空引用！");
                return _components.FirstOrDefault(c => c.name == comName);
            }
        }
        void LogException(string msg) => Debug.LogException(new ViewException($"View.{name} 控件异常：" + msg), this);
        public T Get<T>(string objName) => Get(objName).GetComponent<T>();

        public T Get<T>(int index)
        {
            if (_components.Length >= index)
            {
                LogException("索引超出范围：" + index);
                return default;
            }
            var obj = _components[index].GetComponent<T>();
            if (obj != null) return obj;
            LogException("找不到索引：" + index);
            return default;
        }

        void OnDisable() => OnDisableEvent?.Invoke();
        void OnEnable() => OnEnableEvent?.Invoke();
        #region ForDebug
        //void OnEnable()
        //{
        //}
        #endregion        
        public Coroutine StartCo(IEnumerator enumerator) => StartCoroutine(enumerator);
        public void StopCo(Coroutine coroutine) => StopCoroutine(coroutine);
        public void StopAllCo() => StopAllCoroutines();

        public void SetWidth(float width) => RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        public void SetHeight(float height)=> RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        public void SetSize(float width, float height) => RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

        [Serializable]
        class ResObj
        {
            public Object[] Objs;

            public T GetRes<T>(int index) where T : Object => Objs[index] as T;
            public Object GetRes(string resName) => Objs.FirstOrDefault(o => o.name == resName);
            public T GetRes<T>(string resName) where T : Object => GetRes(resName) as T;
        }

        class ViewException : Exception { public ViewException(string message):base(message) { } }
    }
}
