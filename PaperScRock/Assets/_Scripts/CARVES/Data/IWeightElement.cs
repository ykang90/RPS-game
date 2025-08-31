using System;
using System.Collections.Generic;
using System.Linq;

namespace CARVES.Utls
{
    public interface IWeightElement
    {
        int Weight { get; }
    }

    public static class WeightElementExtension
    {
        /// <summary> 超大权重用 long 的权重随机 </summary>
        public static T WeightPick<T>(this IEnumerable<T> source) where T : IWeightElement
        {
            var pool = source.Where(e => e.Weight > 0).ToArray();
            if (pool.Length == 0)
                throw new InvalidOperationException("Pool has no positive-weight element.");

            var total = pool.Sum(e => (long)e.Weight);
            double r = UnityEngine.Random.value * total; // 0 ≤ r < total

            long acc = 0;
            foreach (var e in pool)
            {
                acc += e.Weight;
                if (r < acc) return e;
            }

            return pool[^1]; // Fallback，理论到不了
        }

        /// <summary>按照权重随机抽取 <paramref name="count"/> 个唯一元素。</summary>
        public static List<T> WeightTake<T>(this IEnumerable<T> source, int count)
            where T : IWeightElement
        {
            if (count <= 0) return new List<T>(0);

            /* ---------- ① 构建正权重池 ---------- */
            var pool = source.Where(e => e.Weight > 0).ToList();
            if (pool.Count == 0) return new List<T>(0);

            long total = pool.Sum(e => (long)e.Weight);
            var result = new List<T>(Math.Min(count, pool.Count));

            /* ---------- ② 抽取 ---------- */
            for (int n = 0; n < count && pool.Count > 0; n++)
            {
                // r ∈ [0, total)
                double r = UnityEngine.Random.value * total;

                long acc = 0;
                int idx = 0;
                for (; idx < pool.Count; idx++)
                {
                    acc += pool[idx].Weight;
                    if (r < acc) break;
                }

                var picked = pool[idx];
                result.Add(picked);

                /* ---------- ③ 从池移除并更新总权重 ---------- */
                total -= picked.Weight;
                pool.RemoveAt(idx);
            }

            return result;
        }
    }
}