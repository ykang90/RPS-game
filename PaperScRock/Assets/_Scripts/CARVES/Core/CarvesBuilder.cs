using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CARVES.Abstracts;
using CARVES.Core;
using UnityEngine;

namespace CARVES.Core
{
    public interface IPostInit
    {
        void PostInit();
    }

    /// <summary>CARVES 运行时入口（唯一暴露的静态类）</summary>
    public static class Carves
    {
        // ────────────── 一、外部 API ──────────────
        public static T Resolve<T>() => Container.Get<T>();
        public static void Inject(object target) => Context.InjectDependency(target);

        public static void Setup(IEventBus eventBus, CarvesMetaInfo meta, Action<CarvesBuilder> cfg = null,ITimeSource timeSource = null)
        {
            EventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus), "[CARVES] EventBus 不能为空");
            cfg?.Invoke(_bootstrapBuilder ??= new CarvesBuilder());
            _bootstrapBuilder?.Build(); // 先登记手动
            var record = BuildTxRecord(timeSource ?? new UnityFrameTimeSource(), eventBus, meta);
            Container.AddInstance(record, typeof(ITxRecord));
            Context.Init(meta); // 再批量 + 注入
        }

        public static IEventBus EventBus { get; private set; }

        static CarvesBuilder _bootstrapBuilder;

        // ────────────── 二、Builder（对外可见，用于手动链式注册） ──────────────
        public sealed class CarvesBuilder
        {
            readonly List<Action> _regs = new();
            public CarvesBuilder RegisterInstance<T>(T inst = default, Type asIface = null)
            {
                if (inst == null)
                {
                    inst = Activator.CreateInstance<T>();
                    if (inst == null)
                    {
                        Debug.LogError($"[CARVES] 无法创建实例 {typeof(T).Name}");
                        return this;
                    }
                }

                _regs.Add(() => Container.AddInstance(inst, asIface));
                return this;
            }

            public CarvesBuilder RegisterSingleton<TI, TImpl>() where TImpl : TI, new()
            {
                _regs.Add(() => Container.RegSingleton(typeof(TI), typeof(TImpl)));
                return this;
            }

            public CarvesBuilder RegisterTransient<TI, TImpl>() where TImpl : TI, new()
            {
                _regs.Add(() => Container.RegFactory(typeof(TI), () => new TImpl()));
                return this;
            }

            public void Build()
            {
                foreach (var r in _regs) r();
            }
        }

        // ────────────── 三、Context（属性注入 + 元信息批量注册） ──────────────
        static class Context
        {
            public static bool IsInit => _isInit;
            static bool _isInit;
            static readonly Dictionary<Type, MemberInfo[]> _cache = new();

            public static void Init(CarvesMetaInfo meta)
            {
                if (_isInit) return;
                // 视图不需要注册到容器
                foreach (var e in meta.Entries.Where(m => m.Layer != CarvesLayers.View))
                {
                    if (e.InterfaceNames.Any())
                        RegisterInterfaces(e);
                    else RegisterInstance(e);
                }
                _isInit = true;
                InjectAllSingletons();
            }
            static void RegisterInstance(CarvesMetaEntry e)
            {
                if (e.SelfManage) return; 
                var impl = Type.GetType(e.AssemblyQualifiedName);
                if (impl == null)
                    Debug.LogError($"[CARVES] 解析失败: {e.TypeName}");
                object obj = typeof(MonoBehaviour).IsAssignableFrom(impl)
                    ? UnityEngine.Object.FindFirstObjectByType(impl)
                    : Activator.CreateInstance(impl);

                if (obj == null)
                {
                    Debug.LogError($"[CARVES] 无法找到或创建 {impl.Name}");
                    return;
                }

                Container.AddInstance(obj);
            }

            static void RegisterInterfaces(CarvesMetaEntry e)
            {
                foreach (var ifaceName in e.InterfaceNames)
                {
                    var iface = Type.GetType(ifaceName);
                    var impl = Type.GetType(e.AssemblyQualifiedName);
                    if (iface == null || impl == null)
                    {
                        Debug.LogError($"[CARVES] 解析失败: {ifaceName}/{e.TypeName}");
                        continue;
                    }

                    Container.RegSingleton(iface, impl);
                }
            }

            public static void InjectDependency(object target)
            {
                if (target == null) return;
                var t = target.GetType();

                if (!_cache.TryGetValue(t, out var members))
                {
                    members =
                        // 属性
                        t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                            .Where(p => p.GetCustomAttribute<InjectAttribute>() != null)
                            .Cast<MemberInfo>()
                            .Concat(
                                // 字段
                                t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                    .Where(f => f.GetCustomAttribute<InjectAttribute>() != null))
                            .ToArray();

                    _cache[t] = members;
                }

                foreach (var m in members)
                {
                    var depType = m is PropertyInfo pi
                        ? pi.PropertyType
                        : ((FieldInfo)m).FieldType;
                    object val;
                    if (depType.IsGenericType)
                    {
                        var genDef = depType.GetGenericTypeDefinition();
                        if (genDef == typeof(Lazy<>))
                            val = ResolveLazy(depType.GetGenericArguments()[0]);
                        else if (genDef == typeof(Func<>))
                            val = ResolveFunc(depType.GetGenericArguments()[0]);
                        else
                            val = Container.Get(depType);
                    }
                    else
                        val = Container.Get(depType);

                    if (m is PropertyInfo p) p.SetValue(target, val);
                    else ((FieldInfo)m).SetValue(target, val);

                    if (val == null)
                        throw new InvalidOperationException(
                            $"[CARVES] 依赖未注册: {t.Name}.{m.Name} → {depType.Name}");
                }

                // 如果有 IPostInit 接口，则调用 PostInit 方法
                if (target is IPostInit postInit) postInit.PostInit();
            }

            public static void InjectAllSingletons()
            {
                foreach (var s in Container.AllSingletons)
                    InjectDependency(s);
            }

            static object ResolveLazy(Type gArg)
            {
                // ① 取得 Container.Get<T>() 的 MethodInfo
                var getMi = typeof(Container)
                    .GetMethod(nameof(Container.Get), Type.EmptyTypes)
                    .MakeGenericMethod(gArg);

                // ② 创建 Func<T> 委托
                var funcType = typeof(Func<>).MakeGenericType(gArg); // Func<GameWorld>
                var factory = Delegate.CreateDelegate(funcType, getMi); // () => Container.Get<GameWorld>()

                // ③ 用正确类型的委托实例化 Lazy<T>
                var lazyType = typeof(Lazy<>).MakeGenericType(gArg); // Lazy<GameWorld>
                return Activator.CreateInstance(lazyType, factory);
            }

            static object ResolveFunc(Type gArg)
            {
                var m = typeof(Container).GetMethod(nameof(Container.Get), Type.EmptyTypes)
                    .MakeGenericMethod(gArg);
                return Delegate.CreateDelegate(typeof(Func<>).MakeGenericType(gArg), m);
            }
        }

        // ────────────── 四、Container（真正保存对象的地方） ──────────────
        static class Container
        {
            static readonly Dictionary<Type, object> _single = new();
            static readonly Dictionary<Type, Func<object>> _factory = new();
            static readonly Dictionary<Type, object> _impl = new();
            public static IEnumerable<object> AllSingletons => _single.Values;

            public static void AddInstance(object obj, Type asIface = null)
            {
                if (obj == null) return;
                if (Context.IsInit) Inject(obj);

                var impl = obj.GetType();
                _impl[impl] = obj;

                if (asIface != null) _single.TryAdd(asIface, obj);
                else
                {
                    _single.TryAdd(impl, obj);
                    foreach (var i in impl.GetInterfaces()) _single.TryAdd(i, obj);
                }
            }

            public static void RegSingleton(Type iface, Type impl)
            {
                if (_single.ContainsKey(iface)) return;
                if (_impl.TryGetValue(impl, out var cached))
                {
                    _single.Add(iface, cached);
                    return;
                }

                object obj = typeof(MonoBehaviour).IsAssignableFrom(impl)
                    ? UnityEngine.Object.FindFirstObjectByType(impl)
                    : Activator.CreateInstance(impl);

                if (obj == null)
                {
                    Debug.LogError($"[CARVES] 无法找到或创建 {impl.Name}");
                    return;
                }

                if (Context.IsInit) Inject(obj);

                _single[iface] = obj;
                _impl[impl] = obj;
            }

            public static void RegFactory(Type iface, Func<object> factory)
            {
                _factory[iface] = () =>
                {
                    var o = factory();
                    if (Context.IsInit) Inject(o);
                    return o;
                };
            }

            public static void Remove(object obj)
            {
                if (obj == null) return;

                // 1) 在 _single 中移除所有 value == obj 的键
                foreach (var key in _single.Where(kv => kv.Value == obj).Select(kv => kv.Key).ToArray())
                    _single.Remove(key);

                // 2) 在 _impl 中移除
                _impl.Remove(obj.GetType());

                // 3) 可选：删除对应工厂（如果希望彻底禁止新建）
                foreach (var key in _factory.Where(kv => kv.Value() == obj).Select(kv => kv.Key).ToArray())
                    _factory.Remove(key);
            }


            public static object Get(Type t)
            {
                if (_single.TryGetValue(t, out var o)) return o;
                if (_impl.TryGetValue(t, out o)) return o;
                if (_factory.TryGetValue(t, out var f)) return f();
                Debug.LogError($"[CARVES] 未注册类型 {t.Name}");
                return null;
            }

            public static T Get<T>() => (T)Get(typeof(T));
        }

        public static void AddInstance(object obj, Type asIface = null) => Container.AddInstance(obj, asIface);
        public static void Remove(object obj) => Container.Remove(obj);
        /// <summary>
        /// 从 CarvesMetaInfo 自动实例化并注册所有 [CarvesRecord] 类型，构建 TxRecord。
        /// </summary>
        /// <param name="time">ITimeSource（UnityFrame/LogicFrame 均可）</param>
        /// <param name="bus">IEventBus</param>
        /// <param name="meta"></param>
        /// <param name="factory">
        /// 可选：自定义实例工厂，形如 t => (ICarvesRecord)container.Resolve(t)。
        /// 不传则使用无参构造 Activator.CreateInstance。
        /// </param>
        public static ITxRecord BuildTxRecord(
            ITimeSource time,
            IEventBus bus,
            CarvesMetaInfo meta,
            Func<Type, ICarvesRecord> factory = null)
        {
            if (time == null) throw new ArgumentNullException(nameof(time));
            if (bus  == null) throw new ArgumentNullException(nameof(bus));

            var list = new List<IRecordShard>(64);

            foreach (var entry in meta.Entries.Where(e=>e.Layer == CarvesLayers.Record)) // ← 你的生成器产出
            {
                var t = Type.GetType(entry.AssemblyQualifiedName)!;
                if (!typeof(ICarvesRecord).IsAssignableFrom(t))
                    throw new InvalidOperationException($"[CarvesBuilder] {t.Name} 未实现 ICarvesRecord");

                var rec = factory != null
                    ? factory(t)
                    : (ICarvesRecord)Activator.CreateInstance(t); // 默认无参构造

                if (rec == null)
                    throw new InvalidOperationException($"[CarvesBuilder] 无法创建 {t.Name}");

                list.Add(rec); // ICarvesRecord : IRecordShard
            }

            return new TxRecord(time, bus, list.Cast<IRecordShard>().ToArray());
        }

    }

    public static class EventBusExt
    {
        static readonly MethodInfo PublishOpen =
            typeof(IEventBus).GetMethods()
                .First(m => m.Name == "Publish" && m.IsGenericMethodDefinition);

        /// <summary>
        /// TxRecord.Commit 内部，框架内部把 EmitContext 收集到的事件一次性发上 EventBus<br/>
        /// IEventBus.Publish 的适配，业务代码不要直接调
        /// </summary>
        /// <param name="bus"></param>
        /// <param name="ev"></param>
        public static void PublishRuntime(this IEventBus bus, IEvent ev)
        {
            var mi = PublishOpen.MakeGenericMethod(ev.GetType());
            // 形参：TEvent evt, string callerMember, string callerFile, int callerLine
            mi.Invoke(bus, new[] { ev, ev.Metadata["Caller"], ev.Metadata["File"], ev.Metadata["Line"] });
        }
    }
}