using CARVES.Core;
using UnityEngine;

/// <summary>
/// 游戏机制类， 用于注册服务和初始化模块。
/// </summary>
public class SceneMechanismHub : MonoBehaviour
{
    public SceneHierarchy SceneHierarchy;

    //注册服务
    public void Setup(Carves.CarvesBuilder builder)
    {
        // 注册模块
        builder
            .RegisterInstance(SceneHierarchy);
    }

    //初始化模块
    public void Init()
    {
    }
}