using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu:MonoBehaviour
{
    public void Game()
    {
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
