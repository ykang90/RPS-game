using System.Collections.Generic;
using System.Linq;

namespace CARVES.Utls
{
    public interface IMinMax
    {
        int Min { get; }
        int Max { get; }
    }
    public static class MinMaxExtension
    {
        public static bool MinMaxDefault(this IMinMax clause) => clause.Min == 0 && clause.Max == 0;
        /// <summary>
        /// 0 为不判断值，直接返(条件)true，<see cref="IMinMax.Max"/>值=-1为判断<see cref="value"/>是否=0，
        /// 一般上都是判断<see cref="value"/>是否在范围内
        /// </summary>
        /// <param name="clause"></param>
        /// <param name="value"></param>
        /// <param name="includedMax"></param>
        /// <returns></returns>
        public static bool InMinMaxRange(this IMinMax clause, int value, bool includedMax = true)
        {
            //上限如果小于0(-1)直接判定是否状态为0
            if (clause.Max < 0) return value == 0;
            var skipMin = clause.Min <= 0;
            var skipMax = clause.Max == 0;
            return (skipMin || value >= clause.Min) &&
                   (skipMax || (includedMax ? value <= clause.Max : value < clause.Max));
        }

        public static bool InMinMaxRange(this IEnumerable<IMinMax> clauses, int value, bool includedMax = true) =>
            clauses.All(c => c.InMinMaxRange(value, includedMax));
    }
}