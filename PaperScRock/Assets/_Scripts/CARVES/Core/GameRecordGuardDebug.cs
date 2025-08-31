using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using CARVES.Abstracts;

namespace CARVES.Core
{
    public static class GameRecordGuardDebug
    {
        record Result(bool IsFromExecuteLayer);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        static readonly ConditionalWeakTable<MethodBase, Result> Cache = new();
        public static bool IsFromExecuteLayer()
        {
            var st = new StackTrace(skipFrames: 2, fNeedFileInfo: false);
            for (int i = 0; i < st.FrameCount; i++)
            {
                var mb = st.GetFrame(i).GetMethod();
                if (mb == null) continue;
                if (Cache.TryGetValue(mb, out var ok)) return ok.IsFromExecuteLayer;

                var t = mb.DeclaringType;
                ok = new(t != null && (
                        t.GetCustomAttribute<CarvesModuleAttribute>() != null ||
                        t.FullName?.Contains(".Execute") == true ||
                        t.GetCustomAttribute<CarvesApiAttribute>() != null)
                );
                Cache.Add(mb, ok);
                if (ok.IsFromExecuteLayer) return true;
            }
            return false;
        }
#else
    public static bool IsFromExecuteLayer() => true;
#endif
    }
}