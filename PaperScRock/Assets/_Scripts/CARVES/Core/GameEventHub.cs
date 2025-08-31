using System;
using System.Collections.Generic;
using System.Linq;
using CARVES.Utls;
using UnityEngine;

namespace CARVES.Core
{
    /// <summary>
    /// 事件消息系统
    /// </summary>
    public class GameEventHub
    {
        Dictionary<string, Dictionary<string, Action<string>>> EventMap { get; set; } = new();

        /// <summary>
        /// 发送事件, 所有物件将以<see cref="DataBag"/>序列化<br/>
        /// 如果不传参不会序列化，性能更好
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="objs"></param>
        public void Send(string eventName, params object[] objs) =>
            SendSerialized(eventName, objs.Any() ? DataBag.Serialize(objs) : string.Empty);

        /// <summary>
        /// 发送事件, 参数为<see cref="string"/>
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="dataBag"></param>
        public void SendSerialized(string eventName, string dataBag)
        {
            if (EventMap.TryGetValue(eventName, out var value))
                foreach (var (_, action) in value)
                    action?.Invoke(string.IsNullOrEmpty(dataBag) ? string.Empty : dataBag);
            else
            {
                $"{eventName} 没有注册事件!".Log(this, LogType.Warning);
            }
        }

        string RegEvent(string eventName, Action<string> action)
        {
            if (!EventMap.ContainsKey(eventName))
            {
                EventMap.Add(eventName, new Dictionary<string, Action<string>>());
            }

            var key = Guid.NewGuid().ToString();
            EventMap[eventName].Add(key, action);
            return key;
        }

        /// <summary>
        /// 注册<see cref="DataBag"/>事件
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public string RegEvent(string eventName, Action<DataBag> action)
        {
            return RegEvent(eventName, ObjBagSerialize);

            void ObjBagSerialize(string arg) => action?.Invoke(DataBag.Deserialize(arg));
        }

        /// <summary>
        /// 删除事件方法(仅仅是删除一个事件方法, 其余的监听方法依然有效)
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="key"></param>
        public void RemoveEvent(string eventName, string key)
        {
            if (EventMap[eventName].ContainsKey(key))
                EventMap[eventName].Remove(key);
        }

        public void Clear() => EventMap.Clear();
    }
}
