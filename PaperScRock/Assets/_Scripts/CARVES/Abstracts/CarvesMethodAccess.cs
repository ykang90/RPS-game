using System;

namespace CARVES.Abstracts
{
    public enum CarvesMethodAccess { Info, Api }

    [AttributeUsage(AttributeTargets.Method)]
    public abstract class CarvesMethodAttribute : Attribute
    {
        public abstract CarvesMethodAccess Access { get; }
    }
    //public class CarvesInfoAttribute : CarvesMethodAttribute
    //{
    //    public override CarvesMethodAccess Access => CarvesMethodAccess.Info;
    //}
    public class CarvesApiAttribute : CarvesMethodAttribute
    {
        public override CarvesMethodAccess Access => CarvesMethodAccess.Api;
    }
}