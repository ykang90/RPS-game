using System;
using CARVES.Views;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CARVES.Core
{
    /// <summary>
    /// <see cref="IView"/>标准的生成器
    /// </summary>
    public class UiBuilder
    {
        Canvas SceneCanvas { get; }
        Res Res { get; }
        public UiBuilder(Canvas sceneCanvas, Res res)
        {
            SceneCanvas = sceneCanvas;
            Res = res;
        }
        /// <summary>
        /// 生成Ui
        /// </summary>
        /// <param name="resName">物体名字</param>
        /// <param name="buildAction">生成函数<see cref="IView"/>为传入物件</param>
        /// <param name="rootCanvas"></param>
        public async UniTask<T> Build<T>(string resName, Func<IView, T> buildAction, bool rootCanvas = true) =>
            await Build(resName, (_, v) => buildAction(v), rootCanvas);
        /// <summary>
        /// 生成Ui
        /// </summary>
        /// <param name="resName">物体名字</param>
        /// <param name="buildAction">生成函数<see cref="IView"/>为传入物件</param>
        /// /// <param name="callbackAfterInit">当初始化完成后执行</param>
        /// <param name="rootCanvas"></param>
        public void Build(string resName, Action<IView> buildAction, Action callbackAfterInit, bool rootCanvas = true) =>
            Build(resName, (_, v) => buildAction(v), callbackAfterInit, rootCanvas);
        /// <summary>
        /// 生成Ui
        /// </summary>
        /// <param name="resName">物体名字</param>
        /// <param name="buildAction">生成函数<see cref="IView"/>为传入物件</param>
        /// /// <param name="callbackAfterInit">当初始化完成后执行</param>
        /// <param name="parent"></param>
        public void Build(string resName, Action<IView> buildAction, Action callbackAfterInit, Transform parent) =>
            Build(resName, (_, v) => buildAction(v), callbackAfterInit, parent);
        /// <summary>
        /// 生成Ui
        /// </summary>
        /// <param name="resName">物体名字</param>
        /// <param name="buildAction">生成函数<see cref="GameObject"/>和<see cref="IView"/>为传入物件</param>
        /// /// <param name="callbackAfterInit">当初始化完成后执行</param>
        /// <param name="rootCanvas"></param>
        void Build(string resName, Action<GameObject, IView> buildAction, Action callbackAfterInit, bool rootCanvas = true) =>
            Build(resName, rootCanvas ? SceneCanvas.transform : null, go =>
            {
                if (go.GetComponent<View>() is not IView v)
                    throw new NullReferenceException($"{go.name} is not View!");
                buildAction(go, v);
            }, callbackAfterInit);
        /// <summary>
        /// 生成Ui
        /// </summary>
        /// <param name="resName">物体名字</param>
        /// <param name="buildAction">生成函数<see cref="GameObject"/>和<see cref="IView"/>为传入物件</param>
        /// <param name="rootCanvas"></param>
        async UniTask<T> Build<T>(string resName, Func<GameObject, IView, T> buildAction, bool rootCanvas = true) =>
            await Build(resName, rootCanvas ? SceneCanvas.transform : null, go =>
            {
                if (go.GetComponent<View>() is not IView v)
                    throw new NullReferenceException($"{go.name} is not View!");
                return buildAction(go, v);
            });

        /// <summary>
        /// 生成Ui
        /// </summary>
        /// <param name="resName">物体名字</param>
        /// <param name="buildAction">生成函数<see cref="GameObject"/>和<see cref="IView"/>为传入物件</param>
        /// <param name="callbackAfterInit">当初始化完成后执行</param>
        /// <param name="parent"></param>
        void Build(string resName, Action<GameObject, IView> buildAction, Action callbackAfterInit, Transform parent) =>
            Build(resName, parent, go =>
            {
                if (go.GetComponent<View>() is not IView v)
                    throw new NullReferenceException($"{go.name} is not View!");
                buildAction(go, v);
            }, callbackAfterInit);

        /// <summary>
        /// 生成Ui
        /// </summary>
        /// <param name="resName">物体名字</param>
        /// <param name="buildAction">生成函数<see cref="GameObject"/>为传入物件</param>
        /// <param name="callbackAfterInit">当初始化完成后执行</param>
        /// <param name="rootCanvas"></param>
        void Build(string resName, Action<GameObject> buildAction, Action callbackAfterInit,
            bool rootCanvas = true) =>
            Build(resName, rootCanvas ? SceneCanvas.transform : null, buildAction, callbackAfterInit);

        async void Build(string resName, Transform parent, Action<GameObject> buildAction, Action callbackAfterInit)
        {
            var task = Res.InstantiateAsync(resName, parent);
            await task.Task;
            var obj = task.Result;
            buildAction.Invoke(obj);
            callbackAfterInit?.Invoke();
        }

        async UniTask<T> Build<T>(string resName, Transform parent, Func<GameObject, T> buildAction)
        {
            var task = Res.InstantiateAsync(resName, parent);
            await task.Task;
            var obj = task.Result;
            return buildAction.Invoke(obj);
        }
    }
}