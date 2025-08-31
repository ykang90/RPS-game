#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace CARVES.Utls
{
    /// <summary>仅用于让字段在 Inspector 里只读显示</summary>
    public class XReadOnlyAttribute : PropertyAttribute
    {
    }
    /// <summary>让标记了 ReadOnly 的字段在 Inspector 中显示为灰色不可改</summary>
    [CustomPropertyDrawer(typeof(XReadOnlyAttribute))]
    public class XReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.DisabledScope(true))
                EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUI.GetPropertyHeight(property, label, true);
    }
}
#endif