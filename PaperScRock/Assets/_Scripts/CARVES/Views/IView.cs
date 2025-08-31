using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CARVES.Views
{
    public interface IPage : IView
    {

    }

    /// <summary>
    /// 基于Unity的通用Ui整合标准
    /// </summary>
    public interface IView
    {
        IReadOnlyDictionary<string, GameObject> GetMap();
        RectTransform RectTransform { get; }
        GameObject GameObject { get; }
        GameObject[] GetObjects();
        GameObject Get(string objName);
        T Get<T>(string objName);
        T Get<T>(int index);
        T GetComponent<T>();
        Coroutine StartCo(IEnumerator enumerator);
        void StopCo(Coroutine coroutine);
        void StopAllCo();
        void SetWidth(float width);
        void SetHeight(float height);
        void SetSize(float width, float height);
        event Action OnDisableEvent;
        event Action OnEnableEvent; 
        string name { get; }
        View GetView();
        Object GetRes(string resName);
        T GetRes<T>(int index) where T : Object;
        T GetRes<T>(string resName) where T : Object;
        void PrintComponents();
    }
}