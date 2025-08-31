using System;
using CARVES.Abstracts;
using Sirenix.OdinInspector;
using UnityEngine;

[CarvesRecord]public class Configure : MonoBehaviour,ICarvesRecord
{
    [LabelText("主配置")]public GameConfig Config;

    public void Dispose() { }
    public void Emit(EmitContext context) => throw new NotImplementedException("Emit method is not implemented in Configure class.");
}

[Serializable]public class GameConfig
{
    
}