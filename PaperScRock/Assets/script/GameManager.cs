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

    public void GameOver(Tower defeatedTower)
    {
        if (defeatedTower == PlayerTowerHealth)
        {
            Debug.Log("You lose!");
            feedbacktext.text = "You Lose!!! Loser !!";
        }
        else
        {
            Debug.Log("You Win!");            
            feedbacktext.text = "You will never Beat me!!!! NEXT!";
        }
        
            Time.timeScale = 0f;
        gamePanel.SetActive(false);
        gameOverPanel.SetActive(true);
        retryButton.interactable = true;
        nextLevelButton.interactable = true;
        quitButton.interactable = true;

    }

    public void Retry()
    {
        Time.timeScale = 1f;
        

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        

    }

    public void nextLevel()
    {
        GameObject[] oldEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach(GameObject enemy in oldEnemies)
        {
            Destroy(enemy);
        }

        GameObject[] oldPlayerUnits = GameObject.FindGameObjectsWithTag("PlayerUnit");
        foreach(GameObject unit in oldPlayerUnits)
        {
            Destroy(unit);
        }

        Time.timeScale = 1f;
        gamePanel.SetActive(true);
        gameOverPanel.SetActive(false);
        EnemyTower.SetActive(true);
        PlayerTower.SetActive(true);
        EnemyTowerHealth = EnemyTower.GetComponent<Tower>();
        PlayerTowerHealth = PlayerTower.GetComponent<Tower>();
        EnemyTowerHealth.hp = 5;
        PlayerTowerHealth.hp = 5;
        
        foreach (var btn in unitButton)
        {
            btn.ResetCooldown();
        }

        if (spawner != null)
        {
            spawner.IncreaseSpawnSpeed(speedUpAmount);
            spawner.StartSpawning();
        }
        
        
    }
public void QuitGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

}
