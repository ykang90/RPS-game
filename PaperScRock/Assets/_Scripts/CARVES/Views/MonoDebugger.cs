using UnityEngine;

namespace CARVES.Views
{
    /// <summary>
    /// 挂在物件调试, 用于测试MonoBehaviour的生命周期
    /// </summary>
    public class MonoDebugger : MonoBehaviour
    {
        void Awake()
        {
            Debug.Log("Awake");
        }

        void Start()
        {
            Debug.Log("Stage_Start");
        }

        void OnDisable()
        {
            Debug.Log("OnDisable");
        }

        void OnEnable()
        {
            Debug.Log("OnEnable");
        }

    }
}