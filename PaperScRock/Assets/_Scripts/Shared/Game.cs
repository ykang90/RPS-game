using System;
using System.Collections;
using CARVES.Core;
using CARVES.Utls;
using UnityEngine;
using UnityEngine.Events;

public class Game
{
    static ControllerServiceContainer ServiceContainer { get; set; } = new();
    public static Camera Camera { get; private set; }
    static GameEventHub EventHub { get; set; } = new();
    public static IMainThreadDispatcher MainThread
    {
        get
        {
            if (!_mainThread) 
                _mainThread = new GameObject("MainThreadDispatcher").AddComponent<MainThreadDispatcher>();
            return _mainThread;
        }
    }
    static MainThreadDispatcher _mainThread;
    public static MonoService MonoService
    {
        get
        {
            if (!_monoService)
                _monoService = new GameObject("MonoService").AddComponent<MonoService>();
            return _monoService;
        }
    }
    static MonoService _monoService;
    static UpdateManager UpdateManager
    {
        get
        {
            if (!_updateManager)
                _updateManager = new GameObject("UpdateManager").AddComponent<UpdateManager>();
            return _updateManager;
        }
    }
    static UpdateManager _updateManager;
    static Game Instance { get; } = new();
    public static bool IsPaused { get; private set; }
    bool isRunning = false;
    //public static GameWorld World { get; } = new();

    public static void Run(UnityAction onGameStartCallback)
    {
        if (Instance.isRunning)
            throw new Exception("游戏已经启动!"); // 防止重复运行
        Instance.isRunning = true;
        Camera = Camera.main;
        // 其他初始化逻辑...
        StartCoService(NextFrameInit());

        IEnumerator NextFrameInit()
        {
            yield return null;
            onGameStartCallback();
        }
    }


    public static T GetController<T>() where T : class, IController, new()
    {
        if (ServiceContainer.TryGet<T>(out var c)) return c;
        c = new T();
        ServiceContainer.Reg(c);
        return c;
    }

    public static void SendEvent(string eventName, DataBag bag) => EventHub.Send(eventName, bag);

    public static void SendEvent(string eventName, params object[] args)
    {
        args ??= Array.Empty<object>();
        EventHub.Send(eventName, args);
    }

    public static string RegEvent(string eventName, Action<DataBag> callbackAction) =>
        EventHub.RegEvent(eventName, callbackAction);

    public static void RemoveEvent(string eventName, string key) => EventHub.RemoveEvent(eventName, key);
    //public static void PlayBGM(AudioClip clip) => AudioComponent.Play(AudioManager.Types.BGM,clip);
    //public static void PlaySFX(AudioClip clip) => AudioComponent.Play(AudioManager.Types.SFX, clip);

    /// <summary>
    /// 开始协程服务
    /// </summary>
    /// <param name="coroutineFunc"></param>
    /// <returns></returns>
    public static Coroutine StartCoService(IEnumerator coroutineFunc) => MonoService.StartCoroutine(coroutineFunc);

    /// <summary>
    /// 停止协程服务
    /// </summary>
    /// <param name="coroutine"></param>
    public static void StopCoService(Coroutine coroutine)
    {
        if (coroutine == null) return;
        MonoService.StopCoroutine(coroutine);
    }

    public static RaycastHit? MouseRayOnLayer(LayerMask layer, int distance = 50)
    {
        var ray = Camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, distance, layer)) return hit;
        return null;
    }

    public static void RegUpdate(IUpdatable updatable) => UpdateManager.Register(updatable);
    public static void UnRegUpdate(IUpdatable updatable) => UpdateManager.Unregister(updatable);

    static void SetPause(bool pause)
    {
        IsPaused = pause;
        UpdateManager.Pause(pause);
    }

    public static void Pause(bool withTimeScale = false)
    {
        if (withTimeScale) Time.timeScale = 0;
        SetPause(true);
    }

    public static void Resume()
    {
        SetPause(false);
        Time.timeScale = 1;
    }
}