using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace CARVES.Core
{
    public interface IMainThreadDispatcher
    {
        void Enqueue(Action action);
    }

    public class MainThreadDispatcher : MonoBehaviour, IMainThreadDispatcher
    {
        static readonly ConcurrentQueue<Action> _executionQueue = new ConcurrentQueue<Action>();
        static MainThreadDispatcher _instance;
        public bool Persist;

        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                if (Persist) DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void Enqueue(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
#if UNITY_EDITOR
            action(); // 在编辑器模式下直接执行
#else
        _executionQueue.Enqueue(action);
#endif
        }

        void Update()
        {
            while (_executionQueue.TryDequeue(out var action)) 
                action.Invoke();
        }
    }
}