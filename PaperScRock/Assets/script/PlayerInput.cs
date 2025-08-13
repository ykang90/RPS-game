using UnityEngine;

public class PlayerInput: MonoBehaviour
{
    public UnitSpawner playerSpawner;

    public void SpawnRock()
    {
        playerSpawner.SpawnUnit(1);
    }

    public void SpawnPaper()
    {
        playerSpawner.SpawnUnit(0);
    }

    public void SpawnSciccers()
    {
        playerSpawner.SpawnUnit(2);
    }
}
