using UnityEngine;

public class Spawner : MonoBehaviour
{
   
    public float spawnInterval = 2f; 
    public float minSpawnInterval = 0.5f; 
    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0;
        }
    }

        public void IncreaseSpawnSpeed(float amount)
    {
        spawnInterval -= amount;
        if (spawnInterval < minSpawnInterval)
        {
            spawnInterval = minSpawnInterval;
        }
        Debug.Log("new interval " + spawnInterval);
    }
}
