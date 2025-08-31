using System.IO;
using UnityEditor;
using UnityEngine;

public class CombineScriptsWindow : EditorWindow
{
    // 要搜索的文件夹路径
    private string folderPath = "";
    // 输出文件名（会放在选定的文件夹下）
    private string outputFileName = "combined_code.txt";
    // 是否包含 DLL 文件
    private bool includeDlls = false;
    // 默认搜索的文件扩展名（用于代码文件）
    private string searchPattern = "*.cs";

    [MenuItem("Tools/Scripts/Export Combined Scripts")]
    public static void ShowWindow()
    {
        GetWindow<CombineScriptsWindow>("Combine Scripts");
    }

    private void OnGUI()
    {
        GUILayout.Label("合并脚本设置", EditorStyles.boldLabel);

        // 文件夹选择
        GUILayout.BeginHorizontal();
        GUILayout.Label("文件夹路径", GUILayout.Width(80));
        folderPath = GUILayout.TextField(folderPath, GUILayout.Width(250));
        if (GUILayout.Button("浏览", GUILayout.Width(60)))
        {
            string selected = EditorUtility.OpenFolderPanel("选择要合并的文件夹", Application.dataPath, "");
            if (!string.IsNullOrEmpty(selected))
            {
                folderPath = selected;
            }
        }
        GUILayout.EndHorizontal();

        // 输出文件名输入
        GUILayout.BeginHorizontal();
        GUILayout.Label("输出文件名", GUILayout.Width(80));
        outputFileName = GUILayout.TextField(outputFileName, GUILayout.Width(250));
        GUILayout.EndHorizontal();

        // 是否包含 DLL
        includeDlls = EditorGUILayout.Toggle("包含 DLL 文件", includeDlls);

        // 如果不包含 DLL，可设置搜索模式（默认 *.cs）
        if (!includeDlls)
        {
            searchPattern = EditorGUILayout.TextField("搜索模式", searchPattern);
        }
        else
        {
            GUILayout.Label("搜索模式：代码文件 (" + searchPattern + ") 以及 DLL 文件 (*.dll)");
        }

        GUILayout.Space(10);
        if (GUILayout.Button("开始合并"))
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                Debug.LogError("请先指定一个文件夹路径！");
                return;
            }
            CombineFiles();
        }
    }

    private void CombineFiles()
    {
        // 输出文件路径放在所选文件夹下
        string outputPath = Path.Combine(folderPath, outputFileName);

        // 搜索代码文件（默认*.cs，如果你想修改可在GUI中修改搜索模式）
        string[] filesCs = Directory.GetFiles(folderPath, searchPattern, SearchOption.AllDirectories);
        // 搜索 DLL 文件
        string[] filesDll = includeDlls ? Directory.GetFiles(folderPath, "*.dll", SearchOption.AllDirectories) : new string[0];

        using (StreamWriter writer = new StreamWriter(outputPath))
        {
            // 合并代码文件
            foreach (string file in filesCs)
            {
                writer.WriteLine($"// Start of: {Path.GetFileName(file)}");
                writer.WriteLine(File.ReadAllText(file));
                writer.WriteLine($"// End of: {Path.GetFileName(file)}");
                writer.WriteLine();
            }

            // 合并 DLL 文件（这里只输出引用提示，避免写入二进制内容）
            foreach (string file in filesDll)
            {
                writer.WriteLine($"// Start of DLL: {Path.GetFileName(file)}");
                writer.WriteLine("// [此处为 DLL 文件，二进制内容不适合直接合并]");
                writer.WriteLine($"// End of DLL: {Path.GetFileName(file)}");
                writer.WriteLine();
            }
        }

        Debug.Log("文件已合并到: " + outputPath);
    }
}