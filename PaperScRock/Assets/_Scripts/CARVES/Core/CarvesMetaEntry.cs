using CARVES.Abstracts;
using System;

// META: Record per‑class metadata for incremental interface generation
[Serializable] public class CarvesMetaEntry
{
    public string TypeName; // 类名
    public string AssemblyQualifiedName; // 命名空间信息
    public string InterfacePath; // 生成文件的相对路径
    public CarvesLayers Layer; // 所属层级
    public GenerateInterfaceType InterfaceType; // 接口生成类型
    public string RelativePath; // 相对路径（相对于 Assets/_Scripts/CARVES/Generated/Interfaces）
    public long LastModified; // 最后修改时间戳
    public string[] InterfaceNames; // 生成的接口名称列表
    public bool SelfManage; // 是否自我管理实例
}