using System;
using System.Collections.Generic;

namespace CARVES.Core
{
    /// <summary>
    /// 逻辑控制器, 主要控制与模型和数据的交互
    /// </summary>
    public interface IController 
    {
    }
    /// <summary>
    /// 基于<see cref="IController"/>的DI容器
    /// </summary>
    public class ControllerServiceContainer
    {
        Dictionary<object, IController> Container { get; set; } = new();

        public T Get<T>() where T : class, IController
        {
            if (!TryGet<T>(out var obj))
                throw new NotImplementedException($"{typeof(T)} hasn't register!");
            return obj;
        }

        public bool TryGet<T>(out T c) where T : class, IController
        {
            if(!Container.TryGetValue(typeof(T), out var obj))
            {
                c = default;
                return false;
            }
            c = obj as T;
            return true;
        }
        public void Reg<T>(T controller) where T : class, IController
        {
            Container.Add(controller.GetType(), controller);
        }
    }
}