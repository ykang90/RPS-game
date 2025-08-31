using UnityEngine;

namespace CARVES.Core
{
    /// <summary>
    /// 初始化规范的控件。必须执行一次<see cref="InitMonoBase.Init"/>才能实际上使用的控件。<br/>
    /// 主要是为了通用Init接口入参<see cref="object"/>强转来实现扩展参数的需求<br/>
    /// 子类可以手动调用<see cref="Init"/>或利用<see cref="Start"/>函数来实现自动初始化。
    /// </summary>
    public abstract class InitMonoBase : InitMonoBase<object>
    {
        
    }
    /// <summary>
    /// 初始化规范的控件。必须执行一次<see cref="Init"/>才能实际上使用的控件。<br/>
    /// 主要是为了Init泛型入参<see cref="TParam"/>来实现扩展参数的需求<br/>
    /// 子类可以手动调用<see cref="Init"/>或利用<see cref="Start"/>函数来实现自动初始化。
    /// </summary>
    /// <typeparam name="TParam"></typeparam>
    public abstract class InitMonoBase<TParam> : MonoBehaviour
    {
        /// <summary>
        /// 初始化插件<br/>
        /// 用内<see cref="record"/>来实现初始化参数的传递如:<br/>
        /// public record InitRecord(int a,int b);<br/>
        /// </summary>
        /// <param name="arg"></param>
        public void Init(TParam arg)
        {
            OnInit(arg);
        }

        protected abstract void OnInit(TParam arg);
    }
}