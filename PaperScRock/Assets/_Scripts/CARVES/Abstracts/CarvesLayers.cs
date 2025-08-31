using System;

namespace CARVES.Abstracts
{
    public enum CarvesLayers
    {
        Controller,
        ExModule,
        ExService,
        Record,
        Actor,
        View,
        Shared
    }

    public enum GenerateInterfaceType
    {
        None,
        ReadWrite
    }

    /// <summary>
    /// 基础特性：标记类所属层级，并可指定是否生成接口
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public abstract class CarvesLayerAttribute : Attribute
    {
        /// <summary>所在架构层级</summary>
        public abstract CarvesLayers Layer { get; }

        public virtual bool SelfManage { get; } = false;
        /// <summary>生成接口类型</summary>
        public virtual GenerateInterfaceType GenerateInterface { get; set; } = GenerateInterfaceType.None;
    }

    // 各层级直观子特性，继承自 CarvesLayerAttribute
    public class CarvesControllerAttribute : CarvesLayerAttribute
    {
        public override CarvesLayers Layer => CarvesLayers.Controller;
    }
    public class CarvesModuleAttribute : CarvesLayerAttribute
    {
        public override CarvesLayers Layer => CarvesLayers.ExModule;
        public override GenerateInterfaceType GenerateInterface => GenerateInterfaceType.ReadWrite;
    }
    public class CarvesServiceAttribute : CarvesLayerAttribute
    {
        public override CarvesLayers Layer => CarvesLayers.ExService;
    }
    //public class CarvesActorAttribute : CarvesLayerAttribute
    //{
    //    public override CarvesLayers Layer => CarvesLayers.Actor; 
    //    public override bool ManualInstance { get; } = true;
    //}
    public class CarvesRecordAttribute : CarvesLayerAttribute
    {
        public override CarvesLayers Layer => CarvesLayers.Record;
        public override bool SelfManage { get; } = true;
    }
    public class CarvesViewAttribute : CarvesLayerAttribute
    {
        public override CarvesLayers Layer => CarvesLayers.View;
        public override bool SelfManage { get; } = true;
    }
    // <summary>
    // Shared层可选标记（工具、杂物层）
    // </summary>
    public class CarvesSharedAttribute : CarvesLayerAttribute
    {
        public override CarvesLayers Layer => CarvesLayers.Shared;
    }
}