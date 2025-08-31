using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace CARVES.Utls
{

    public static class GameLinqExtension
    {
        static readonly ThreadLocal<Random> RandomGenerator = new(() => new Random());

        public static T RandomPick<T>(this IEnumerable<T> enumerable, bool allowDefault, [CallerMemberName] string methodName = null)
        {
            var array = enumerable.ToArray();
            if (!allowDefault && array.Length == 0)
                throw new InvalidOperationException($"{methodName}.{nameof(RandomPick)}: array is null or empty!");

            if (array.Length == 0) return default;

            // 直接随机索引
            var randomIndex = RandomGenerator.Value.Next(array.Length);
            return array[randomIndex];
        }
        public static T RandomPick<T>(this IEnumerable<T> enumerable) => RandomPick(enumerable, false);
        public static T[] RandomTake<T>(this IEnumerable<T> enumerable, int take)
        {
            var array = enumerable.ToArray();
            if (take <= 0 || array.Length == 0)
                return Array.Empty<T>();

            // Fisher-Yates 洗牌算法
            var random = RandomGenerator.Value;
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }

            return array.Take(take).ToArray();
        }
    }
}
