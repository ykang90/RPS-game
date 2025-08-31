using System;
using System.Collections.Generic;
using CARVES.Core;

// ── 事务环境：记录“变更键”与“已排队的事件” ─────────────
internal enum ScheduledKind { Normal, Key, Force }

internal sealed class TxAmbientContext
{
    public readonly HashSet<(Type type, string key)> Changed;
    public readonly Dictionary<Type, List<ScheduledItem>> Scheduled = new();

    public TxAmbientContext(HashSet<(Type,string)> changed) => Changed = changed;

    public sealed class ScheduledItem
    {
        public readonly string Key;
        public readonly Func<IEvent> Factory;
        public readonly ScheduledKind Kind;
        public ScheduledItem(string key, Func<IEvent> factory, ScheduledKind kind)
        { Key = key; Factory = factory; Kind = kind; }
    }
}
internal static class TxAmbient
{
    [ThreadStatic] public static TxAmbientContext Current;

    public static void Enter(HashSet<(Type,string)> changed)
        => Current = new TxAmbientContext(changed);

    public static void Exit() => Current = null;
}
