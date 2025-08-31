using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CARVES.Core
{
    public static class Diag {
        static int _seq;                        // 日志顺序号
        public  static bool Enabled = true;     // 一键关掉
        public static void Log(string tag, string msg,
            Object ctx = null, LogType t = LogType.Log) {
            if (!Enabled) return;
            var prefix = $"[{Time.realtimeSinceStartup:F1}s #{++_seq}] {tag} · ";
            Debug.unityLogger.Log(t, tag, prefix + msg, ctx);
        }
        public static void Err(string tag, Exception e, Object ctx = null) =>
            Log(tag, $"❌ {e.GetType().Name}: {e.Message}", ctx, LogType.Error);
    }
}