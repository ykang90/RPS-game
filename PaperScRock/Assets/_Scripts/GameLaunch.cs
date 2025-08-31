using CardLegend.Views;
using CARVES.Core;
using CARVES.Utls;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class GameLaunch : MonoBehaviour
{
    [LabelText("主配置")]public Configure Config;
    public CarvesMetaInfo MetaInfo;
    [FormerlySerializedAs("sceneHierarchy")] public SceneMechanismHub sceneMechanismHub;
    public UiManager UiManager;
    void Awake()
    {
        Carves.Setup(gameObject.AddComponent<FrameEventBus>()
            , MetaInfo
            , b => sceneMechanismHub.Setup(b));
        Game.Run(OnGameStart);
    }

    void OnGameStart()
    {
        "游戏启动！".Log(this);
        sceneMechanismHub.Init();
        UiManager.Init();
        Carves.Inject(this);
    }
}