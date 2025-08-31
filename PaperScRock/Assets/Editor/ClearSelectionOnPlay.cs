using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class ClearSelectionOnPlay
{
    private const string MENU_NAME = "Tools/Enable Clear Selection on Play";
    private static bool isEnabled;

    static ClearSelectionOnPlay()
    {
        // 加载初始设置
        isEnabled = EditorPrefs.GetBool(MENU_NAME, true);
        Menu.SetChecked(MENU_NAME, isEnabled);

        // 注册 ClearSelection 方法
        EditorApplication.playModeStateChanged += ClearSelection;
    }

    private static void ClearSelection(PlayModeStateChange state)
    {
        // 检查功能是否启用
        if (!isEnabled)
        {
            return;
        }

        if (state == PlayModeStateChange.ExitingEditMode)
        {
            Selection.activeObject = null;
            Debug.Log("已清除层级视图的选中项以防止错误！");
        }
    }

    [MenuItem(MENU_NAME)]
    private static void ToggleAction()
    {
        isEnabled = !isEnabled;
        Menu.SetChecked(MENU_NAME, isEnabled);
        EditorPrefs.SetBool(MENU_NAME, isEnabled);
    }
}