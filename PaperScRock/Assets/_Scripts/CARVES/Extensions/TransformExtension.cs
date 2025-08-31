using UnityEngine;

namespace CARVES.Utls
{
    public static class TransformExtension
    {
        public static Transform? Find(this Transform transform, string name)
        {
            var recursiveCheck = 9999;
            foreach (Transform tran in transform)
            {
                if (recursiveCheck <= 0) throw new System.Exception("递归查找>9999!");
                recursiveCheck--;
                var t = RecursiveFindTransform(tran, name, ref recursiveCheck);
                if (t) return t;
            }

            return null;

            static Transform RecursiveFindTransform(Transform t, string tranName, ref int recursiveCheck)
            {
                foreach (Transform tran in t)
                {
                    recursiveCheck--;
                    var tan = tran.Find(tranName);
                    if (tan) return tan;
                }

                return null;
            }
        }
        public static void PlaceOn(this Transform transform, Transform target)
        {
            transform.SetParent(target);
            transform.localPosition = Vector3.zero;
            transform.rotation = new Quaternion(0,0,0,0);
        }
    }
}