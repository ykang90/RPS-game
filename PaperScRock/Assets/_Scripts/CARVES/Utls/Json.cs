using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace CARVES.Utls
{
    /// <summary>
    ///  数据袋,作为数据传输的载体标准
    /// </summary>
    public sealed record DataBag
    {
        /// <summary>
        /// 序列化数据袋
        /// </summary>
        /// <param name="name"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string SerializeWithName(string name, params object[] data)
        {
            var bag = new DataBag();
            bag.Data = data;
            bag.Size = data.Length;
            bag.DataName = name;
            return Json.Serialize(bag);
        }
        /// <summary>
        /// 序列化数据袋
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Serialize(params object[] data)
        {
            var bag = new DataBag();
            bag.Data = data;
            bag.Size = data.Length;
            bag.DataName = $"Data[{data.Length}]";
            return Json.Serialize(bag);
        }

        /// <summary>
        /// 数据袋反序列
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static DataBag? Deserialize(object? obj)
        {
            if (obj == null) return null;
            var dto = Json.Deserialize<DataBag>(obj.ToString());
            return dto;
        }

        public string DataName { get; set; }
        public object[] Data { get; set; }
        public int Size { get; set; }

        public T Get<T>(int index)
        {
            if (Data.Length <= index)
                throw new IndexOutOfRangeException($"The requested index {index} is out of range. The length of the Data array is {Data.Length}.");
            var value = Data[index];
            return Parse<T>(value);
        }

        public string Serialize() => Json.Serialize(this);

        static Type floatType = typeof(float);
        static Type doubleType = typeof(double);
        static Type longType = typeof(long);
        static Type intType = typeof(int);
        static Type stringType = typeof(string);
        static Type jTokenType = typeof(JToken);

        public static T Parse<T>(object value)
        {
            var t = typeof(T);
            if (value == default) return default;
            try
            {
                if (t.IsEnum)
                {
                    // 尝试将值转换为 int 再转成对应的枚举类型
                    if (value is int intValue)
                        return (T)Enum.ToObject(t, intValue);
                    if(value is string stringValue)
                        return (T)Enum.Parse(t, stringValue);
                    // 如果 value 不是 int，尝试将其转换为 int 再处理
                    var intParsedValue = Convert.ToInt32(value);
                    return (T)Enum.ToObject(t, intParsedValue);
                }
                if (t == doubleType) return (T)Convert.ChangeType(value, t);
                if (t == floatType) return (T)Convert.ChangeType(value, t);
                if (t == longType) return (T)Convert.ChangeType(value, t);
                if (t == intType) return (T)Convert.ChangeType(value, t);
                if (t == stringType) return (T)Convert.ChangeType(value, t);
                if (value is JToken token) return token.ToObject<T>();
                return (T)value;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
    public static class Json
    {
        public static JsonSerializerSettings Settings => new JsonSerializerSettings
        {
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new PrivateSetterContractResolver()
        };

        public static string JListAction<T>(string jList, Action<List<T>> action)
        {
            var list = Deserialize<List<T>>(jList) ?? new List<T>();
            action.Invoke(list);
            return Serialize(list);
        }

        public static string JObjAction<T>(string jObj, Action<T> action) where T : class, new()
        {
            var obj = Deserialize<T>(jObj) ?? new T();
            action.Invoke(obj);
            return Serialize(obj);
        }

        public static TResult JListAction<T, TResult>(string jList, System.Func<List<T>, TResult> function)
        {
            var list = Deserialize<List<T>>(jList) ?? new List<T>();
            return function.Invoke(list);
        }

        public static object[] DeserializeObjs(string value) => Deserialize<object[]>(value);
        public static string Serialize(object obj) => JsonConvert.SerializeObject(obj, Settings);

        public static T Deserialize<T>(string value)
        {
            try
            {
                return value == null ? default : JsonConvert.DeserializeObject<T>(value, Settings);
            }
            catch (Exception e)
            {
#if DEBUG
                $"Json Err:{e}".Log(nameof(Json), LogType.Warning);
#endif
                return default;
            }
        }

        public static T Deserialize<T>(string value, JsonConverter[] converters)
        {
            try
            {
                return value == null ? default : JsonConvert.DeserializeObject<T>(value, converters);
            }
            catch (Exception e)
            {
#if DEBUG
                $"Json Err:{e}".Log(nameof(Json), LogType.Warning);
#endif
                return default;
            }
        }

        public static T Deserialize<T>(string value, IContractResolver resolver) where T : class
        {
            try
            {
                return value == null
                    ? null
                    : JsonConvert.DeserializeObject<T>(value, new JsonSerializerSettings
                    {
                        ContractResolver = resolver
                    });
            }
            catch (Exception e)
            {
#if DEBUG
                $"Json Err:{e}".Log(nameof(Json), LogType.Warning);
#endif
                return null;
            }
        }

        public static List<T> DeserializeList<T>(string jList, params JsonConverter[] converters) =>
            string.IsNullOrWhiteSpace(jList) ? new List<T>() : Deserialize<List<T>>(jList, converters);

        public static List<T> DeserializeList<T>(string jList, IContractResolver resolver) =>
            string.IsNullOrWhiteSpace(jList) ? new List<T>() : Deserialize<List<T>>(jList, resolver);

        public static List<T> DeserializeList<T>(string jList) =>
            string.IsNullOrWhiteSpace(jList) ? new List<T>() : Deserialize<List<T>>(jList);

    }

    public class PrivateSetterContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var jProperty = base.CreateProperty(member, memberSerialization);
            if (jProperty.Writable)
                return jProperty;

            jProperty.Writable = member.IsPropertyWithSetter();

            return jProperty;
        }
    }

    public class PrivateSetterCamelCasePropertyNamesContractResolver : CamelCasePropertyNamesContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var jProperty = base.CreateProperty(member, memberSerialization);
            if (jProperty.Writable)
                return jProperty;

            jProperty.Writable = member.IsPropertyWithSetter();

            return jProperty;
        }
    }

    internal static class MemberInfoExtensions
    {
        internal static bool IsPropertyWithSetter(this MemberInfo member)
        {
            var property = member as PropertyInfo;

            return property?.SetMethod != null;
        }
    }
}