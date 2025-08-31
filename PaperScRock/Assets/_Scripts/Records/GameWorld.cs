using System;
using CARVES.Abstracts;
using CARVES.Core;

[CarvesRecord]
public class GameWorld : CarvesRecordBase
{
    [Inject]ITxRecord Record { get; set; }
}