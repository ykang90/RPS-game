using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CARVES.Abstracts;
using CARVES.Core;
using CARVES.Utls;

namespace CARVES.Core
{
    /// <summary>
    ///Record 根对象（权威数据 + 唯一事件出口）<br/>
    ///DomainEvent（对外领域事件）：只允许由 TxRecord.Commit() 统一发布。<br/>
    ///Record.Emit 只“决定发不发、发什么”，不直接碰 EventBus。<br/>
    ///Execute/Module 层不直接发布 DomainEvent，而是改数据 → 让 Record.Emit 决定。<br/>
    ///LocalSignal（本地信号）：Actor/Registry/Module 内部用，不要上 EventBus。<br/>
    /// </summary>
    [CarvesShared]
    public sealed class TxRecord : ITxRecord
    {
        // 版本与帧
        public long Version { get; private set; }
        public int Frame { get; private set; }

        readonly ITimeSource _time;
        readonly IEventBus _bus; // 你现有的事件总线
        readonly Dictionary<Type, IRecordShard> _shards = new();

        bool _txnOpen; // 禁嵌套

        public TxRecord(ITimeSource time, IEventBus bus, params IRecordShard[] shards)
        {
            _time = time ?? throw new ArgumentNullException(nameof(time));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            foreach (var s in shards) _shards[s.GetType()] = s;
        }

        public TShard Get<TShard>() where TShard : class, IRecordShard =>
            _shards.TryGetValue(typeof(TShard), out var s) ? (TShard)s : null;

        public IRecordTxn BeginTxn(Guid causeId = default)
        {
            if (causeId == Guid.Empty)
                causeId = Guid.NewGuid(); // 自动生成一个新的 CauseId
            if (!_txnOpen && !GameRecordGuard.CanBeginTxn())
                throw new InvalidOperationException("[RecordTxn] 非法层尝试 BeginTxn");
            if (_txnOpen) throw new InvalidOperationException("[RecordTxn] 禁止嵌套事务");
            _txnOpen = true;

            return new Txn(this, causeId);
        }

        // 内部 Txn
        sealed class Txn : IRecordTxn
        {
            readonly TxRecord _root;
            readonly Dictionary<Type, object> _shardCopies = new(); // 分片的“工作副本”
            readonly HashSet<(Type shard, string key)> _changed = new();

            bool _committed, _disposed;
            long _ver;
            int _frame;

            public Txn(TxRecord root, Guid causeId)
            {
                _root = root;
                CauseId = causeId;
                TxAmbient.Enter(_changed);
            }

            public long Version => _ver;
            public int Frame => _frame;
            public Guid CauseId { get; }

            public void Touch<TShard>(string key = null) where TShard : class, IRecordShard
            {
                MarkChanged<TShard>(key);
            }

            public void Update<TShard>(Action<TShard> mutate, string key) where TShard : class, IRecordShard
            {
                Update(mutate);
                MarkChanged<TShard>(key);
            }

            public void Update<TShard>(Action<TShard> mutate) where TShard : class, IRecordShard
            {
                if (_disposed) throw new ObjectDisposedException(nameof(Txn));
                if (_committed) throw new InvalidOperationException("[RecordTxn] 已提交");

                // ① 取“工作副本”
                if (!_shardCopies.TryGetValue(typeof(TShard), out var copyObj))
                {
                    var original = _root.Get<TShard>();

                    // 这里采用“原地写 + 变更合并标记”的轻量策略
                    // 若你需要强隔离，可改成深拷贝后写入，Commit 再替换回去。
                    copyObj = original ??
                              throw new InvalidOperationException($"[RecordTxn] 未注册分片 {typeof(TShard).Name}");
                    _shardCopies[typeof(TShard)] = copyObj;
                }

                mutate((TShard)copyObj);
                _changed.Add((typeof(TShard), null)); // 粗粒度标记；细粒度可在下行：
                // MarkChanged<TShard>("SomeKey");
            }

            public void MarkChanged<TShard>(string key = null) where TShard : class, IRecordShard
            {
                _changed.Add((typeof(TShard), key));
            }

            public void Commit()
            {
                if (_disposed) throw new ObjectDisposedException(nameof(Txn));
                if (_committed) throw new InvalidOperationException("[RecordTxn] 不可重复提交");
                if (!GameRecordGuard.CanCommit()) throw new InvalidOperationException("[RecordTxn] 非法层尝试 Commit");

                // 版本 & 帧
                _frame = _root._time.Frame;
                _ver = ++_root.Version;
                _root.Frame = _frame;

                // （如采用深拷贝工作副本，这里替换回正本）

                _committed = true;
                _root._txnOpen = false;

                // ① 让各 Shard 就近产出“业务领域事件”
                var events = new List<IEvent>(8);
                foreach (var kv in _root._shards)
                {
                    if (kv.Value is IRecordEmitter emitter)
                        emitter.Emit(new EmitContext(_changed, kv.Key, events));
                }

                // ② 统一发布（不发布 RecordCommitted）
                foreach (var ev in events)
                    _root._bus.PublishRuntime(ev);
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;

                // 未提交即离开 → 视为回滚（不做任何外部可见变更）
                if (!_committed)
                    _root._txnOpen = false;
                TxAmbient.Exit();
            }
        }
    }

    // ───────────────────────────────
    // Shared: 只读接口与通用头
    // ───────────────────────────────
    public interface IRecordShard
    {
    } // 各分片（如 PlayerShard, BattleShard）实现它

    public interface IReadOnlyTxRecord
    {
        long Version { get; } // 最近一次提交后的版本
        int Frame { get; } // 最近一次提交的帧号
    }

    public interface ITxRecord : IReadOnlyTxRecord
    {
        IRecordTxn BeginTxn(Guid causeId);
        TShard Get<TShard>() where TShard : class, IRecordShard; // 读侧查分片
    }

    public interface IRecordTxn : IDisposable
    {
        long Version { get; } // 提交后才有效（提交前为 0）
        int Frame { get; } // 同上
        Guid CauseId { get; }
        // 在“写窗口”里对目标分片做变更，框架负责快照/合并/变更日志
        void Update<TShard>(Action<TShard> mutate) where TShard : class, IRecordShard;

        // 允许写“逻辑事件摘要”（可选）。也可以只靠自动差异生成。
        void MarkChanged<TShard>(string key = null) where TShard : class, IRecordShard;

        // 尝试提交（成功：分配 Version & 发布一次事件；失败：抛异常并回滚）
        void Commit();
    }

    // 事件载体（仅示意，你可以扩展为更细的 DataChanged/AggregateChanged…）
    public readonly struct RecordCommitted
    {
        public readonly long Version;
        public readonly int Frame;
        public readonly Guid CauseId;
        public readonly IReadOnlyList<ChangeItem> Changes; // 合并后的“变更摘要”

        public RecordCommitted(long v, int f, Guid c, IReadOnlyList<ChangeItem> ch)
        {
            Version = v;
            Frame = f;
            CauseId = c;
            Changes = ch;
        }

        public readonly struct ChangeItem
        {
            public readonly Type ShardType;
            public readonly string Key; // 可为空，代表该分片整体摘要更新

            public ChangeItem(Type t, string k)
            {
                ShardType = t;
                Key = k;
            }

            public override string ToString() => $"{ShardType.Name}#{Key}";
        }
    }

    // 时间与守卫（可接你的现有 EventBus/Guard）
    public interface ITimeSource
    {
        int Frame { get; }
    }

    public static class ExecuteScope
    {
        [ThreadStatic] static int _depth;
        public static bool Active => _depth > 0;

        public readonly ref struct Token
        {
            public void Dispose() => _depth--;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Token Enter() { _depth++; return new Token(); }
    }

    public static class GameRecordGuard
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CanBeginTxn() => ExecuteScope.Active;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CanCommit()   => ExecuteScope.Active;
    }
    public sealed class TxnBuilder : IDisposable
    {
        readonly IRecordTxn _tx;

        internal TxnBuilder(ITxRecord record, Guid causeId)
            => _tx = record.BeginTxn(causeId);

        public TxnBuilder Update<TShard>(Action<TShard> edit, string key = null)
            where TShard : class, IRecordShard
        {
            _tx.Update(edit);
            if (key != null) _tx.MarkChanged<TShard>(key);
            return this;
        }

        public TxnBuilder Mark<TShard>(string key = null)
            where TShard : class, IRecordShard
        {
            _tx.MarkChanged<TShard>(key);
            return this;
        }

        /// <summary>提交并返回本次版本号（一次性发布“业务事件”）。</summary>
        public long Publish()
        {
            _tx.Commit();
            return _tx.Version;
        }

        public void Dispose() => _tx?.Dispose();

    }

    public static class RecordTxnBuilderExt
    {
        /// <summary>最短写法：把 using / Commit 都包起来。</summary>
        public static long Do(this ITxRecord record, Action<IRecordTxn> body, Guid? causeId = null)
        {
            using var _ = ExecuteScope.Enter();
            using var tx = record.BeginTxn(causeId ?? Guid.NewGuid());
            body(tx);
            if (tx.Version == 0) tx.Commit();      // 只在未提交时提交
            return tx.Version;
        }

        /// <summary>
        /// 一步到位：BeginTxn → 编辑分片 → (可选)细粒度标记 → Commit。
        /// 返回本次提交的 Version。调用端只写一个 lambda。
        /// </summary>
        public static long TxUpdate<TShard>(this ITxRecord record, Action<TShard> edit, string key = null, Guid? causeId = null)
            where TShard : class, IRecordShard
        {
            using var _ = ExecuteScope.Enter();
            using var tx = record.BeginTxn(causeId ?? Guid.NewGuid());
            tx.Update(edit);
            if (key != null) tx.MarkChanged<TShard>(key);
            tx.Commit();
            return tx.Version;
        }

        public static TResult TxUpdate<TShard, TResult>(this ITxRecord record, Func<TShard, TResult> edit, out long version, string key = null, Guid? causeId = null)
            where TShard : class, IRecordShard
        {
            using var _ = ExecuteScope.Enter();
            using var tx = record.BeginTxn(causeId ?? Guid.NewGuid());
            TResult result = default;
            tx.Update<TShard>(s => result = edit(s));
            if (key != null) tx.MarkChanged<TShard>(key);
            tx.Commit();
            version = tx.Version;
            return result;
        }
        /// <summary>
        /// 不改数据，只发通知（例如触发重排/重算等）。
        /// </summary>
        public static long TxMark<TShard>(this ITxRecord record, string key = null, Guid? causeId = null)
            where TShard : class, IRecordShard
        {
            using var _ = ExecuteScope.Enter();
            using var tx = record.BeginTxn(causeId ?? Guid.NewGuid());
            tx.MarkChanged<TShard>(key);
            tx.Commit();
            return tx.Version;
        }
    }


    // ── 调度器：Record 方法调用这些 API 来“挂起事件” ─────────────
    internal enum DropPolicy
    {
        DropNewest,
        DropOldest,
        DropByKeyKeepLast
    }

    internal static class EmitScheduler
    {
        /// <summary>同一事务内，同一个 Record 的排队上限。</summary>
        public static int MaxQueuedPerRecord = 4096;

        /// <summary>溢出时的策略：丢最新 / 丢最旧 / 同 key 覆盖（更友好）。</summary>
        public static DropPolicy OverflowPolicy = DropPolicy.DropByKeyKeepLast;

        /// <summary>是否对“带 key 的事件”做去重（同事务内同 key 只保留最后一次）。</summary>
        public static bool CoalesceKeyed = true;

        public static void Schedule(Type ownerType, Func<IEvent> factory)
            => Schedule(ownerType, null, factory, ScheduledKind.Normal);

        public static void ScheduleKey(Type ownerType, string key, Func<IEvent> factory)
            => Schedule(ownerType, key, factory, ScheduledKind.Key);

        public static void ScheduleForce(Type ownerType, Func<IEvent> factory)
            => Schedule(ownerType, null, factory, ScheduledKind.Force);

        static void Schedule(Type t, string key, Func<IEvent> factory, ScheduledKind kind)
        {
            var amb = TxAmbient.Current
                      ?? throw new InvalidOperationException("[Emit] No active Tx. Use ITxRecord.BeginTxn/TxUpdate.");

            if (!amb.Scheduled.TryGetValue(t, out var list))
                amb.Scheduled[t] = list = new List<TxAmbientContext.ScheduledItem>(4);

            // 背压：容量保护
            if (list.Count >= MaxQueuedPerRecord)
            {
                switch (OverflowPolicy)
                {
                    case DropPolicy.DropNewest:
                        // 直接丢弃本次
                        return;
                    case DropPolicy.DropOldest:
                        list.RemoveAt(0);
                        break;
                    case DropPolicy.DropByKeyKeepLast:
                        if (kind == ScheduledKind.Key && key != null)
                        {
                            // 尝试覆盖同 key 的最末一条
                            for (int i = list.Count - 1; i >= 0; i--)
                            {
                                var it = list[i];
                                if (it.Kind == ScheduledKind.Key && it.Key == key)
                                {
                                    list[i] = new TxAmbientContext.ScheduledItem(key, factory, kind);
                                    // 自动打标（保持语义）
                                    amb.Changed.Add((t, key));
                                    return;
                                }
                            }
                        }

                        // 找不到就按 DropOldest 处理
                        list.RemoveAt(0);
                        break;
                }
            }

            // 正常入队
            list.Add(new TxAmbientContext.ScheduledItem(key, factory, kind));

            // 为细粒度/粗粒度自动打标（让 ctx.Publish / PublishKey 能命中）
            if (kind == ScheduledKind.Key) amb.Changed.Add((t, key));
            if (kind == ScheduledKind.Normal) amb.Changed.Add((t, null));
        }

        /// <summary>在 Emit(ctx) 阶段把排队事件刷出来（一次性）。</summary>
        public static void FlushTo(EmitContext ctx, Type ownerType)
        {
            var amb = TxAmbient.Current;
            if (amb == null) return;
            if (!amb.Scheduled.TryGetValue(ownerType, out var list) || list.Count == 0) return;

            // ①（可选）同 key 合并：同事务内只保留“最后一次”
            List<TxAmbientContext.ScheduledItem> snapshot;
            if (CoalesceKeyed)
            {
                // 分开处理：Keyed / 非 Keyed
                var keyedMap = new Dictionary<string, TxAmbientContext.ScheduledItem>();
                var nonKeyed = new List<TxAmbientContext.ScheduledItem>(list.Count);

                foreach (var it in list)
                {
                    if (it.Kind == ScheduledKind.Key && it.Key != null)
                        keyedMap[it.Key] = it; // 覆盖到最后一次
                    else
                        nonKeyed.Add(it);
                }

                snapshot = new List<TxAmbientContext.ScheduledItem>(nonKeyed.Count + keyedMap.Count);
                snapshot.AddRange(nonKeyed);
                snapshot.AddRange(keyedMap.Values);
            }
            else
            {
                snapshot = list; // 不合并就直接用原列表
            }

            // ② 统一发布（保持惰性构造）
            foreach (var it in snapshot)
            {
                switch (it.Kind)
                {
                    case ScheduledKind.Normal:
                        ctx.Publish(it.Factory); // 仅当该 Record 被写过
                        break;
                    case ScheduledKind.Key:
                        ctx.PublishKey(it.Key, it.Factory); // 仅当事务标了对应 key
                        break;
                    case ScheduledKind.Force:
                        ctx.PublishForce(it.Factory);
                        break;
                }
            }

            // ③ 清空本 Record 的队列
            list.Clear();
        }
    }

}
