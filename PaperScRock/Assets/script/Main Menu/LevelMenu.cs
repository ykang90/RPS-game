using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class LevelMenu : MonoBehaviour
{
    public Button[] LevelButton;
    public GameObject levelButtons;

    private void Awake()
    {
        LevelButton = GetComponentsInChildren<Button>(true);

        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        for(int i = 0; i < LevelButton.Length; i++)
        {
            LevelButton[i].interactable = (i < unlockedLevel);
        }
        
    }
    public void OpenLevel( int LevelId)
    {
        string levelName = "Level" + LevelId;
        SceneManager.LoadScene(levelName);
    }

    void ButtonsToArray()
    {
        int childCount = levelButtons.transform.childCount;
        LevelButton = new Button[childCount];
        for (int i = 0; i < childCount; i++)
        {
            LevelButton[i] = levelButtons.transform.GetChild(i).gameObject.GetComponent<Button>();
        }
    }
}
