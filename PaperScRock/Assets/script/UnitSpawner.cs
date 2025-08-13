using UnityEngine;

public class UnitSpawner: MonoBehaviour
{
    public GameObject[] unitPrefabs;
    public float spawnInterval = 3f;
    public bool isAutoSpawn = true;
    private float cooldown = 0f;

    public float minSpawnInterval = 0.02f;

     void Start()
    {
        if (isAutoSpawn)
        {
            InvokeRepeating("AutoSpawn", 1f, spawnInterval);
        }
    }

    void Update()
    {
        if (!isAutoSpawn && cooldown > 0)
        {
            cooldown -= Time.deltaTime;
        }
    }

    void AutoSpawn()
    {
        SpawnUnit(Random.Range(0, unitPrefabs.Length));
    }

    public void SpawnUnit(int index)
    {
        if (!isAutoSpawn && cooldown > 0) return;
        Instantiate(unitPrefabs[index],transform.position,Quaternion.identity);
        if (!isAutoSpawn) cooldown = 1f;
    }
    
    public void IncreaseSpawnSpeed(float amount)
    {
        spawnInterval -= amount;
        if (spawnInterval < minSpawnInterval ) spawnInterval = minSpawnInterval;

        if(isAutoSpawn)
        {
            CancelInvoke("AutoSpawn");
            InvokeRepeating("AutoSpawn", spawnInterval, spawnInterval);

        }
        Debug.Log("new spawntime:" + spawnInterval);
    }

    public void StartSpawning()
    {
        CancelInvoke("AutoSpawn");
        if(isAutoSpawn)
        {
            InvokeRepeating("AutoSpawn", 1f, spawnInterval);
        }
    }
}
