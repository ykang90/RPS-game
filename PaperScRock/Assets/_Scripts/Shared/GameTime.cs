using System;
using UnityEngine;

[Flags] public enum GamePeriodMask
{
    [InspectorName("晨")]Morning = 1, 
    [InspectorName("午")]Noon=2, 
    [InspectorName("暮")]Evening=4, 
    [InspectorName("夜")]Midnight=8, 
    [InspectorName("全")]All=15
}
public enum GamePeriod
{
    [InspectorName("晨")]Morning,
    [InspectorName("午")]Noon,
    [InspectorName("暮")]Evening,
    [InspectorName("夜")]Night
}
public static class GamePeriodExtension
{
    static int count = -1;

    static int _timePeriodCount = -1;
    public static int PeriodCount
    {
        get
        {
            if (_timePeriodCount < 0)
                _timePeriodCount = Enum.GetValues(typeof(GamePeriod)).Length;
            return _timePeriodCount;
        }
    }
    public static string ToText(this GamePeriod period)
    {
        return period switch
        {
            GamePeriod.Morning => "晨",
            GamePeriod.Noon => "午",
            GamePeriod.Evening => "暮",
            GamePeriod.Night => "夜",
            _ => "未知"
        };
    }
    public static bool Matches(this GamePeriod period, GamePeriodMask mask)
    {
        if (mask == GamePeriodMask.All) return true;
        return (mask & (GamePeriodMask)(1 << (int)period)) != 0;
    }
}