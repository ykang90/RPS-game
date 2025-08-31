using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CARVES.Utls
{
    public static class XArg
    {
        public static ArgumentException Exception<T>(string message, T arg, [CallerMemberName] string method = null!)
            where T : class => new(Error(message, arg, method));

        public static ArgumentException Exception<T>(T arg, [CallerMemberName] string method = null!) where T : class =>
            Exception(string.Empty, arg, method);

        public static string Error<T>(T arg, [CallerMemberName] string method = null!) where T : class =>
            Error("Error", arg, method);

        public static string Error<T>(string message, T arg, [CallerMemberName] string method = null!) where T : class
        {
            var type = arg.GetType();
            var param = type
               .GetProperties()
               .Select(p => $"{p.Name}= {Format(p.GetValue(arg))}");
            param = param.Concat(type.GetFields()
               .Select(f => $"{f.Name}= {Format(f.GetValue(arg))}"));
            return $"{method}: {message}. on arg: {string.Join(',', param)}";
        }

        public static string Format(object? value)
        {
            if (value == null) return "null";

            // 检查是否是字典
            if (value is IDictionary dictionary)
            {
                var dictionaryEntries = dictionary.Cast<DictionaryEntry>()
                   .Select(de => $"[{de.Key}, {de.Value?.ToString() ?? "null"}]");
                return $"{{{string.Join(", ", dictionaryEntries)}}}";
            }

            // 检查是否是 IEnumerable
            if (value is IEnumerable enumerable && !(value is string))
            {
                var items = enumerable.Cast<object>()
                   .Select(item => item?.ToString() ?? "null");
                return $"[{string.Join(", ", items)}]";
            }

            // 否则直接返回 ToString() 结果
            return value.ToString() ?? "null";
        }
    }
}