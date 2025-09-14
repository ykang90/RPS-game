using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static int currentLevelIndex = 0;
    public static GameManager Instance;

    public GameObject EnemyTower;
    public GameObject PlayerTower;
    public GameObject gameOverPanel;
    public GameObject gamePanel;
    public UnitSpawner spawner;
    public Tower EnemyTowerHealth;
    public Tower PlayerTowerHealth;
    public Button retryButton;
    public Button nextLevelButton;
    public Button quitButton;
    public UnitButton[] unitButton;
    public TMP_Text feedbacktext;
    public UnitSpawner[] trackedSpawners;

    public LevelManager levelManager;
    private bool gameEnded = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (levelManager != null)
        {
            levelManager.LoadLevel(currentLevelIndex);
        }

        if (UnitRegistry.Instance != null)
        {
            UnitRegistry.Instance.OnUnitRegistered += OnUnitRegistered;
            UnitRegistry.Instance.OnUnitUnregistered += OnUnitUnregistered;
        }

        foreach (var s in trackedSpawners)
        {
            if (s != null) s.OnSpawnComplete += OnSpawnerComplete;
        }
    }

    void OnDestroy()
    {
        if (UnitRegistry.Instance != null)
        {
            UnitRegistry.Instance.OnUnitRegistered -= OnUnitRegistered;
            UnitRegistry.Instance.OnUnitUnregistered -= OnUnitUnregistered;
        }
        foreach (var s in trackedSpawners)
        {
            if (s != null) s.OnSpawnComplete -= OnSpawnerComplete;
        }
    }

    private void OnUnitRegistered(Unit u) { }

    private void OnUnitUnregistered(Unit u)
    {
        CheckEndConditions();
    }

    private void OnSpawnerComplete(UnitSpawner s)
    {
        CheckEndConditions();
    }

    public void CheckEndConditions()
    {
        if (gameEnded) return;

        if (PlayerTowerHealth != null && PlayerTowerHealth.hp <= 0f)
        {
            GameOver(PlayerTowerHealth);
            return;
        }

        if (EnemyTowerHealth != null && EnemyTowerHealth.hp <= 0f)
        {
            GameOver(EnemyTowerHealth);
            return;
        }

        bool anyEnemySpawnerStillSpawning = false;
        if (trackedSpawners != null)
        {
            foreach (var s in trackedSpawners)
            {
                if (s == null) continue;
                if (s.team == TeamType.Enemy && s.IsSpawning)
                {
                    anyEnemySpawnerStillSpawning = true;
                    break;
                }
            }
        }

        int enemyCount = 0;
        if (UnitRegistry.Instance != null) enemyCount = UnitRegistry.Instance.CountByTeam(TeamType.Enemy);

        if (!anyEnemySpawnerStillSpawning && enemyCount == 0)
        {
            GameOver(EnemyTowerHealth);
        }
    }
    public void GameOver(Tower defeatedTower)
    {
        if (gameEnded) return;
        gameEnded = true;

        Time.timeScale = 0f;
        gamePanel.SetActive(false);
        gameOverPanel.SetActive(true);
        retryButton.interactable = true;
        quitButton.interactable = true;

        if (defeatedTower == PlayerTowerHealth)
        {
            Debug.Log("You lose!");
            nextLevelButton.interactable = false;
        }
        else if (defeatedTower == EnemyTowerHealth)
        {

            Debug.Log("You Win!");
            nextLevelButton.interactable = true;
            UnlockNewLevel();
        }

    }

    public void OnLevelPassed(int passedLevel)
    {
        int current = PlayerPrefs.GetInt("UnlockedLevel", 1);

        PlayerPrefs.SetInt("UnlockedLevel", Mathf.Max(current, passedLevel + 1));
        PlayerPrefs.Save();

        Debug.Log("unlocked Level " + (passedLevel + 1));
    }
    void UnlockNewLevel()
    {
        if (SceneManager.GetActiveScene().buildIndex >= PlayerPrefs.GetInt("ReachedIndex"))
        {
            PlayerPrefs.SetInt("ReachedIndex", SceneManager.GetActiveScene().buildIndex + 1);
            PlayerPrefs.SetInt("UnlockedLevel", PlayerPrefs.GetInt("UnlockedLevel", 1) + 1);
            PlayerPrefs.Save();
        }
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        

    }

    public void nextLevel()
    {
        Time.timeScale = 1f;
        currentLevelIndex++;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
       
    }
    public void QuitGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

}
