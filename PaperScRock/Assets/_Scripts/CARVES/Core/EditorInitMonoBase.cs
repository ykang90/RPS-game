using UnityEngine;

namespace CARVES.Core
{
    /// <summary>
    /// 自动初始化<see cref="InitMonoBase{T}"/>类，主要是测试用。
    /// </summary>
    [RequireComponent(typeof(InitMonoBase))] public class EditorInitMonoBase : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] InitMonoBase monoBase;
        void Start()
        {
            monoBase.Init(null);
        }
#endif
    }
}