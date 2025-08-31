using System;
using System.Runtime.CompilerServices;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CARVES.Extensions
{
    public static class UnityEventExtension
    {
        public static void OnClickAdd(this Button btn, UnityAction action) =>
            btn.OnClickAdd(action, true);
        public static void OnClickAdd(this Button btn, Action action, bool removeAllListener = true) =>
            btn.OnClickAdd(new UnityAction(action), removeAllListener);

        public static void OnClickAdd(this Button btn, UnityAction action, bool removeAllListener)
        {
            CheckNull(btn);
            if (removeAllListener) btn.onClick.RemoveAllListeners();
            if (action != null) btn.onClick.AddListener(action);
        }

        static void CheckNull<T>(T obj, [CallerMemberName] string method = null)
        {
            if (obj == null) throw new NullReferenceException($"{method}(): 物件=null!");
        }

        public static void AddAction(this UnityEvent unityEvent, Action action, bool removeAllListener = true)
        {
            CheckNull(unityEvent);
            if (removeAllListener) unityEvent.RemoveAllListeners();
            if (action != null) unityEvent.AddListener(new UnityAction(action));
        }

        public static void AddAction(this UnityEvent<string> unityEvent, Action<string> action, bool removeAllListener = true)
        {
            CheckNull(unityEvent);
            if (removeAllListener) unityEvent.RemoveAllListeners();
            if (action != null) unityEvent.AddListener(new UnityAction<string>(action));
        }
        public static void AddAction(this UnityEvent<int> unityEvent, Action<int> action, bool removeAllListener = true)
        {
            CheckNull(unityEvent);
            if (removeAllListener) unityEvent.RemoveAllListeners();
            if (action != null) unityEvent.AddListener(new UnityAction<int>(action));
        }
        public static void AddAction(this UnityEvent<float> unityEvent, Action<float> action, bool removeAllListener = true)
        {
            CheckNull(unityEvent);
            if (removeAllListener) unityEvent.RemoveAllListeners();
            if (action != null) unityEvent.AddListener(new UnityAction<float>(action));
        }
    }
}