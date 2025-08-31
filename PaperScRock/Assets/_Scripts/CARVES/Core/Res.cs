using System;
using System.Collections.Generic;
using System.Linq;
using CARVES.Utls;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace CARVES.Core
{
    /// <summary>资源种类，用于区分句柄</summary>
    public enum ResKind { Catalog, Asset, Instance, Download }

    /// <summary>键 = (值 + 种类)，避免 Asset 与 Download 名字冲突</summary>
    public readonly struct ResKey : IEquatable<ResKey>
    {
        public readonly object Value;
        public readonly ResKind Kind;
        public ResKey(object value, ResKind kind) { Value = value; Kind = kind; }

        public bool Equals(ResKey other) => Value.Equals(other.Value) && Kind == other.Kind;
        public override bool Equals(object obj) => obj is ResKey k && Equals(k);
        public override int GetHashCode() => Value.GetHashCode() ^ (int)Kind;
        public override string ToString() => $"{Kind}:{Value}";
    }

    public sealed class Res : MonoBehaviour
    {
        /* ---------------- 单例 ---------------- */
        public static Res Instance { get; private set; }
        void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /* ------------ 句柄集中管理 ------------- */
        readonly Dictionary<ResKey, List<AsyncOperationHandle>> _map = new();
        void AddHandle(ResKey k, AsyncOperationHandle h)
        {
            if (!_map.TryGetValue(k, out var list)) _map[k] = list = new();
            list.Add(h);
        }

        /* --------------- 初始化 --------------- */
        public bool IsInit;
        public async UniTask InitAsync(IProgress<float> progress = null)
        {
            if (IsInit) return;
            var h = Addressables.InitializeAsync(autoReleaseHandle: false);
            await ProgressUpdateLoopAsync(nameof(InitAsync),progress, h);
            if (h.Status != AsyncOperationStatus.Succeeded)
                throw h.OperationException ?? new Exception($"{nameof(InitAsync)} failed");

            AddHandle(new ResKey("catalog", ResKind.Catalog), h);
            IsInit = true;
        }

        /// <summary>
        /// 下载并立即反序列化 label 对应的全部资源，用作“Shader / 字体 / 公共依赖”预热。
        /// 返回加载后的对象列表；如果只是预热，可视情况立刻 Addressables.Release(handle)。
        /// </summary>
        public async UniTask WarmUpLabelAsync<T>(string label, Action<T> onItem = null, IProgress<float> progress = null)
        {
            // ---------- ① 下载远程 Bundle ----------
            // GetDownloadSizeAsync + DownloadDependenciesAsync 已封装在你的 PreDownloadAsync
            await PreDownloadAsync(label, progress);

            //---------- ② 反序列化到内存----------
            var h = Addressables.LoadAssetsAsync<T>(label.SingleElementToArray().AsEnumerable(), onItem,
                Addressables.MergeMode.Union);
            AddHandle(new ResKey(label, ResKind.Asset), h); // 先登记句柄，防止早释放
            await ProgressUpdateLoopAsync(label, progress, h);
            if (h.Status != AsyncOperationStatus.Succeeded)
                throw h.OperationException ?? new Exception($"WarmUp {label} failed");
        }

        /* ------------- 资源加载 / 实例化 -------------- */
        public AsyncOperationHandle<T> LoadAsync<T>(object key)
        {
            var h = Addressables.LoadAssetAsync<T>(key);
            AddHandle(new ResKey(key, ResKind.Asset), h);
            return h;
        }

        public AsyncOperationHandle<GameObject> InstantiateAsync(object key, Transform parent = null, bool world = false)
        {
            var h = Addressables.InstantiateAsync(key, parent, world);
            AddHandle(new ResKey(key, ResKind.Instance), h);
            return h;
        }

        /* ------------- 预下载（后台偷下） -------------- */
        /// <summary>按 Label 预下载依赖，返回本次下载的字节数</summary>
        public async UniTask<long> PreDownloadAsync(object label, IProgress<float> progress = null)
        {
            var sizeTask = Addressables.GetDownloadSizeAsync(label).Task;
            var size = await sizeTask;  // 获取下载大小
            if (size == 0) return 0;// 已缓存，无需下载

            var dl = Addressables.DownloadDependenciesAsync(label, false);
            AddHandle(new ResKey(label, ResKind.Download), dl);
            await ProgressUpdateLoopAsync(label, progress, dl);
            return size;
        }

        static async UniTask ProgressUpdateLoopAsync(object label,IProgress<float> progress, AsyncOperationHandle dl)
        {
            await dl.ToUniTask(progress);
            if (dl.Status != AsyncOperationStatus.Succeeded)
                throw new Exception($"Download {label} failed: {dl.OperationException?.Message}");
            //while (!dl.IsDone)
            //{
            //    progress?.Report(dl.PercentComplete);
            //    await UniTask.Delay(ms);
            //}
        }

        /* ----------------- 释放 ----------------- */
        public void Release(object value, ResKind kind)
        {
            var key = new ResKey(value, kind);
            if (!_map.TryGetValue(key, out var list)) return;
            foreach (var h in list) Addressables.Release(h);
            _map.Remove(key);
        }
        public void ReleaseInstance(GameObject go) => Addressables.ReleaseInstance(go);

        public void ReleaseAll()
        {
            foreach (var list in _map.Values)
            foreach (var h in list)
                Addressables.Release(h);
            _map.Clear();
        }
    }
}