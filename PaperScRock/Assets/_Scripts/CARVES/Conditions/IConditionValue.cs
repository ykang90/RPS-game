
namespace CARVES.Conditions
{
    /// <summary>
    /// 游戏状态值，作为非常基础的游戏属性，分为固定，上限，当前值
    /// </summary>
    public interface IConditionValue
    {
        int Max { get; }
        int Value { get; }
        int Fix { get; }
        /// <summary>
        /// 上限与固定值的比率
        /// </summary>
        double MaxFixRatio { get; }
        /// <summary>
        /// 当前与固定值的比率
        /// </summary>
        double ValueFixRatio { get; }
        /// <summary>
        /// 当前与最大值的比率
        /// </summary>
        double ValueMaxRatio { get; }
    }
}