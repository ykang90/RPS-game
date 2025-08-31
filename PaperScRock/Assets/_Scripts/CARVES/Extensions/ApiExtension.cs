#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace CARVES.Utls
{
    public enum ApiMethod
    {
        Get,
        Post,
        Put,
        Delete,
        Head,
    }
    static class ApiMethodEx
    {
        // 映射到 UnityWebRequest.kHttpVerbXXXX
        public static string ToVerb(this ApiMethod m) => m switch
        {
            ApiMethod.Get     => UnityWebRequest.kHttpVerbGET,
            ApiMethod.Post    => UnityWebRequest.kHttpVerbPOST,
            ApiMethod.Put     => UnityWebRequest.kHttpVerbPUT,
            ApiMethod.Delete  => UnityWebRequest.kHttpVerbDELETE,
            ApiMethod.Head    => UnityWebRequest.kHttpVerbHEAD,
            _                 => throw new ArgumentOutOfRangeException(nameof(m), m, null)
        };

        // 是否允许有 request-body
        public static bool SupportsBody(this ApiMethod m) => m is ApiMethod.Post or ApiMethod.Put;
    }
    /// <summary>统一的返回结构，便于记录更多调试信息。</summary>
    public struct ApiResult
    {
        public string RequestUri { get; } 
        public bool Ok { get; }
        public string Body { get; }
        public long Status { get; }
        public string Error { get; }
        public long ElapsedMs { get; }
        public bool Canceled { get; } // 是否被取消
        /// <summary>统一的返回结构，便于记录更多调试信息。</summary>
        public ApiResult(string requestUri, bool ok, string body, long status, string error, long elapsedMs, bool canceled)
        {
            Ok = ok;
            Body = body;
            Status = status;
            Error = error;
            ElapsedMs = elapsedMs;
            Canceled = canceled;
            RequestUri = requestUri;
        }
    }

    public static class ApiExtension
    {
        const int DefaultTimeout = 15;
        public static Uri UriJoin(this string url, string api = null)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url), "URL 不能为空");
            if (!url.EndsWith("/"))
                url += "/";                     // ← 关键：末尾加 /
            if (string.IsNullOrEmpty(api))
                return new Uri(url, UriKind.Absolute);
            if (api.StartsWith("/"))
                api = api[1..];                // 去掉开头的 /
            return new Uri(url + api);
        }
        public static Uri ToUri(this string url,string api = null)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url), "URL 不能为空");
            if (!url.EndsWith("/"))
                url += "/";                     // ← 关键：末尾加 /
            var uri = new Uri(url, UriKind.Absolute);
            return string.IsNullOrEmpty(api) ? uri : new Uri(uri, api); // 追加 API 路径
        }
        /// <summary>
        /// 通用的请求方法，基于Unitask封装的异步(底层非异步)请求。
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="method"></param>
        /// <param name="payloadObj"></param>
        /// <param name="ct"></param>
        /// <param name="timeoutSeconds"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static UniTask<ApiResult> SendAsync(
            this Uri uri,
            ApiMethod method,
            object? payloadObj = null,        // 可传 DTO / string / null
            CancellationToken ct = default,
            int timeoutSeconds = DefaultTimeout,
            IDictionary<string, string>? headers = null)
        {
            var json = payloadObj switch
            {
                null => null,
                string s => s,
                _ => Json.Serialize(payloadObj)
            };     // 这里换成你常用的序列化器

            return SendInternalAsync(uri, method, json, ct, timeoutSeconds, headers);
        }

        static async UniTask<ApiResult> SendInternalAsync(
            Uri uri, ApiMethod method, string? json,
            CancellationToken ct, int timeoutSeconds,
            IDictionary<string, string>? headers)
        {
            using var req = new UnityWebRequest(uri, method.ToVerb())
            {
                timeout = timeoutSeconds
            };

            // ── Body（仅在需要时附加） ──────────────────────
            if (json != null)
            {
                if (!method.SupportsBody())
                    throw new InvalidOperationException($"{method} 不允许包含请求体");

                var bytes = Encoding.UTF8.GetBytes(json);
                req.uploadHandler = new UploadHandlerRaw(bytes);
                req.SetRequestHeader("Content-Type", "application/json");
            }

            // ── Headers（额外自定义）──────────────────────
            if (headers != null)
                foreach (var kv in headers)
                    req.SetRequestHeader(kv.Key, kv.Value);

            // 必须有 downloadHandler，否则不能读取 Text
            req.downloadHandler = new DownloadHandlerBuffer();
#if UNITY_EDITOR
            $"{req.method} {uri} Req...".Log(nameof(ApiExtension));
#endif
            var sw = Stopwatch.StartNew();
            try
            {
                await req.SendWebRequest().ToUniTask(cancellationToken: ct);
            }
            catch (OperationCanceledException)
            {
                sw.Stop();
                return new ApiResult(uri.AbsoluteUri,false, string.Empty, -1, "Canceled", sw.ElapsedMilliseconds, true);
            }

            sw.Stop();
            var ok = req.result == UnityWebRequest.Result.Success;
            var body = ok ? req.downloadHandler?.text ?? string.Empty : string.Empty;
#if UNITY_EDITOR
            $"{req.method} {req.responseCode} {uri} [{sw.ElapsedMilliseconds}ms]".Log(nameof(ApiExtension));
#endif
            return new ApiResult(uri.AbsoluteUri,ok, body, req.responseCode, ok ? string.Empty : req.error,
                sw.ElapsedMilliseconds, false);
        }

        // =====================================================
        // 3. 兼容旧包装：GetAsync / PostJsonAsync 等
        // =====================================================
        public static UniTask<ApiResult> GetAsync(this Uri uri,
            CancellationToken ct = default, int timeoutSeconds = DefaultTimeout)
            => uri.SendAsync(ApiMethod.Get, null, ct, timeoutSeconds);

        public static UniTask<ApiResult> PostJsonAsync(this Uri uri, string json,
            CancellationToken ct = default, int timeoutSeconds = DefaultTimeout)
            => uri.SendAsync(ApiMethod.Post, json, ct, timeoutSeconds);

        public static UniTask<ApiResult> PostObjAsync<T>(this Uri uri, T payload,
            CancellationToken ct = default, int timeoutSeconds = DefaultTimeout)
            => uri.SendAsync(ApiMethod.Post, payload, ct, timeoutSeconds);

        // 其他动词同理扩展...
    }
}