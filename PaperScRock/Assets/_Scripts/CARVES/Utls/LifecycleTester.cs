using UnityEngine;

namespace CARVES.Utls
{
    /// <summary>
    /// 用于测试 Unity 生命周期方法的脚本。该脚本会在各种生命周期方法被调用时输出日志。
    /// 还提供一个布尔变量来控制 Update 方法是否输出日志，以防止控制台被过多信息污染。
    /// </summary>
    public class LifecycleTester : MonoBehaviour
    {
        /// <summary>
        /// 控制是否禁用 Update 方法的日志输出。
        /// 如果设置为 true，Update 中的日志将不会输出。
        /// </summary>
        [Tooltip("禁用 Update 日志输出，以避免控制台被污染。")]
        public bool disableUpdateLogs = false;

        /// <summary>
        /// Awake 是生命周期中的第一个调用，在所有脚本实例初始化之前调用。
        /// </summary>
        private void Awake()
        {
            Debug.Log($"{gameObject.name}: Awake 被调用");
        }

        /// <summary>
        /// OnEnable 在脚本或其所属的 GameObject 被启用时调用。
        /// </summary>
        private void OnEnable()
        {
            Debug.Log($"{gameObject.name}: OnEnable 被调用");
        }

        /// <summary>
        /// Start 在第一次 Update 调用之前调用一次。
        /// </summary>
        private void Start()
        {
            Debug.Log($"{gameObject.name}: Start 被调用");
        }

        /// <summary>
        /// Update 每帧调用一次。如果 disableUpdateLogs 为 false，则输出日志。
        /// </summary>
        private void Update()
        {
            if (!disableUpdateLogs)
            {
                Debug.Log($"{gameObject.name}: Update 被调用");
            }
        }

        /// <summary>
        /// OnDisable 在脚本或其所属的 GameObject 被禁用时调用。
        /// </summary>
        private void OnDisable()
        {
            Debug.Log($"{gameObject.name}: OnDisable 被调用");
        }

        /// <summary>
        /// OnDestroy 在脚本实例被销毁时调用。
        /// </summary>
        private void OnDestroy()
        {
            Debug.Log($"{gameObject.name}: OnDestroy 被调用");
        }
    }
}