using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CARVES.Data
{
    /// <summary>
    /// 数据映射合约。
    /// 只保留原始 <see cref="_initData"/> (key -> string) 供懒加载，子类自行维护缓存(可用各种字典)。<br/>
    /// 子类需实现 <see cref="CacheData"/> ：子类所有“已缓存”数据的 (key->string) 形式，用于最终合并
    /// </summary>
    public abstract class DataMappingContract<TKey> where TKey : notnull
    {
        /// <summary>
        /// 原始初始化数据 (key -> string)，
        /// 子类可以在 <see cref="GetExtendedValue{TValue}"/> 时懒加载。
        /// </summary>
        protected readonly ConcurrentDictionary<TKey, string> _initData;

        protected DataMappingContract(IDictionary<TKey, string> data)
        {
            // 父类只做简单拷贝，子类之后可随时从这里面 Deserialize.
            _initData = new ConcurrentDictionary<TKey, string>(data);
        }

        /// <summary>
        /// 子类在把数据打包成 (key->string) 
        /// 在 <see cref="ToStringDictionary"/> 中会先收集子类各字典，再合并未被覆盖的 initData。
        /// </summary>
        public Dictionary<TKey, string> ToStringDictionary()
        {
            var result = new Dictionary<TKey, string>();

            // 1. 让子类提供所有扩展字典
            foreach (var dic in CacheData)
            {
                foreach (var (key, strVal) in dic)
                {
                    if (!result.TryAdd(key, strVal))
                    {
                        throw new Exception($"Key conflict detected in child dictionary: {key}");
                    }
                }
            }

            // 2. 再补充 initData 里尚未覆盖的键
            foreach (var kvp in _initData)
            {
                if (!result.ContainsKey(kvp.Key))
                {
                    // 说明子类并没有覆盖这个 Key，就把原始字符串塞进去
                    result[kvp.Key] = kvp.Value;
                }
            }

            return result;
        }

        /// <summary>
        /// 子类返回若干个并行字典 (key->string)，表示子类“已缓存数据”的最终形态。
        /// 在 <see cref="ToStringDictionary"/> 时，会优先把这些覆盖到结果集里。
        /// </summary>
        protected abstract ConcurrentDictionary<TKey, string>[] CacheData { get; }
    }
}