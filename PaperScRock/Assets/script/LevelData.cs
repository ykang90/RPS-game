using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/LevelData")]
public class LevelData : ScriptableObject
{
    public string levelName = "New Level";

    public SpawnSettings enemySpawnSettings;  

    public int enemyTowerHP = 5;
    public int playerTowerHP = 5;

}