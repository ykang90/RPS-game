using UnityEngine;

namespace CARVES.Utls
{
    public static class RectResizer
    {
        public static void AdjustRectToAspectRatio(RectTransform targetRect, float width, float height, float maxWidth, float maxHeight)
        {
            // 获取sprite的原始尺寸和比例
            float spriteRatio = width / height;

            // 计算新的尺寸，保持比例不变
            float newWidth, newHeight;
            var isBaseOnWith = width > height;
            if (isBaseOnWith)
            {
                newWidth = maxWidth;
                newHeight = newWidth / spriteRatio;
            }
            else
            {
                newHeight = maxHeight;
                newWidth = newHeight * spriteRatio;
            }
            // 调整RectTransform的大小
            targetRect.sizeDelta = new Vector2(newWidth, newHeight);
        }

    }
}