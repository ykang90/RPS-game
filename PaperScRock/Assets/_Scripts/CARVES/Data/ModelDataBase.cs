using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CARVES.Data
{
    /// <summary>
    /// 这是基础模型类，维护了 5 个专用字典:
    ///   (key -> string), (key -> int), (key -> double), (key -> float), (key -> bool), (key-> object)。
    /// 并把它们合并到父类的序列化流程中。<br/>
    /// 子类通过 ExtendedSerializeValue / ExtendedDeserializeValue 可以覆盖其中的序列化逻辑。
    /// 需要额外维护的属性可放到 SelfManagedSerializeData 中。<br/>
    /// 使用手册：<br/>
    /// 一般使用，只需继承此类，声明一个枚举或是string类型作为属性的<see cref="TProp"/>，然后对于属性调用Get与Set方法即可。如果要性能优化可以根据对应的5大类型调用对应的GetXXX与SetXXX方法。<br/>
    /// <br/>
    /// 特殊属性类型或不在5大类型的属性需要实现一下的扩展方法:<br/>
    /// 1. <see cref="SelfManagedSerializeData"/>：声明额外维护的属性，这些属性不会被默认的序列化逻辑处理，需要自行处理。<br/>
    /// 2. <see cref="ExtendedDeserializeValue{TValue}"/>/<see cref="ExtendedSerializeValue"/>：<br/>
    /// - 声明除了默认的5个(string/int/double/float/bool)之外的类型的序列化逻辑。也可以改写默认的逻辑。<br/>
    /// - 但必须在 ExtendedSerializeValue / ExtendedDeserializeValue 中处理所有 SelfManagedSerializeData 中的属性。<br/>
    /// 注意：由于特殊类型不会缓存到5大缓存字典，所以<see cref="Get{TValue}"/>, <see cref="Set{TValue}"/>方法(包括<c>GetXXX</c>,<c>SetXXX</c>)将不适用。<br/>如果想获取初始数据，用<see cref="GetInitValue{TValue}"/>读取序列化数据<br/>
    /// </summary>
    public abstract class ModelDataBase<TProp> : DataMappingContract<TProp>
        where TProp : notnull
    {
        public const string BoolTrue = "1";
        public const string BoolFalse = "0";

        //=== 1) 5个原有专用字典 + 1个 object 字典 ===
        private readonly ConcurrentDictionary<TProp, string> _stringDic = new();
        private readonly ConcurrentDictionary<TProp, int> _intDic = new();
        private readonly ConcurrentDictionary<TProp, double> _doubleDic = new();
        private readonly ConcurrentDictionary<TProp, float> _floatDic = new();
        private readonly ConcurrentDictionary<TProp, bool> _boolDic = new();
        private ConcurrentDictionary<TProp, TValue> GetDictionary<TValue>()
        {
            return typeof(TValue) switch
            {
                Type t when t == typeof(string) => _stringDic as ConcurrentDictionary<TProp, TValue> ?? throw new InvalidOperationException("Dictionary type mismatch."),
                Type t when t == typeof(int) => _intDic as ConcurrentDictionary<TProp, TValue> ?? throw new InvalidOperationException("Dictionary type mismatch."),
                Type t when t == typeof(double) => _doubleDic as ConcurrentDictionary<TProp, TValue> ?? throw new InvalidOperationException("Dictionary type mismatch."),
                Type t when t == typeof(float) => _floatDic as ConcurrentDictionary<TProp, TValue> ?? throw new InvalidOperationException("Dictionary type mismatch."),
                Type t when t == typeof(bool) => _boolDic as ConcurrentDictionary<TProp, TValue> ?? throw new InvalidOperationException("Dictionary type mismatch."),
                _ => throw new NotSupportedException($"GetDictionary: Unsupported type {typeof(TValue)}"),
            };
        }

        //=== 2) 构造函数 ===
        protected ModelDataBase(IDictionary<TProp, string> data)
            : base(data)
        {
        }

        //-------------------------------------------------------------------------
        // 3) 提供 Set / Get 接口让外部访问；存取到相应的字典
        //-------------------------------------------------------------------------


        /// <summary>
        /// 设置一个值，自动分配到 5 大专用字典或 _objectDic。
        /// </summary>
        protected void Set<TValue>(TProp prop, TValue value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value), $"Value for prop {prop} cannot be null.");
            // 检查键是否已经存在于其他字典
            if (_stringDic.ContainsKey(prop) || _intDic.ContainsKey(prop) || _doubleDic.ContainsKey(prop) ||
                _floatDic.ContainsKey(prop) || _boolDic.ContainsKey(prop))
            {
                // 根据需求选择是覆盖还是抛出异常
                // 例如，抛出异常防止多次赋值不同类型
                throw new InvalidOperationException($"Key '{prop}' is already set in another dictionary.");
            }

            var added = false;
            switch (value)
            {
                case string s:
                    added = _stringDic.TryAdd(prop, s);
                    if (!added) throw new InvalidOperationException($"Key '{prop}' is already set in _stringDic.");
                    break;
                case int i:
                    added = _intDic.TryAdd(prop, i);
                    if (!added) throw new InvalidOperationException($"Key '{prop}' is already set in _intDic.");
                    break;
                case double d:
                    added = _doubleDic.TryAdd(prop, d);
                    if (!added) throw new InvalidOperationException($"Key '{prop}' is already set in _doubleDic.");
                    break;
                case float f:
                    added = _floatDic.TryAdd(prop, f);
                    if (!added) throw new InvalidOperationException($"Key '{prop}' is already set in _floatDic.");
                    break;
                case bool b:
                    added = _boolDic.TryAdd(prop, b);
                    if (!added) throw new InvalidOperationException($"Key '{prop}' is already set in _boolDic.");
                    break;
                default: throw new NotSupportedException($"Unsupported type: {value.GetType()}");
            }
        }

        /// <summary>
        /// 获取一个值。如果找不到 prop 或类型不匹配，就返回 defaultValue。
        /// 若都没有，则懒加载 _initData 并存入对应字典。
        /// </summary>
        protected TValue Get<TValue>(TProp prop, TValue defaultValue = default!)
        {
            var dic = GetDictionary<TValue>();
            if (dic.TryGetValue(prop, out var value))
                return value;

            // 懒加载并确保线程安全
            var loaded = GetInitValue(prop, defaultValue);
            var added = loaded switch
            {
                string s => _stringDic.TryAdd(prop, s),
                int i => _intDic.TryAdd(prop, i),
                double d => _doubleDic.TryAdd(prop, d),
                float f => _floatDic.TryAdd(prop, f),
                bool b => _boolDic.TryAdd(prop, b),
                _ => throw new NotSupportedException($"Unsupported type: {loaded.GetType()}")
            };
            return loaded;
        }

        /// <summary>
        /// 从 _initData 里获取 rawText 并反序列化为 TValue；若不存在则返回 defaultValue。<br/>
        /// 一般用Get就可以获取数据，但如果是某些自定义类型，需要自行处理反序列化逻辑，可以用此方法。获取到最初数据。
        /// </summary>
        protected TValue GetInitValue<TValue>(TProp prop, TValue defaultValue = default!) =>
            !_initData.TryGetValue(prop, out var rawText) ? defaultValue : DeserializeValue<TValue>(prop, rawText);

        //-------------------------------------------------------------------------
        // 4) 转换为 (key->string) 的部分 => 供 ToStringDictionary() 使用
        //-------------------------------------------------------------------------

        protected override ConcurrentDictionary<TProp, string>[] CacheData
        {
            get
            {
                var dicStr = SerializeDict(_stringDic);
                var dicInt = SerializeDict(_intDic);
                var dicDou = SerializeDict(_doubleDic);
                var dicFlt = SerializeDict(_floatDic);
                var dicBool = SerializeDict(_boolDic);

                // SelfManagedSerializeData: 把它们先做一个空串集合再存成 ConcurrentDictionary
                var dicSelf = SelfManagedSerializeData != null
                    ? new ConcurrentDictionary<TProp, string>(SelfManagedSerializeData.Select(o =>
                        new KeyValuePair<TProp, string>(o.Key, SerializeValue(o.Key, o.Value))))
                    : new();
                return new[] { dicStr, dicInt, dicDou, dicFlt, dicBool, dicSelf };

                ConcurrentDictionary<TProp, string> SerializeDict<TVal>(ConcurrentDictionary<TProp, TVal> source)
                {
                    var result = new ConcurrentDictionary<TProp, string>();
                    foreach (var (key, value) in source)
                    {
                        // 调用子类的序列化逻辑
                        var serialized = SerializeValue(key, value);
                        result[key] = serialized;
                    }
                    return result;
                }
            }
        }

        /// <summary>
        /// 子类自行维护的一些额外键(或字典)。
        /// 在序列化时会用空串覆盖 initData，具体内容需子类在 ExtendedSerializeValue 等处处理。
        /// </summary>
        protected virtual Dictionary<TProp, object> SelfManagedSerializeData { get; } = new();

        //-------------------------------------------------------------------------
        // 5) 一些便捷的 GetXxx / SetXxx
        //-------------------------------------------------------------------------
        /// <summary>
        /// <see cref="string"/>的直接设置方法，性能更好
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="value"></param>
        public void SetString(TProp prop, string value) => _stringDic[prop] = value;
        /// <summary>
        /// <see cref="string"/>的直接获取方法，性能更好
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public string GetString(TProp prop, string defaultValue = "")
        {
            if (_stringDic.TryGetValue(prop, out var value)) return value;
            var loaded = GetInitValue(prop, defaultValue);
            _stringDic[prop] = loaded;
            return loaded;
        }
        /// <summary>
        /// <see cref="int"/>的直接设置方法，性能更好
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="value"></param>
        public void SetInt(TProp prop, int value) => _intDic[prop] = value;
        /// <summary>
        /// <see cref="int"/>的直接获取方法，性能更好
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public int GetInt(TProp prop, int defaultValue = 0)
        {
            if (_intDic.TryGetValue(prop, out var value)) return value;
            var loaded = GetInitValue(prop, defaultValue);
            _intDic[prop] = loaded;
            return loaded;
        }
        /// <summary>
        /// <see cref="double"/>的直接设置方法，性能更好
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="value"></param>
        public void SetDouble(TProp prop, double value) => _doubleDic[prop] = value;
        /// <summary>
        /// <see cref="double"/>的直接获取方法，性能更好
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public double GetDouble(TProp prop, double defaultValue = 0.0)
        {
            if (_doubleDic.TryGetValue(prop, out var value)) return value;
            var loaded = GetInitValue(prop, defaultValue);
            _doubleDic[prop] = loaded;
            return loaded;
        }
        /// <summary>
        /// <see cref="float"/>的直接设置方法，性能更好
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="value"></param>
        public void SetFloat(TProp prop, float value) => _floatDic[prop] = value;
        /// <summary>
        /// <see cref="float"/>的直接获取方法，性能更好
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public float GetFloat(TProp prop, float defaultValue = 0)
        {
            if (_floatDic.TryGetValue(prop, out var value)) return value;
            var loaded = GetInitValue(prop, defaultValue);
            _floatDic[prop] = loaded;
            return loaded;
        }
        /// <summary>
        /// <see cref="bool"/>的直接设置方法，性能更好
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="value"></param>
        public void SetBool(TProp prop, bool value) => _boolDic[prop] = value;
        /// <summary>
        /// <see cref="bool"/>的直接获取方法，性能更好
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public bool GetBool(TProp prop, bool defaultValue = false)
        {
            if (_boolDic.TryGetValue(prop, out var value)) return value;
            var loaded = GetInitValue(prop, defaultValue);
            _boolDic[prop] = loaded;
            return loaded;
        }

        //-------------------------------------------------------------------------
        // 6) (反)序列化：允许子类覆盖
        //-------------------------------------------------------------------------

        /// <summary>
        /// 子类可覆盖此方法，根据 key+value 做特殊序列化，设置 hasExtended=true 表示已处理。
        /// </summary>
        protected abstract string ExtendedSerializeValue(TProp key, object value, out bool hasExtended);

        /// <summary>
        /// 将某个泛型值序列化为字符串；默认支持 5 大类型，否则抛异常。
        /// 如果想支持更多类型，需要在 ExtendedSerializeValue 做自定义处理。
        /// </summary>
        protected virtual string SerializeValue<TValue>(TProp key, TValue value)
        {
            var str = ExtendedSerializeValue(key, value!, out var hasExtended);
            if (hasExtended) return str;
            if (SelfManagedSerializeData.ContainsKey(key))
                throw new NotSupportedException(
                    $"Key: {key} is registered in SelfManagedSerializeData, but not found in ExtendedSerializeValue.");

            // 默认处理 5 种 + bool
            return value switch
            {
                string s => s,
                int i => i.ToString(),
                double d => d.ToString(CultureInfo.InvariantCulture),
                float f => f.ToString(CultureInfo.InvariantCulture),
                bool b => b ? BoolTrue : BoolFalse,
                _ => throw new NotSupportedException($"SerializeValue: Unsupported type: {typeof(TValue)}")
            };
        }

        /// <summary>
        /// 子类可覆盖此方法，根据 key+rawString 做特殊反序列化，设置 hasExtended=true 表示已处理。
        /// </summary>
        protected abstract TValue ExtendedDeserializeValue<TValue>(TProp key, string value, out bool hasExtended);

        /// <summary>
        /// 反序列化一个字符串到 TValue；默认支持 5 大类型 + bool，否则抛异常。
        /// </summary>
        protected virtual TValue DeserializeValue<TValue>(TProp key, string value)
        {
            var result = ExtendedDeserializeValue<TValue>(key, value, out var hasExtended);
            if (hasExtended) return result;
            var type = typeof(TValue);
            return type switch
            {
                Type _ when type == typeof(string) => (TValue)(object)value,
                Type _ when type == typeof(int) => (TValue)(object)int.Parse(value),
                Type _ when type == typeof(double) => (TValue)(object)double.Parse(value, CultureInfo.InvariantCulture),
                Type _ when type == typeof(float) => (TValue)(object)float.Parse(value, CultureInfo.InvariantCulture),
                Type _ when type == typeof(bool) => (TValue)(object)(value == BoolTrue),
                _ => throw new NotSupportedException($"DeserializeValue: Unsupported type: {typeof(TValue)}")
            };
        }
    }
}