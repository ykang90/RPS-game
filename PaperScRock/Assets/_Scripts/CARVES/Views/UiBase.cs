using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using CARVES.Core;
using CARVES.Utls;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CARVES.Views
{
    /// 标记需要自动绑定的 UI 控件
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class UiBindAttribute : Attribute
    {
        /// GameObject 路径或名字；null = 用字段/属性名
        public string Path { get; }
        public UiBindAttribute(string path = null) => Path = path;
    }
    /// 标记需要自动绑定的 ListView 控件
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ListViewBindAttribute : Attribute
    {
        public string Prefab { get; }
        public string ParentComponent { get; }
        public bool Display { get; }
        public bool UsePool { get; } 
        public bool HideChildrenViews { get; }
        /// GameObject 路径或名字；null = 用字段/属性名
        public string Path { get; }
        public ListViewBindAttribute(string prefab, string parentComponent, bool display, bool usePool=true, bool hideChildrenViews=true, string path = null)
        {
            Prefab = prefab;
            ParentComponent = parentComponent;
            Display = display;
            UsePool = usePool;
            HideChildrenViews = hideChildrenViews;
            Path = path;
        }
    }

    public abstract class UiBase : IUiBase
    {
        public GameObject GameObject { get; }
        public Transform Transform { get; }
        public RectTransform RectTransform { get; }
        IView _v;

        public UiBase(IView v, bool display = true)
        {
            _v = v ?? throw new ArgumentNullException($"{GetType().Name}: view = null!");
            GameObject = v.GameObject;
            Transform = v.GameObject.transform;
            RectTransform = v.RectTransform;
            GameObject.SetActive(display);

            UiBindReflectionHelper.Bind(this, _v);
            Carves.Inject(this);
        }

        /// <summary>
        /// 当ui显示触发器
        /// </summary>
        protected virtual void OnUiShow() { }
        /// <summary>
        /// 当ui隐藏触发器
        /// </summary>
        protected virtual void OnUiHide() { }
        public View GetView() => _v.GetView();
        public event UnityAction OnDestroyEvent;
        public void Show() => Display(true);
        public void Hide() => Display(false);

        public void Display(bool display)
        {
            if (display) OnUiShow();
            else OnUiHide();
            GameObject.SetActive(display);
        }

        public virtual void ResetUi() { }

        public Coroutine StartCoroutine(IEnumerator enumerator) => _v.StartCo(enumerator);
        public void StopCoroutine(Coroutine coroutine) => _v.StopCo(coroutine);
        public void StopAllCoroutines() => _v.StopAllCo();

        public void Destroy() => Object.Destroy(GameObject);

        protected void Log(string msg = null) => Debug.Log($"{GameObject.name}: {msg}", GameObject);

        protected void LogEvent(string msg = null, [CallerMemberName] string methodName = null) =>
            Debug.Log($"{GameObject.name}.{methodName}() {msg}", GameObject);
        protected void LogError(string msg = null) => Debug.LogError($"{GameObject.name}: {msg}", GameObject);
        protected void LogWarning(string msg = null) => Debug.LogWarning($"{GameObject.name}: {msg}", GameObject);
        protected void LogException(Exception e) => Debug.LogException(e, GameObject);
    }

    /// <summary>
    /// 用于管理列表的ui, 但是不包含滚动条<br/>
    /// 要使用滚动条, 请使用<see cref="ListView_Scroll{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ListView_Trans<T> : UiBase where T : UiBase
    {
        List<T> _list { get; } = new List<T>();
        public IReadOnlyList<T> List => _list;
        public int Count => _list.Count;
        public View Prefab { get; }
        public Transform Content { get; }
        ObjectPool<View> pool;

        public ListView_Trans(IView v, View prefab, Transform content, bool display = true, bool usePool = true,
            bool hideChildrenViews = true) : base(v, display)
        {
            Prefab = prefab;
            Content = content;

            if (usePool)
            {
                pool = new ObjectPool<View>(
                    () => Object.Instantiate(Prefab, Content),
                    obj =>
                    {
                        obj.gameObject.SetActive(true);
                        obj.transform.SetAsLastSibling();
                    },
                    obj => obj.gameObject.SetActive(false),
                    obj => Object.Destroy(obj.gameObject),
                    false, 10, 100 // 可根据需要设置容量
                );
            }

            if (hideChildrenViews) HideChildren();
        }

        public ListView_Trans(IView v, string prefabName, string contentName,bool display = true, bool usePool = true) 
            : this(v, v.Get<View>(prefabName), v.Get<Transform>(contentName), display,usePool) { }

        public ListView_Trans(IView v, string prefabName,bool display = true , bool usePool = true) 
            : this(v, v.Get<View>(prefabName), v.RectTransform, display,usePool) { }

        public void HideChildren()
        {
            foreach (Transform tran in Content)
                tran.gameObject.SetActive(false);
        }

        public T Instance(Func<View, T> func)
        {
            Func<View> onCreateView = pool != null ? pool.Get : () => Object.Instantiate(Prefab, Content);
            return Instance(onCreateView, func);
        }

        T Instance(Func<View> onCreateView, Func<View, T> func)
        {
            var obj = onCreateView();
            var ui = func.Invoke(obj);
            _list.Add(ui);
            return ui;
        }
        
        public void ClearList(Action<T> onRemoveFromList = null)
        {
            foreach (var ui in _list)
            {
                onRemoveFromList?.Invoke(ui);
                if (pool != null)
                    pool.Release(ui.GetView());
                else
                    Object.Destroy(ui.GetView().gameObject);
            }

            _list.Clear();
        }
        
        public void ClearPool(Action<T> onRemoveFromList)
        {
            if (pool == null)
            {
                "未使用对象池，无法清空对象池。".Log(GameObject);
                return;
            }

            foreach (var ui in _list)
            {
                onRemoveFromList?.Invoke(ui);
                pool.Release(ui.GetView());
            }

            _list.Clear();
        }

        public void Remove(T obj)
        {
            if (pool != null)
                pool.Release(obj.GetView());
            else
                Object.Destroy(obj.GetView().gameObject);
            _list.Remove(obj);
        }

        public int IndexOf(T obj)
        {
            if (_list.Contains(obj)) return _list.IndexOf(obj);
            return -1;
        }
    }

    /// <summary>
    /// 滚动条列表ui, 用于管理列表的ui, 包含滚动条<br/>
    /// 如果不用滚动条, 请使用<see cref="ListView_Trans{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ListView_Scroll<T> : ListView_Trans<T> where T : UiBase
    {
        readonly ScrollRect _scrollRect;
        public ScrollRect ScrollRect
        {
            get
            {
                if (_scrollRect == null)
                    throw new InvalidOperationException("如果要调用ScrollRect,请在构造的时候传入scrollrect控件");
                return _scrollRect;
            }
        }
        /// <summary>
        /// 构造一个带滚动条的列表视图<br/>
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="scrollRect"></param>
        /// <param name="v"></param>
        /// <param name="display">一般直接从上层类传入display值就行，主要控制初始是否显示</param>
        /// <param name="usePool"></param>
        /// <param name="hideChildrenViews"></param>
        public ListView_Scroll(View prefab, ScrollRect scrollRect, IView v, bool display,
            bool usePool = true,
            bool hideChildrenViews = true) : base(v, prefab, scrollRect.content, display, usePool)
        {
            _scrollRect = scrollRect;
            if (hideChildrenViews) HideChildren();
        }
        /// <summary>
        /// 构造一个带滚动条的列表视图<br/>
        /// </summary>
        /// <param name="v"></param>
        /// <param name="prefabName"></param>
        /// <param name="scrollRectName"></param>
        /// <param name="display">一般直接从上层类传入display值就行，主要控制初始是否显示</param>
        /// <param name="usePool"></param>
        /// <param name="hideChildrenViews"></param>
        public ListView_Scroll(IView v, string prefabName, string scrollRectName, bool display,
            bool usePool = true,
            bool hideChildrenViews = true) : this(
            v.Get<View>(prefabName),
            v.Get<ScrollRect>(scrollRectName), v, 
            display: display, 
            usePool : usePool,
            hideChildrenViews: hideChildrenViews)
        {
        }

        public void SetVerticalScrollPosition(float value)
        {
            ScrollRect.verticalNormalizedPosition = value;
        }

        public void SetHorizontalScrollPosition(float value)
        {
            ScrollRect.horizontalNormalizedPosition = value;
        }

        public void ScrollRectSetSize(Vector2 size) => ((RectTransform)_scrollRect.transform).sizeDelta = size;

        public void ScrollRectSetSizeX(float x)
        {
            var rect = ((RectTransform)_scrollRect.transform);
            rect.sizeDelta = new Vector2(x, rect.sizeDelta.y);
        }

        public void ScrollRectSetSizeY(float y)
        {
            var rect = ((RectTransform)_scrollRect.transform);
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, y);
        }
        public void DisplayScroll(bool display) => ScrollRect.gameObject.SetActive(display);

        public void HideOptions()
        {
            ScrollRect.gameObject.SetActive(false);
        }

        public void ShowOptions() => ScrollRect.gameObject.SetActive(true);
        public override void ResetUi() => HideOptions();
    }

    /// 只做一件事：把泛型参数透传给 view.Get<T>(string)
    public static class IViewExtensions
    {
        public static T GetGeneric<T>(this IView view, string path) =>
            view.Get<T>(path);
    }

    static class UiBindReflectionHelper
    {
        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        public static void Bind(object target, IView view)
        {
            foreach (var m in target.GetType().GetMembers(Flags))
            {
                object value = null;
                if (TryUiBindAttr(target, view, m, ref value))
                {
                    SetMember(target, m, value);
                    continue;
                }

                if (TryListViewAttr(target, view, m, ref value))
                {
                    SetMember(target, m, value);
                    continue;
                }
            }
        }

        static bool TryListViewAttr(object target, IView view, MemberInfo m, ref object value)
        {
            var attr = m.GetCustomAttribute<ListViewBindAttribute>();
            // ① 没有特性 → 直接跳过
            if (attr == null) return false;

            var memberType = GetMemberType(m);
            if (memberType == null || !memberType.IsGenericType)
            {
                Debug.LogError($"{target.GetType().Name}: {m.Name} 不是有效的 ListView 类型");
                return false;
            }

            var genericDef = memberType.GetGenericTypeDefinition();
            object[] args = null;

            try
            {
                if (genericDef == typeof(ListView_Trans<>))
                {
                    // ListView_Trans：content 可以为空
                    args = string.IsNullOrEmpty(attr.ParentComponent)
                        ? new object[] { view, attr.Prefab, attr.Display, attr.UsePool } // ctor(v,string,bool)
                        : new object[]
                        {
                            view, attr.Prefab, attr.ParentComponent, attr.Display, attr.UsePool
                        }; // ctor(v,string,string,bool)
                }
                else if (genericDef == typeof(ListView_Scroll<>))
                {
                    // ListView_Scroll 需要 prefab + scrollRect 名称
                    if (string.IsNullOrEmpty(attr.ParentComponent))
                    {
                        Debug.LogError($"{target.GetType().Name}: {m.Name} 缺少 scrollRectName");
                        return false;
                    }
                    args = new object[] { view, attr.Prefab, attr.ParentComponent, attr.Display, attr.UsePool, attr.HideChildrenViews };
                }
                else
                {
                    Debug.LogError($"{target.GetType().Name}: {m.Name} 不支持的 ListView 泛型类型");
                    return false;
                }

                // ② 反射创建实例
                value = Activator.CreateInstance(memberType, args);
                if (value != null) return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            Debug.LogError($"{target.GetType().Name}: 无法为 {m.Name} 生成 ListView 实例");
            return false;
        }

        static bool TryUiBindAttr(object target, IView view, MemberInfo m, ref object value)
        {
            var attr = m.GetCustomAttribute<UiBindAttribute>();
            if (attr == null) return false;
            var path = attr.Path ?? m.Name;
            var type = GetMemberType(m);
            value = typeof(IViewExtensions)
                .GetMethod(nameof(IViewExtensions.GetGeneric)) // 调用扩展
                .MakeGenericMethod(type)
                .Invoke(view, new object[] { view, path });

            if (value != null) return true;
            Debug.LogError($"{target.GetType().Name}: 找不到控件 <{path}> Type ={type.Name}");
            return false;
        }

        static Type GetMemberType(MemberInfo mi) => mi switch
        {
            FieldInfo f => f.FieldType,
            PropertyInfo p => p.PropertyType,
            _ => null
        };

        static void SetMember(object obj, MemberInfo mi, object val)
        {
            switch (mi)
            {
                case FieldInfo f:
                    f.SetValue(obj, val);
                    break;

                case PropertyInfo p:
                    // ① 有 setter → 直接赋值
                    if (p.SetMethod != null)
                    {
                        p.SetValue(obj, val);
                        return;
                    }

                    // ② 尝试设置编译器生成的 backing field
                    var backing = obj.GetType()
                        .GetField($"<{p.Name}>k__BackingField",
                            BindingFlags.Instance | BindingFlags.NonPublic);
                    if (backing != null)
                    {
                        backing.SetValue(obj, val);
                        return;
                    }

                    Debug.LogError($"{obj.GetType().Name}: 属性 {p.Name} 没有 setter 且未找到 backing field");
                    break;
            }
        }
    }
}