using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitButton: MonoBehaviour
{
    public Button button;
    public Image cooldownOverlay;
    public float cooldownTime = 1f;

    private bool isCoolingDown = false;

    public GameObject unitPrefab;
    public Transform spawnPoint;

    private static List<UnitButton> allButtons = new List<UnitButton>();


    void Awake()
    {
        allButtons.Add(this);
    }
    private void OnDestroy()
    {
        allButtons.Remove(this);
    }

    void Start()
    {
        button.onClick.AddListener(SpawnUnit);
        cooldownOverlay.fillAmount = 1;

    }
    void SpawnUnit()
    {
        if (isCoolingDown) return;

        Instantiate(unitPrefab, spawnPoint.position, Quaternion.identity);
        
        foreach(var btn in allButtons)
        {
            if(!btn.isCoolingDown)
            {
                btn.StartCoroutine(btn.CooldownRoutine());
            }
        }
        
    }

    IEnumerator CooldownRoutine()
    {
        isCoolingDown=true;
        button.interactable = false;
        float timeLeft = cooldownTime;

        while(timeLeft >0f)
        {
            timeLeft -= Time.deltaTime;
            cooldownOverlay.fillAmount = timeLeft / cooldownTime;
            yield return null;
        }
        cooldownOverlay.fillAmount = 1;
        button.interactable = true;
        isCoolingDown = false;
    }

        public void ResetCooldown()
    {
        cooldownOverlay.fillAmount = 1;
        isCoolingDown = false;
        button.interactable = true;
        
    }
}
