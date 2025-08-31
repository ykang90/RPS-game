namespace CARVES.Core
{
    public interface ILogicTimeSource : ITimeSource { void Next(); }

    public sealed class LogicTimeSource : ILogicTimeSource
    {
        public int Frame { get; private set; }
        public void Next() => Frame++; // 在主循环 / 流程推进点调用
    }

    public sealed class UnityFrameTimeSource : ITimeSource
    {
        public int Frame => UnityEngine.Time.frameCount; // 每个渲染帧自增
    }
    public sealed class ManualTimeSource : ITimeSource
    {
        public int Frame { get; private set; }
        public void Set(int frame) => Frame = frame;
        public void Advance(int n = 1) => Frame += n;
    }

}