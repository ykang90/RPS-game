using UnityEditorInternal;
using UnityEngine;

namespace CARVES.Editor
{
    [CreateAssetMenu(fileName = "CarvesSettings", menuName = "CARVES/Settings")]
    public class CarvesSettings : ScriptableObject
    {
        public static readonly string DefaultDir = $"Assets/_Scripts/{CarvesDir}";
        public const string Ns_GeneratedCarves = "Generated.Carves";
        public const string GeneratedAssemblyName = "CARVES.Generate";
        public const string CarvesDir = "CARVES";

        [Header("路径配置")] 
        public string RootPath = "_Scripts";
        public string GeneratedInterfaceDir = "CARVES.Generated"; // 相对工程资产根
        public string LinkXmlPath = "Carves_AutoLink.xml"; // 相对工程根
        public string MetaInfoName = "CarvesMetaInfo.asset"; // 文件名，不含目录
        public string ControllerPath = "Controllers";
        public string ActorPath = "Actors";
        public string RecordPath = "Records";
        public string ViewPath = "Views";
        public string ExecuteModulePath = "Execute/Modules";
        public string ExecuteServicePath = "Execute/Services";
        public string SharedPath = "Shared";

        public AssemblyDefinitionAsset[] ScanAssemblies;
        [Header("接口/编译生成配置")] [Tooltip("启用后 Unity 在脚本保存时立即触发编译 & Domain Reload；关闭则须手动重新编译 (⇧⌘R / Ctrl+R)")]
        public bool AutoGenerate = true;

        public bool GenerateInterfaceFiles => true;   // ← 测试用，是否生成接口文件
    }
}