using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace CARVES.Utls
{
    [DisallowMultipleComponent]
    public sealed class ApiClient : MonoBehaviour
    {
        // --------- 可在 Inspector 调整 -------------
        [SerializeField] int timeoutSecs = 15;
        [SerializeField] GameObject loadingIndicator;
        [SerializeField, OnValueChanged(nameof(ResetServerUri))] string serverUrl = "https://localhost:7272/";

        // --------- 单例 ---------------------------
        static ApiClient _instance;
        public static ApiClient Instance
        {
            get
            {
                if (_instance) return _instance;

                // 懒加载：场景里没有则动态建一个隐藏节点
                var go = new GameObject("[ApiClient]");
                _instance = go.AddComponent<ApiClient>();
                return _instance;
            }
        }

        // --------- 状态 / 事件 ---------------------
        public bool IsBusy = false;
        public event UnityAction<ApiResult> OnRequestFailed;

        readonly HashSet<string> _pending = new();
        Uri _baseUri;

        // --------- 生命周期 -----------------------
        void Awake()
        {
            if (_instance && _instance != this)
            {
                Destroy(gameObject);             // 防止多实例
                return;
            }
            _instance = this;

            ResetServerUri();
        }

        void ResetServerUri() => SetServerUrl(serverUrl);

        // 如需运行时改服务器地址
        public static void SetServerUrl(string url)
        {
            _instance.serverUrl = url;
            _instance._baseUri = url.ToUri();
        }

        // =====================================================
        //  对外静态快捷方法
        // =====================================================
        public static UniTask<ApiResult> GetAsync(string path, CancellationToken ct = default)
            => Instance.SendAsync(ApiMethod.Get, path, null, ct, null);

        public static UniTask<ApiResult> PostAsync(string path, string json,
            CancellationToken ct = default)
            => Instance.SendAsync(ApiMethod.Post, path, json, ct, null);

        public static void Get(string path, UnityAction<ApiResult> cb)
            => _ = WrapCallback(GetAsync(path), cb);

        public static void Post(string path, string json, UnityAction<ApiResult> cb)
            => _ = WrapCallback(PostAsync(path, json), cb);

        static async UniTaskVoid WrapCallback(UniTask<ApiResult> t, UnityAction<ApiResult> cb)
            => cb?.Invoke(await t);

        // =====================================================
        //  内核
        // =====================================================
        async UniTask<ApiResult> SendAsync(ApiMethod method, string path, object body,
            CancellationToken ct, string gateKey)
        {
            gateKey ??= $"{method}:{path}:{body?.GetHashCode() ?? 0}";
            var uri = _baseUri.ToString().UriJoin(path);
            if (!_pending.Add(gateKey))
                return new ApiResult(uri.AbsoluteUri,false, "", 0, "Duplicate request", 0, false);

            IsBusy = true;
            if (loadingIndicator) loadingIndicator.SetActive(IsBusy);
            try
            {
                var result = await uri.SendAsync(method, body, ct, timeoutSecs);

                if (!result.Ok && !ct.IsCancellationRequested)
                    OnRequestFailed?.Invoke(result);

                return result;
            }
            finally
            {
                _pending.Remove(gateKey);
                IsBusy = _pending.Count > 0;
                if (loadingIndicator) loadingIndicator.SetActive(IsBusy);
            }
        }
    }
}
