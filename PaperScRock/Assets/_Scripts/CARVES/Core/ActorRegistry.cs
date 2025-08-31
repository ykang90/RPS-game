using System;
using System.Collections.Generic;
using System.Linq;
using CARVES.Utls;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;

namespace CARVES.Core
{
    public interface IResettable { void ResetState(); }

    /// <summary>
    /// 通用 <see cref="ActorRegistry{T}"/> ：泛型实体注册表 / 生命周期容器。
    /// 1. 通过构造函数一次性注入 ❶预生成(PreSpawn) 与 ❷预回收(PostDespawn) 委托，
    ///    行为与 Unity <see cref="ObjectPool{T}.ObjectPool"/> 构造函数保持一致；
    /// 2. 内部自动在对象池 onGet / onRelease 中调用这两个委托；
    /// 3. Spawn/Despawn API 不再暴露临时委托参数，调用端更安全、易读。
    /// </summary>
    /// <typeparam name="T">派生自 <see cref="MonoBehaviour"/> 的实体根脚本</typeparam>
    public class ActorRegistry<T> : IUpdatable where T : MonoBehaviour
    {
        #region Events (对外仍可额外订阅)
        public event UnityAction<T> OnSpawned;
        public event UnityAction<T> OnDespawned;
        #endregion

        #region Ctor-injected callbacks
        readonly UnityAction<T> _preSpawn;     // 在 OnGet 之后、OnSpawned 之前
        readonly UnityAction<T> _postDespawn;  // 在 OnDespawned 之后、真正回池之前
        #endregion

        #region Collections
        readonly HashSet<T> _liveEntities   = new();
        readonly HashSet<IUpdatable> _ticks = new();
        readonly Dictionary<GameObject, IObjectPool<T>> _pools    = new();
        readonly Dictionary<T, GameObject> _originMap             = new();
        #endregion

        #region Public Accessors
        public GameObject gameObject { get; }
        public IReadOnlyCollection<T>  LiveEntities => _liveEntities;
        public IEnumerable<T>          Entities     => _liveEntities;
        public T Find(Predicate<T> match)      => _liveEntities.FirstOrDefault(e => match(e));
        public List<T> FindAll(Predicate<T> m) => _liveEntities.Where(e => m(e)).ToList();
        #endregion

        #region Constructor

        /// <param name="host">挂载 Registry 的宿主 GO（一般是场景级空物体）</param>
        /// <param name="preSpawn">对象激活后、正式加入 LiveEntities 前调用</param>
        /// <param name="postDespawn">对象移出 LiveEntities、回池前调用</param>
        public ActorRegistry(
            GameObject host,
            UnityAction<T> preSpawn = null,
            UnityAction<T> postDespawn = null)
        {
            gameObject = host ? host : throw new ArgumentNullException(nameof(host));
            _preSpawn = preSpawn;
            _postDespawn = postDespawn;
        }

        #endregion

        #region Spawn / Despawn
        public T Spawn(T prefab, Transform parent = null)
        {
            if (!prefab) throw new ArgumentNullException(nameof(prefab));

            T inst = GetOrCreatePool(prefab).Get();
            PrepareInstance(inst, prefab, parent); // 记录源 prefab
            _liveEntities.Add(inst);
            if (inst is IUpdatable u) _ticks.Add(u);

            OnSpawned?.Invoke(inst);
            return inst;
        }

        public void Despawn(T entity)
        {
            if (!entity || !_liveEntities.Remove(entity)) return;
            if (entity is IUpdatable u) _ticks.Remove(u);

            OnDespawned?.Invoke(entity);
            _postDespawn?.Invoke(entity); // 全局回收前 hook

            if (_originMap.TryGetValue(entity, out var prefabGo) &&
                _pools.TryGetValue(prefabGo, out var pool))
            {
                pool.Release(entity);
            }
            else
            {
                UnityEngine.Object.Destroy(entity.gameObject);
            }

            _originMap.Remove(entity); // 防止泄漏
        }

        public void DespawnAll() => DespawnWhere(_ => true);
        public void DespawnWhere(Predicate<T> match)
        {
            foreach (var e in _liveEntities.Where(le => match(le)).ToArray())
                Despawn(e);
        }
        #endregion

        #region Pool helpers
        IObjectPool<T> GetOrCreatePool(T prefab)
        {
            if (_pools.TryGetValue(prefab.gameObject, out var pool)) return pool;

            pool = new ObjectPool<T>(
                // 创建
                () => UnityEngine.Object.Instantiate(prefab),
                // OnGet —— 激活后立即调用
                o =>
                {
                    o.gameObject.SetActive(true);
                    _preSpawn?.Invoke(o);      // 统一入口：预生成 hook
                },
                // OnRelease
                o =>
                {
                    if (o is IResettable r) r.ResetState();
                    o.gameObject.SetActive(false);
                },
                // OnDestroy
                o => UnityEngine.Object.Destroy(o.gameObject),
                collectionCheck: false, maxSize: 256);

            _pools[prefab.gameObject] = pool;
            return pool;
        }

        void PrepareInstance(T inst, T prefab, Transform parent)
        {
            if (parent) inst.transform.SetParent(parent);
            _originMap[inst] = prefab.gameObject;
        }
        #endregion

        #region IUpdatable
        public void OnUpdate() { foreach (var t in _ticks) t.OnUpdate(); }
        #endregion
    }
}
