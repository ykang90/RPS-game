using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public LevelDatabase database;    
    public UnitSpawner enemySpawner;  
    public UnitSpawner playerSpawner; 
    public Tower enemyTower;          
    public Tower playerTower;         

    public int startLevelIndex = 0;
    private int currentIndex = 0;

    void Start()
    {
        if (database == null || database.levels == null || database.levels.Length == 0)
        {
            Debug.LogError("LevelManager: no levels configured in database.");
            return;
        }
        int selected = PlayerPrefs.GetInt("SelectedLevel",1);
        currentIndex = Mathf.Clamp(selected - 1, 0, database.levels.Length - 1);
        LoadLevel(currentIndex);
    }

    public void LoadLevel(int index)
    {
        if (index < 0 || index >= database.levels.Length)
        {
            Debug.LogError("LevelManager: index out of range: " + index);
            return;
        }

        LevelData level = database.levels[index];

       
        if (enemyTower != null) { enemyTower.hp = level.enemyTowerHP; enemyTower.gameObject.SetActive(true); }
        if (playerTower != null) { playerTower.hp = level.playerTowerHP; playerTower.gameObject.SetActive(true); }

        ClearUnits();

        if (enemySpawner != null) enemySpawner.Configure(level.enemySpawnSettings);

        Debug.Log("Loaded level: " + level.levelName);
    }

    public void LoadNextLevel()
    {
        currentIndex++;
        if (currentIndex >= database.levels.Length)
        {
            Debug.Log("All levels finished.");
            return;
        }
        LoadLevel(currentIndex);
    }

    public void ClearUnits()
    {
        foreach (var go in GameObject.FindGameObjectsWithTag("Enemy")) Destroy(go);
        foreach (var go in GameObject.FindGameObjectsWithTag("PlayerUnit")) Destroy(go);
    }
}
