using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CARVES.Utls
{
    public static class DebugExtension
    {
        public static void Log(this Object obj, object message, LogType type, [CallerMemberName] string methodName = null)
        {
            var msg = message?.ToString();
            if (string.IsNullOrWhiteSpace(msg)) msg = "Invoke()!";
            var log = $"{obj.GetType().Name}.{methodName}\n{obj.name} : {msg}";
            switch (type)
            {
                case LogType.Error:
                    Debug.LogError(log, obj);
                    break;
                case LogType.Warning:
                case LogType.Assert:
                    Debug.LogWarning(log, obj);
                    break;
                case LogType.Log:
                    Debug.Log(log, obj);
                    break;
                case LogType.Exception:
                    var ep = new DebugException(log);
                    Debug.LogException(ep, obj);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        public static void Log(this Type obj, object message, LogType type, [CallerMemberName] string methodName = null)
        {
            var msg = message?.ToString();
            if (string.IsNullOrWhiteSpace(msg)) msg = "Invoke()!";
            var log = $"{methodName}\n{obj.Name} : {msg}";
            switch (type)
            {
                case LogType.Error:
                    Debug.LogError(log);
                    break;
                case LogType.Warning:
                case LogType.Assert:
                    Debug.LogWarning(log);
                    break;
                case LogType.Log:
                    Debug.Log(log);
                    break;
                case LogType.Exception:
                    var ep = new DebugException(log);
                    Debug.LogException(ep);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public static void Log(this string message, object obj, LogType type, [CallerMemberName] string method = null) => obj.GetType().Log(message, type, method);
        public static void Log(this string message, object obj, [CallerMemberName] string method = null) => obj.GetType().Log(message, LogType.Log, method);
        public static void Log(this string message, Object obj, [CallerMemberName] string methodName = null) => obj.Log(message, LogType.Log, methodName);
        public static void Log(this Object obj, [CallerMemberName] string methodName = null) => obj.Log(string.Empty, LogType.Log, methodName);
        public static void Log(this string message, Object obj, LogType type, [CallerMemberName] string methodName = null) => obj.Log(message, type, methodName);
        public static void Log(this Object obj, LogType type, [CallerMemberName] string methodName = null) => obj.Log(string.Empty, type, methodName);

        class DebugException : Exception
        {
            public DebugException(string message) : base(message)
            {

            }
        }
    }
}