using UnityEngine;

public static class BattleSystem
{
    public static int Compare(RpsType a, RpsType b)
    {
        if (a == b) return 0;
        if ((a == RpsType.Rock && b == RpsType.Scissors) ||
            (a == RpsType.Scissors && b == RpsType.Paper) ||
            (a == RpsType.Paper && b == RpsType.Rock))
        {
            return 1;
        }
        return -1;
    }
}
