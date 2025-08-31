using System;
using System.Collections.Generic;

namespace CARVES.Utls
{
    /// <summary>
    /// 仅用于实现了的功能，这只是个简化的更新通知字典。
    /// </summary>
    public class ObservableDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        // 定义事件，用于通知字典更新
        public event Action<TKey, TValue>? ItemAdded;
        public event Action<TKey, TValue>? ItemUpdated;
        public event Action<TKey>? ItemRemoved;

        // 添加项目
        public new void Add(TKey key, TValue value)
        {
            base.Add(key, value);
            ItemAdded?.Invoke(key, value);
        }

        // 更新项目
        public new TValue this[TKey key]
        {
            get => base[key];
            set
            {
                var isUpdate = base.ContainsKey(key);
                base[key] = value;
                if (isUpdate)
                {
                    ItemUpdated?.Invoke(key, value);
                }
                else
                {
                    ItemAdded?.Invoke(key, value);
                }
            }
        }

        // 删除项目
        public new bool Remove(TKey key)
        {
            var result = base.Remove(key);
            if (result)
            {
                ItemRemoved?.Invoke(key);
            }
            return result;
        }
    }
}