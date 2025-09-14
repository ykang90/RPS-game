using UnityEngine;


public class MainMenu:MonoBehaviour
{
    public GameObject LevelSelect;
    public GameObject menuselect;
    public void Game()
    {
        LevelSelect.SetActive(true);
        menuselect.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
