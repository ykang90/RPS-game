using UnityEngine;

namespace CARVES.Utls
{
    public static class GameObjectExtensions
    {
        public const string UnityNull = "null";
        public static void Display(this GameObject gameObject, bool display) => gameObject.SetActive(display);
        public static void Display(this Transform transform, bool display) => transform.gameObject.SetActive(display);
        public static void Display(this Component component, bool display) => component.gameObject.SetActive(display);
        public static bool IsUnityNull(this object obj) => obj == null || obj.ToString() == UnityNull;
        public static void DestroyMe<T>(this T obj) where T : Component
        {
            if (!obj) return;
            obj.gameObject.DestroyMe();
        }

        public static void DestroyMe(this GameObject obj)
        {
            if (Application.isPlaying) Object.Destroy(obj);
            else Object.DestroyImmediate(obj);
        }
        public static bool IsInLayerMask(this GameObject obj, LayerMask mask) => (mask.value & (1 << obj.layer)) != 0;
    }
}