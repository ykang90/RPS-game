using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
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

    public float speedUpAmount = 0.05f;

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

        if (defeatedTower == PlayerTowerHealth)
        {
            Debug.Log("You lose!");
            Time.timeScale = 0f;
            gamePanel.SetActive(false);
            gameOverPanel.SetActive(true);
            retryButton.interactable = true;
            nextLevelButton.interactable = false;
            quitButton.interactable = true;
        }
        else if (defeatedTower == EnemyTowerHealth)
        {

            Debug.Log("You Win!");
            Time.timeScale = 0f;
            gamePanel.SetActive(false);
            gameOverPanel.SetActive(true);
            retryButton.interactable = true;
            nextLevelButton.interactable = true;
            quitButton.interactable = true;
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
        gamePanel.SetActive(true);
        gameOverPanel.SetActive(false);
        EnemyTower.SetActive(true);
        PlayerTower.SetActive(true);

        foreach (var btn in unitButton) { btn.ResetCooldown(); }

       levelManager.LoadNextLevel();
       
    }
    public void QuitGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

}
