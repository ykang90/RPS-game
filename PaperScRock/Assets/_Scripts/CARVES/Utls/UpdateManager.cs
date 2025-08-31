using System.Collections.Generic;
using UnityEngine;

namespace CARVES.Utls
{
    /// <summary>
    /// Interface for objects that need per-frame updates via the UpdateManager.
    /// </summary>
    public interface IUpdatable
    {
        void OnUpdate();
        GameObject gameObject { get; }
    }
    // 使用示例：
    // public class MyService : IUpdatable
    // {
    //     void OnEnable() => UpdateManager.Instance?.Register(this);
    //     void OnDisable() => UpdateManager.Instance?.Unregister(this);
    //     public void OnUpdate()
    //     {
    //         // Your per-frame logic here
    //     }
    // }

    /// <summary>
    /// A global Update Manager to centralize per-frame updates and support pausing.
    /// </summary>
    public class UpdateManager : MonoBehaviour
    {
        //public static UpdateManager Instance { get; private set; }

        // List of registered updatables
        private readonly List<IUpdatable> _updatables = new List<IUpdatable>();

        /// <summary>
        /// When true, registered updatables will not receive OnUpdate calls.
        /// </summary>
        public bool IsPaused { get; private set; } = false;

        //void Awake()
        //{
        //    if (Instance && Instance != this)
        //    {
        //        Destroy(gameObject);
        //        return;
        //    }
        //    Instance = this;
        //    DontDestroyOnLoad(gameObject);
        //}

        //void OnDestroy()
        //{
        //    if (Instance == this)
        //        Instance = null;
        //}

        void Update()
        {
            if (IsPaused) return;

            // Iterate in reverse in case unregistering during iteration
            for (var i = _updatables.Count - 1; i >= 0; i--)
            {
                if (_updatables[i].gameObject.activeSelf)
                    _updatables[i].OnUpdate();
            }
        }

        /// <summary>
        /// Register an IUpdatable to receive per-frame updates.
        /// </summary>
        public void Register(IUpdatable updatable)
        {
            if (updatable != null && !_updatables.Contains(updatable))
                _updatables.Add(updatable);
        }

        /// <summary>
        /// Unregister an IUpdatable to stop receiving per-frame updates.
        /// </summary>
        public void Unregister(IUpdatable updatable)
        {
            if (updatable != null)
                _updatables.Remove(updatable);
        }

        /// <summary>
        /// Convenience methods to pause or resume the update loop.
        /// </summary>
        public void Pause(bool pause) => IsPaused = pause;
    }
}