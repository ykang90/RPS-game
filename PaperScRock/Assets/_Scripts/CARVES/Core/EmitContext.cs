using System;
using System.Collections.Generic;
using System.Linq;
using CARVES.Core;

/// <summary>
/// DomainEvent（对外领域事件）上下文
/// </summary>
public sealed class EmitContext
{
    readonly HashSet<(Type type, string key)> _changed;
    readonly Type _self;
    readonly List<IEvent> _events;

    internal EmitContext(HashSet<(Type,string)> changed, Type self, List<IEvent> events)
    { _changed = changed; _self = self; _events = events; }

    public bool ChangedAny => _changed.Any(p => p.type == _self);
    public bool Changed(string key) => _changed.Contains((_self, key));
    /// <summary>
    /// 非常规：无条件强制发布，（开发期要求附带原因）
    /// </summary>
    /// <param name="factory"></param>
    public void PublishForce(Func<IEvent> factory) => PublishForce(factory());
    /// <summary>
    /// 非常规：无条件强制发布，（开发期要求附带原因）
    /// </summary>
    /// <param name="e"></param>
    public void PublishForce(IEvent e) => _events.Add(e);

    // —— 关键：一行写法 —— 
    /// <summary>
    /// 常规：本 Record 在本次事务里“被写过”才发
    /// </summary>
    /// <param name="e"></param>
    public void Publish(IEvent e)
    { if (ChangedAny) _events.Add(e); }
    /// <summary>
    /// 常规（细粒度）：只有标了 key 才发
    /// </summary>
    /// <param name="key"></param>
    /// <param name="e"></param>
    public void PublishKey(string key, IEvent e)
    { if (Changed(key)) _events.Add(e); }

    /// <summary>
    /// 常规：本 Record 在本次事务里“被写过”才发
    /// 惰性构造，避免未变更时白创建事件
    /// </summary>
    /// <param name="factory"></param>
    public void Publish(Func<IEvent> factory)
    { if (ChangedAny) _events.Add(factory()); }
    /// <summary>
    /// 常规（细粒度）：只有标了 key 才发
    /// 惰性构造，避免未变更时白创建事件
    /// </summary>
    /// <param name="key"></param>
    /// <param name="factory"></param>
    public void PublishKey(string key, Func<IEvent> factory)
    { if (Changed(key)) _events.Add(factory()); }
}