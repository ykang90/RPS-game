using UnityEngine;

namespace CARVES.Utls
{
    public static class Uni
    {
        public static float Range(float min, float max) => UnityEngine.Random.Range(min, max);
        public static float Random(Vector2 vec) => Range(vec.x, vec.y);
        public static int Range(int min, int max, bool includeMax = true) => UnityEngine.Random.Range(min, includeMax ? max + 1 : max);
        public static int Random(Vector2Int vec, bool includeMax = true) => Range(vec.x, vec.y, includeMax);
    }
}