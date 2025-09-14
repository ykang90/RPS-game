using UnityEngine;

public class Tower: MonoBehaviour
{
    public TeamType team;
    public int hp = 5;
    public int currentHealth;

    private void Awake()
    {
        Debug.Log($"{gameObject.name} Awake with hp={hp}");
        currentHealth = hp;
    
    }
    public void TakeDamage(int damage)
    {
        hp -= damage;
        Debug.Log($"{gameObject.name}HP:{hp}");

        if (hp <= 0 )
        {
            GameManager.Instance.GameOver(this);
            gameObject.SetActive(false);
        }
    }

    public void ResetHealth()
    
    {
        hp = 5;

    }


}
