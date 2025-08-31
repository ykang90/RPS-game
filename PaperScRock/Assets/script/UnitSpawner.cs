using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static LevelData;

public class UnitSpawner: MonoBehaviour
{
    public TeamType team;
    public Transform spawnPoint;
    public SpawnSettings settings;
    public event Action<UnitSpawner> OnSpawnComplete;
    
    private Coroutine spawnCoroutine;
    private bool isSpawning = false;
    public bool IsSpawning => isSpawning;

     public void StartSpawning()
    {
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    public void StopSpawning()
    {
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        spawnCoroutine = null;
        isSpawning = false;
    }

    public void Configure(SpawnSettings spawnSettings)
    {
        if (spawnSettings == null || spawnSettings.events == null || spawnSettings.events.Length == 0)
        {
            Debug.LogWarning("Spawner: no spawn events configured.");
                return;
        }
        settings = spawnSettings;
        StartSpawning();
    }
    private IEnumerator SpawnRoutine()
    {
        if (settings == null || settings.events == null || settings.events.Length == 0)
        {
            Debug.LogWarning("UnitSpawner: No Prefab");
            yield break;
        }

        isSpawning = true;

        foreach (var e in settings.events)
        {
            yield return new WaitForSeconds(e.delay);

            if (e.prefab == null) continue;

            for (int i = 0; i < e.count; i++)
            {
                GameObject go = Instantiate(e.prefab, spawnPoint.position, Quaternion.identity);

                Unit unit = go.GetComponent<Unit>();
                if (unit != null)
                {
                    unit.team = team;
                }

                go.tag = (team == TeamType.Player) ? "PlayerUnit" : "Enemy";
            }
        }

        isSpawning = false;
        spawnCoroutine = null;
        OnSpawnComplete?.Invoke(this);
    }

}

