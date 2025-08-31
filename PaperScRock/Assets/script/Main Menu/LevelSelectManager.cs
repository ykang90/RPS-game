using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LevelSelectManager : MonoBehaviour
{
    public GameObject levelButtonPrefab;
    public Transform levelGrid;

    public Button prevButton;
    public Button nextButton;

    public int totalLevels = 30; 
    public int levelsPerPage = 9;

    private int currentPage = 0;
    private int maxPage;

    private List<GameObject> spawnedButtons = new List<GameObject>();

    void Start()
    {
        maxPage = Mathf.CeilToInt((float)totalLevels / levelsPerPage) - 1;

        prevButton.onClick.AddListener(PrevPage);
        nextButton.onClick.AddListener(NextPage);

        ShowPage(0);
    }

    void ShowPage(int pageIndex)
    {
        foreach (var btn in spawnedButtons)
        {
            Destroy(btn);
        }
        spawnedButtons.Clear();

        int startLevel = pageIndex * levelsPerPage + 1;
        int endLevel = Mathf.Min(startLevel + levelsPerPage - 1, totalLevels);

        for (int i = startLevel; i <= endLevel; i++)
        {
            GameObject btn = Instantiate(levelButtonPrefab, levelGrid);
            btn.GetComponentInChildren<Text>().text = i.ToString();

            int levelIndex = i; 
            btn.GetComponent<Button>().onClick.AddListener(() => LoadLevel(levelIndex));

            spawnedButtons.Add(btn);
        }

        prevButton.gameObject.SetActive(pageIndex > 0);
        nextButton.gameObject.SetActive(pageIndex < maxPage);

        currentPage = pageIndex;
    }

    void PrevPage()
    {
        if (currentPage > 0)
        {
            ShowPage(currentPage - 1);
        }
    }

    void NextPage()
    {
        if ((currentPage + 1 )* levelsPerPage < totalLevels)
        {
            ShowPage(currentPage + 1);
        }
    }

    void LoadLevel(int levelIndex)
    {
        Debug.Log("Load Level " + levelIndex);
        // SceneManager.LoadScene("Level" + levelIndex);
    }
}

