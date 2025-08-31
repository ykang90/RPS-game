using System;
using System.Collections.Generic;
using UnityEngine;

namespace CARVES.Core
{
    /// <summary>
    /// 同帧事件总线：挂在场景中的单例物体即可。
    /// </summary>
    public sealed class FrameEventBus : MonoBehaviour, IEventBus
    {
        public const int MaxDepth = 8;
        private int _depth;                 // 递归深度保护
        private readonly Queue<IEvent> _queue = new();
        private readonly Dictionary<Type, List<ISubscription>> _subs = new();

        #region Mono
        private void Update()
        {
            int frameCount = _queue.Count;
            for (int i = 0; i < frameCount; i++)
            {
                var evt = _queue.Dequeue();
                Dispatch(evt);
            }
        }
        #endregion

        #region Publish / Subscribe
        public void Publish<TEvent>(TEvent evt, string callerMember = "", string callerFile = "", int callerLine = 0) where TEvent : IEvent
        {
            EventMeta.Inject(evt, callerMember, callerFile, callerLine);

            if (_depth >= MaxDepth)
            {
                Debug.LogError($"[FrameEventBus] Depth overflow (>{MaxDepth}) – {typeof(TEvent).Name}");
                return;
            }
            _queue.Enqueue(evt);
        }

        public IEventSubscription Subscribe<TEvent>(Action<TEvent> handler, int order = 0) where TEvent : IEvent
        {
            var sub = new Subscription<TEvent>(handler, order, Unsubscribe);
            var list = GetOrCreateList(typeof(TEvent));
            list.Add(sub);
            list.Sort((a, b) => a.Order.CompareTo(b.Order));
            return sub;
        }
        #endregion

        #region 内部实现
        private List<ISubscription> GetOrCreateList(Type t)
        {
            if (!_subs.TryGetValue(t, out var list))
            {
                list = new List<ISubscription>();
                _subs[t] = list;
            }
            return list;
        }

        private void Unsubscribe<TEvent>(Subscription<TEvent> sub) where TEvent : IEvent
        {
            if (_subs.TryGetValue(typeof(TEvent), out var list))
                list.Remove(sub);
        }

        private void Dispatch(IEvent evt)
        {
            _depth++;
            try
            {
                if (_subs.TryGetValue(evt.GetType(), out var list))
                {
                    // 拷贝避免订阅者内增删影响当次遍历
                    var snapshot = list.ToArray();
                    foreach (var sub in snapshot)
                    {
                        try { sub.Invoke(evt); }
                        catch (Exception ex)
                        {
                            Debug.LogException(new EventDispatchException(evt, ex));
                        }
                    }
                }
            }
            finally { _depth--; }
        }
        #endregion
    }
}