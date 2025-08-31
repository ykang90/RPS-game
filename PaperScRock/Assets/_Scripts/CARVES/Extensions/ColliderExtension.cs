using UnityEngine;

namespace CARVES.Utls
{
    public static class ColliderExtension
    {
        // 检查某个Transform是否在Collider范围内
        public static bool IsInRange(this SphereCollider sphereCollider, Transform targetTransform)
        {
            // 获取SphereCollider的半径并考虑Scale
            var scaledRadius = sphereCollider.radius * Mathf.Max(sphereCollider.transform.lossyScale.x,
                sphereCollider.transform.lossyScale.y, sphereCollider.transform.lossyScale.z);
            var sphereCenter = sphereCollider.transform.position + sphereCollider.center;

            // 计算与中心点的距离
            var distance = Vector3.Distance(sphereCenter, targetTransform.position);
            return distance <= scaledRadius;
        }

        public static bool IsInRange(this BoxCollider boxCollider, Transform targetTransform)
        {
            // 获取BoxCollider的大小并考虑Scale
            var halfSize = Vector3.Scale(boxCollider.size / 2, boxCollider.transform.lossyScale);
            var boxCenter = boxCollider.transform.position + boxCollider.center;

            // 计算世界坐标中的范围
            var bounds = new Bounds(boxCenter, halfSize * 2);
            return bounds.Contains(targetTransform.position);
        }
        public static bool IsInRange2D(this SphereCollider sphereCollider, Transform targetTransform)
        {
            // 获取 SphereCollider 的半径，并考虑 X 和 Z 轴的缩放
            var scaleXZ = Mathf.Max(
                sphereCollider.transform.lossyScale.x,
                sphereCollider.transform.lossyScale.z);
            var scaledRadius = sphereCollider.radius * scaleXZ;

            // 获取球体的中心位置，忽略 Y 轴
            var sphereCenter = sphereCollider.transform.position + sphereCollider.center;
            var sphereCenter2D = new Vector2(sphereCenter.x, sphereCenter.z);

            // 获取目标位置，忽略 Y 轴
            var targetPosition = targetTransform.position;
            var targetPosition2D = new Vector2(targetPosition.x, targetPosition.z);

            // 计算 2D 平面上的距离
            var distance = Vector2.Distance(sphereCenter2D, targetPosition2D);

            return distance <= scaledRadius;
        }
        public static bool IsInRange2D(this BoxCollider boxCollider, Transform targetTransform)
        {
            // 获取 BoxCollider 的大小，并考虑 X 和 Z 轴的缩放
            var size = boxCollider.size;
            var scale = boxCollider.transform.lossyScale;
            var sizeXZ = new Vector2(size.x * scale.x, size.z * scale.z);

            // 计算半尺寸
            var halfSizeXZ = sizeXZ / 2f;

            // 获取盒子的中心位置，考虑位置和中心偏移，忽略 Y 轴
            var boxCenter = boxCollider.transform.position + boxCollider.center;
            var boxCenter2D = new Vector2(boxCenter.x, boxCenter.z);

            // 获取目标位置，忽略 Y 轴
            var targetPosition = targetTransform.position;
            var targetPosition2D = new Vector2(targetPosition.x, targetPosition.z);

            // 如果 BoxCollider 没有旋转，可以直接判断
            if (boxCollider.transform.rotation == Quaternion.identity)
            {
                // 计算最小和最大范围
                var min = boxCenter2D - halfSizeXZ;
                var max = boxCenter2D + halfSizeXZ;

                // 判断目标位置是否在范围内
                return (targetPosition2D.x >= min.x && targetPosition2D.x <= max.x) &&
                       (targetPosition2D.y >= min.y && targetPosition2D.y <= max.y);
            }

            // 如果 BoxCollider 有旋转，需要将点转换到碰撞器的本地坐标系
            var localPoint = boxCollider.transform.InverseTransformPoint(targetPosition);
            var localCenter = boxCollider.center;
            var halfSize = boxCollider.size / 2f;

            // 在本地坐标系中忽略 Y 轴
            var localPoint2D = new Vector2(localPoint.x, localPoint.z);
            var localCenter2D = new Vector2(localCenter.x, localCenter.z);
            var halfSize2D = new Vector2(halfSize.x, halfSize.z);

            // 判断点是否在本地 2D 范围内
            return Mathf.Abs(localPoint2D.x - localCenter2D.x) <= halfSize2D.x &&
                   Mathf.Abs(localPoint2D.y - localCenter2D.y) <= halfSize2D.y;
        }

    }
}