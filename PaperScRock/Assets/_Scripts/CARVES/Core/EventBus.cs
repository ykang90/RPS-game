// LeoGameFramework – CARVES
// ------------------------------------------------------------
// FrameEventBus  (主线程、同帧、顺序保证)
// ------------------------------------------------------------
//  设计要点
//  ● 单线程 FIFO：保证游戏逻辑确定性；每帧只消费入队时刻 ≤ 本帧开始的事件。
//  ● TraceId / CorrelationId / Caller 自动注入，方便链路追踪。
//  ● 订阅优先级 (order) + CompositeSubscription 统一取消。
//  ● DepthGuard 防御事件递归导致的栈溢出 (MaxDepth)。
//  ● 无反射 / dynamic：采用 ISubscription 接口 + 显式强转，性能友好。
// ------------------------------------------------------------
//  使用示例
//  -------------------------------------------
//  public sealed class PlayerSpawnEvent : EventBase { public int PlayerId; }
//  
//  void Awake()
//  {
//      var bus = FindObjectOfType<FrameEventBus>();
//      bus.Subscribe<PlayerSpawnEvent>(OnPlayerSpawn);
//      bus.Publish(new PlayerSpawnEvent { PlayerId = 99 });
//  }
//  
//  void OnPlayerSpawn(PlayerSpawnEvent e) => Debug.Log($"Spawn {e.PlayerId}");
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CARVES.Utls;

namespace CARVES.Core
{
    #region 基础契约

    /// <summary>
    /// 事件接口，包含链路追踪字段与元数据。
    /// </summary>
    public interface IEvent
    {
        Guid TraceId { get; set; }
        Guid CorrelationId { get; set; }
        IDictionary<string, object> Metadata { get; }
    }

    /// <summary>
    /// 可以直接继承的简单基类。
    /// </summary>
    public abstract class EventBase : IEvent
    {
        public Guid TraceId { get; set; }
        public Guid CorrelationId { get; set; }
        public IDictionary<string, object> Metadata { get; } = new Dictionary<string, object>();
    }
    public abstract record EventBaseRec : IEvent
    {
        public Guid TraceId { get; set; }
        public Guid CorrelationId { get; set; }
        public IDictionary<string, object> Metadata { get; } = new Dictionary<string, object>();
    }

    /// <summary>取消订阅句柄。</summary>
    public interface IEventSubscription : IDisposable { }

    /// <summary>事件总线 API。</summary>
    public interface IEventBus
    {
        IEventSubscription Subscribe<TEvent>(Action<TEvent> handler, int order = 0) where TEvent : IEvent;
    }

    #endregion

    #region 内部工具

    internal static class EventMeta
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Inject<T>(T evt, string callerMember, string callerFile, int callerLine) where T : IEvent
        {
            if (evt.TraceId == Guid.Empty) evt.TraceId = Guid.NewGuid();
            if (evt.CorrelationId == Guid.Empty) evt.CorrelationId = evt.TraceId;
            evt.Metadata["Caller"] = callerMember;
            evt.Metadata["File"] = callerFile;
            evt.Metadata["Line"] = callerLine.ToString();
            evt.Metadata["Location"] = System.IO.Path.GetFileName(callerFile) + ":" + callerLine;
        }
    }

    /// <summary>内部统一调度接口，避免 dynamic / 反射。</summary>
    internal interface ISubscription
    {
        int Order { get; }
        void Invoke(IEvent evt);
        bool Matches(Type eventType);
    }

    internal sealed class Subscription<TEvent> : ISubscription, IEventSubscription where TEvent : IEvent
    {
        private readonly Action<TEvent> _handler;
        private readonly Action<Subscription<TEvent>> _onDispose;
        public int Order { get; }
        private bool _disposed;

        public Subscription(Action<TEvent> handler, int order, Action<Subscription<TEvent>> onDispose)
        {
            _handler = handler;
            Order = order;
            _onDispose = onDispose;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(IEvent evt) => _handler((TEvent)evt);

        public bool Matches(Type eventType) => eventType == typeof(TEvent);

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _onDispose(this);
        }
    }

    #endregion

    #region 例外包装 & 复合订阅

    public sealed class EventDispatchException : Exception
    {
        public IEvent Event { get; }
        public EventDispatchException(IEvent evt, Exception inner)
            : base($"[EventBus] 处理 {evt.GetType().Name} 失败  Trace={evt.TraceId}  Caller={evt.Metadata.GetOrDefault("Caller", "?")}", inner)
        {
            Event = evt;
        }
    }

    /// <summary>
    /// 组合订阅，方便集中释放。
    /// </summary>
    public sealed class CompositeSubscription : IEventSubscription
    {
        private readonly List<IEventSubscription> _subs = new();
        private bool _disposed;
        public void Add(IEventSubscription sub)
        {
            if (_disposed) { sub.Dispose(); return; }
            _subs.Add(sub);
        }
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            foreach (var s in _subs) s.Dispose();
            _subs.Clear();
        }
    }

    #endregion
}