using System;
using System.Collections.Generic;

namespace CARVES.Core
{
    /// <summary>
    /// 袖珍注入器，提供简单的依赖注入功能。<br/>
    /// </summary>
    public sealed class TinyInjector
    {
        readonly Dictionary<Type, Func<object>> _factories = new();
        readonly Dictionary<Type, object> _singletons = new();

        public void RegSingleton<T>(T instance) =>
            _singletons[typeof(T)] = instance;

        public void RegFactory<T>(Func<T> factory) =>
            _factories[typeof(T)] = () => factory();

        public T Get<T>()
        {
            var t = typeof(T);
            if (_singletons.TryGetValue(t, out var inst)) return (T)inst;
            if (!_factories.TryGetValue(t, out var f)) throw new InvalidOperationException($"Type {t} not registered");
            var obj = (T)f();
            return obj;
        }
    }
}