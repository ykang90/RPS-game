using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class PrefabSpawnerWindow : EditorWindow
{
    // 要生成的预制体列表
    public List<GameObject> prefabs = new List<GameObject>();

    // 生成区域的碰撞器
    public Collider generationArea;

    // 父对象名称前缀
    public string parentNamePrefix = "GeneratedPrefabs_";

    // 总生成数量
    public int totalPrefabs = 1000;

    // 循环次数
    public int totalLoops = 2;

    // 初始生成范围半径
    private float initialRadius = 0f;

    // 最大生成范围半径
    private float maxRadius = 0f;

    // 生成范围扩大倍数
    private float radiusMultiplier = 2f;

    // 随机数种子（可选）
    public int randomSeed = 0;
    public bool useRandomSeed = false;

    [MenuItem("Tools/Prefab Spawner")]
    public static void ShowWindow()
    {
        GetWindow<PrefabSpawnerWindow>("Prefab Spawner");
    }

    private void OnGUI()
    {
        GUILayout.Label("Prefab Spawner Settings", EditorStyles.boldLabel);

        // 预制体列表
        SerializedObject serializedObject = new SerializedObject(this);
        SerializedProperty prefabsProperty = serializedObject.FindProperty("prefabs");
        EditorGUILayout.PropertyField(prefabsProperty, true);
        serializedObject.ApplyModifiedProperties();

        // 生成区域
        generationArea = EditorGUILayout.ObjectField("Generation Area Collider", generationArea, typeof(Collider), true) as Collider;

        // 父对象名称前缀
        parentNamePrefix = EditorGUILayout.TextField("Parent Name Prefix", parentNamePrefix);

        // 总生成数量
        totalPrefabs = EditorGUILayout.IntField("Total Prefabs", totalPrefabs);

        // 循环次数
        totalLoops = EditorGUILayout.IntField("Total Loops", totalLoops);

        // 生成范围扩大倍数
        radiusMultiplier = EditorGUILayout.FloatField("Radius Multiplier", radiusMultiplier);

        // 随机数种子
        useRandomSeed = EditorGUILayout.Toggle("Use Random Seed", useRandomSeed);
        if (useRandomSeed)
        {
            randomSeed = EditorGUILayout.IntField("Random Seed", randomSeed);
        }

        // 开始生成按钮
        if (GUILayout.Button("Generate Prefabs"))
        {
            GeneratePrefabs();
        }
    }

    private void GeneratePrefabs()
    {
        if (prefabs.Count == 0)
        {
            Debug.LogError("Prefab list is empty!");
            return;
        }

        if (generationArea == null)
        {
            Debug.LogError("Generation area collider is not set!");
            return;
        }

        if (totalLoops <= 0)
        {
            Debug.LogError("Total loops must be greater than zero!");
            return;
        }

        // 设置随机数种子
        if (useRandomSeed)
        {
            Random.InitState(randomSeed);
        }

        // 创建父对象
        GameObject parentObject = new GameObject(parentNamePrefix + System.DateTime.Now.ToString("yyyyMMdd_HHmmss"));
        parentObject.transform.SetParent(generationArea.transform);
        // 计算生成区域的面积
        float areaSize = CalculateAreaSize(generationArea);
        if (areaSize <= 0)
        {
            Debug.LogError("Generation area size is invalid!");
            return;
        }

        // 计算初始生成范围半径
        initialRadius = Mathf.Sqrt(areaSize / Mathf.PI) / Mathf.Pow(radiusMultiplier, totalLoops - 1);

        // 最大生成范围半径不能超过区域范围
        maxRadius = initialRadius * Mathf.Pow(radiusMultiplier, totalLoops - 1);

        // 每次循环生成的预制体数量
        int prefabsPerLoop = Mathf.CeilToInt((float)totalPrefabs / totalLoops);

        int prefabsCreated = 0;

        for (int loop = 0; loop < totalLoops; loop++)
        {
            float currentRadius = initialRadius * Mathf.Pow(radiusMultiplier, loop);

            // 确保当前生成范围不超过最大范围
            if (currentRadius > maxRadius)
            {
                currentRadius = maxRadius;
            }

            // 在生成区域内随机生成预制体
            for (int i = 0; i < prefabsPerLoop; i++)
            {
                if (prefabsCreated >= totalPrefabs)
                    break;

                Vector3 spawnPosition = GetRandomPositionInArea(generationArea);

                // 在当前范围内随机偏移
                Vector2 randomOffset = Random.insideUnitCircle * currentRadius;
                spawnPosition.x += randomOffset.x;
                spawnPosition.z += randomOffset.y;

                // 检查生成位置是否在碰撞器范围内
                if (generationArea.bounds.Contains(spawnPosition))
                {
                    GameObject prefab = prefabs[Random.Range(0, prefabs.Count)];
                    GameObject obj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                    obj.transform.position = spawnPosition;
                    obj.transform.SetParent(parentObject.transform);

                    prefabsCreated++;
                }
            }
        }

        Debug.Log($"Generated {prefabsCreated} prefabs.");
    }

    // 计算碰撞器的面积（仅支持 BoxCollider）
    private float CalculateAreaSize(Collider collider)
    {
        if (collider is BoxCollider boxCollider)
        {
            Vector3 size = Vector3.Scale(boxCollider.size, collider.transform.lossyScale);
            float area = size.x * size.z;
            return area;
        }
        else if (collider is SphereCollider sphereCollider)
        {
            float radius = sphereCollider.radius * Mathf.Max(collider.transform.lossyScale.x, collider.transform.lossyScale.z);
            float area = Mathf.PI * radius * radius;
            return area;
        }
        else
        {
            Debug.LogError("Collider Element not supported for area calculation.");
            return 0;
        }
    }

    // 在碰撞器范围内获取一个随机位置
    private Vector3 GetRandomPositionInArea(Collider collider)
    {
        Bounds bounds = collider.bounds;
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;

        Vector3 randomPosition = new Vector3(
            Random.Range(min.x, max.x),
            bounds.center.y,
            Random.Range(min.z, max.z)
        );

        // 确保位置在碰撞器内
        if (collider.bounds.Contains(randomPosition))
        {
            return randomPosition;
        }
        else
        {
            return GetRandomPositionInArea(collider);
        }
    }
}
