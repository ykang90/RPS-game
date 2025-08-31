#if UNITY_EDITOR
using System;
using System.IO;
using CARVES.Core;
using UnityEditor;
using UnityEngine;

namespace CARVES.Editor
{
    public static class CarvesScriptCreator
    {
        // --------- 配置读取 ---------
        static CarvesSettings Config
        {
            get
            {
                var settingName = $"t:{nameof(CarvesSettings)}";
                var guids = AssetDatabase.FindAssets(settingName);
                if (guids.Length == 0)
                    throw new InvalidOperationException("未找到 CarvesSettings.asset，请先创建并配置路径。");
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<CarvesSettings>(path);
            }
        }

        // --------- 通用创建方法 ---------
        static void CreateScript(string subFolder, string defaultName, string template)
        {
            // 1) 组合目标文件夹 Assets/RootFolder/SubFolder
            var cfg = Config;
            const string assetsPath = "Assets";
            var fullDir = Path.Combine(assetsPath, cfg.RootPath, subFolder);
            if (!Directory.Exists(fullDir))
                Directory.CreateDirectory(fullDir);

            // 2) 弹出保存对话框
            var savePath = EditorUtility.SaveFilePanelInProject(
                title: "Create CARVES Script",
                defaultName: defaultName + ".cs",
                extension: "cs",
                message: "",
                path: fullDir
            );
            if (string.IsNullOrEmpty(savePath))
                return;

            // 3) 填充模板
            var className = Path.GetFileNameWithoutExtension(savePath);
            var code = template
                .Replace("#NAME#", className)
                .Replace("#NAMESPACE#", PlayerPrefNamespace());

            // 4) 写文件、导入、选中
            File.WriteAllText(savePath, code);
            AssetDatabase.ImportAsset(savePath);
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(savePath);
        }

        // 可选：从 Player Settings 或其他地方动态获取基础命名空间
        static string PlayerPrefNamespace() => Application.productName.Replace(" ", "");

        // --------- 各层模板 ---------
        static string ControllerTemplate => 
            @"using CARVES.Core;
using CARVES.Abstracts;
using UnityEngine;

namespace #NAMESPACE#.Controllers
{
    /// <summary>❖ #NAME#</summary>
    [CarvesController]public class #NAME#: IController
    {
        // TODO: 填写命令逻辑
    }
}";

        static string ActorTemplate => 
            @"using CARVES.Core;
using CARVES.Abstracts;
using UnityEngine;

namespace #NAMESPACE#.Actors
{
    /// <summary>❖ #NAME#</summary>
    public class #NAME#: MonoBehaviour
    {
        // TODO: Actor 行为、事件触发
    }
}";

        static string RecordTemplate => 
            @"using CARVES.Data;
using CARVES.Abstracts;
using UnityEngine;

namespace #NAMESPACE#.Records
{
    /// <summary>❖ #NAME#</summary>
    [CarvesRecord]public class #NAME# : CarvesRecordBase
    {       
        // TODO: 调用 SetXXX/GetXXX
    }
}";

        static string ViewTemplate =>
            @"using CARVES.Views;
using CARVES.Abstracts;
using UnityEngine;

namespace #NAMESPACE#.Views
{
    /// <summary>❖ #NAME#</summary>
    [CarvesView]public class #NAME# : UiBase
    {
        //Todo: 实现UiBase构造函数
    }
}";

        static string ModuleTemplate => 
            @"using CARVES.Core;
using CARVES.Abstracts;
using UnityEngine;

namespace #NAMESPACE#.Execute.Modules
{
    /// <summary>❖ #NAME#</summary>
    [CarvesModule]public class #NAME#
    {
    }
}";

        static string ServiceTemplate => 
            @"using CARVES.Core;
using CARVES.Abstracts;
using UnityEngine;

namespace #NAMESPACE#.Execute.Services
{
    /// <summary>❖ #NAME#</summary>
    [CarvesService]public class #NAME#
    {
        // TODO: 提供纯功能方法
    }
}";

        // --------- 菜单项注册 ---------
        [MenuItem("Assets/CARVES/CARV/Controller", priority = 10)]
        static void CreateCommand() 
            => CreateScript(Config.ControllerPath, "NewController", ControllerTemplate);

        [MenuItem("Assets/CARVES/CARV/Actor", priority = 11)]
        static void CreateActor() 
            => CreateScript(Config.ActorPath, "NewActor", ActorTemplate);

        [MenuItem("Assets/CARVES/CARV/Record", priority = 12)]
        static void CreateRecord() 
            => CreateScript(Config.RecordPath, "NewRecord", RecordTemplate);

        [MenuItem("Assets/CARVES/CARV/View", priority = 13)]
        static void CreateView() 
            => CreateScript(Config.ViewPath, "NewView", ViewTemplate);

        [MenuItem("Assets/CARVES/Execute/Module", priority = 14)]
        static void CreateModule() 
            => CreateScript(Config.ExecuteModulePath, "NewModule", ModuleTemplate);

        [MenuItem("Assets/CARVES/Execute/Service", priority = 15)]
        static void CreateService() 
            => CreateScript(Config.ExecuteServicePath, "NewService", ServiceTemplate);
    }
}
#endif
