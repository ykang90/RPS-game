using System;
using System.Collections.Generic;

namespace CARVES.Utls
{
    public static class EnumerableExtensions
    {
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
            TValue defaultValue = default) =>
            GetOrDefault(dictionary, key, (success, value) => success ? value : defaultValue);
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
            Func<bool,TValue,TValue> middleFunc)
        {
            var result = dictionary.TryGetValue(key, out var value);
            return middleFunc(result, value);
        }

        public static TValue[] SingleElementToArray<TValue>(this TValue obj) => new[] { obj };
        public static List<TValue> SingleElementToList<TValue>(this TValue obj) => new() { obj };
    }
}