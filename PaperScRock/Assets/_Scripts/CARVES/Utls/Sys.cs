using System;
using UnityEngine;
using Random = System.Random;

namespace CARVES.Utls
{
    public static class Sys
    {
        public static Random Random { get; } = new Random(DateTime.Now.Millisecond);
        public static bool RandomBool() => Random.NextDouble() >= 0.5;
    }
    public static class Vector2IntExtension
    {
        public static int RandomXYRange(this Vector2Int vector) => Sys.Random.Next(vector.x, vector.y + 1);
    }
}