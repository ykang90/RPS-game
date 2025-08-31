using CARVES.Core;
using System;
using System.Runtime.CompilerServices;

/// <summary>
/// Record基类
/// </summary>
public abstract class CarvesRecordBase : ICarvesRecord
{
    public virtual void Emit(EmitContext context) => EmitScheduler.FlushTo(context, GetType());
    public virtual void Dispose() => Carves.Remove(this);

    // 默认：用调用方法名作为细粒度 key（SetUp/Npc_Add/...）
    protected void Publish(Func<IEvent> factory, [CallerMemberName] string methodName = null)
        => EmitScheduler.ScheduleKey(GetType(), Sanitize(methodName), factory);

    // 带后缀：形成 MethodName:Suffix（如 NpcFavor_Update:42）
    protected void Publish(Func<IEvent> factory, object suffix, [CallerMemberName] string methodName = null)
        => EmitScheduler.ScheduleKey(GetType(), $"{Sanitize(methodName)}:{suffix}", factory);

    // 显式指定 key（极少用：跨多个内部帮助方法统一成一个 key 时）
    protected void PublishKey(string key, Func<IEvent> factory)
        => EmitScheduler.ScheduleKey(GetType(), key, factory);

    // 粗粒度：不分 key；只要本 Record 在本事务被写过就发（简单 Record 用它最省事）
    protected void PublishAll(Func<IEvent> factory)
        => EmitScheduler.Schedule(GetType(), factory);

    // 强制：无条件发（基础设施/被动广播，谨慎使用）
    protected void PublishForce(Func<IEvent> factory, string reason = null)
        => EmitScheduler.ScheduleForce(GetType(), factory);

    static string Sanitize(string key) => string.IsNullOrEmpty(key) ? "__Auto" : key;
}

/// <summary>
/// 发射接口
/// </summary>
public interface IRecordEmitter
{
    void Emit(EmitContext context);
}

/// <summary>
/// Record 统一接口：直接继承 IRecordEmitter（还保留 IDisposable）
/// </summary>
public interface ICarvesRecord : IRecordShard, IRecordEmitter, IDisposable { }