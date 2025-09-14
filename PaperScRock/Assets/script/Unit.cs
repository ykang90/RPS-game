using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Unit:MonoBehaviour
{
    public RpsType unitType;
    public TeamType team;
    public float speed = 100f;
    public float hp = 1f;

    private void Update()
    {
        Vector2 dir = (team == TeamType.Player) ? Vector2.up : Vector2.down;
        transform.Translate(dir * speed * Time.deltaTime);
    }
    public void TakeDamage(float amount)
    {
        hp -= amount;
        if (hp <= 0f)
        {
            hp = 0f;
            Destroy(gameObject);
            Debug.Log($"{team} tower damage.HP = {hp}");  
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Unit enemy = other.GetComponent<Unit>();
        if (enemy != null && enemy.team != this.team)
        {
            StartCoroutine(BattleLoop(enemy));
        }
        Tower tower = other.GetComponent<Tower>();
        if(tower != null && tower.team != this.team)
        {
            tower.TakeDamage(1);
            Destroy(gameObject);
        }
    }

    private IEnumerator BattleLoop(Unit enemy)
    {
        if (enemy == null) yield break;
        if (hp <= 0 || enemy.hp <= 0) yield break;
       
        while (this != null && hp > 0 && enemy != null && enemy.hp > 0)
        {
            int result = BattleSystem.Compare(this.unitType, enemy.unitType);
            if (result > 0)
            {
                this.TakeDamage(0.5f);
                enemy.TakeDamage(1f);
            }
            else if (result < 0)
            {
                this.TakeDamage(1f);
                enemy.TakeDamage(0.5f);
            }
            else
            {
                this.TakeDamage(1f);
                enemy.TakeDamage(1f);
            }
            yield return new WaitForSeconds(0.2f);
        }
    }
    void OnEnable()
    {
        if (UnitRegistry.Instance != null) UnitRegistry.Instance.Register(this);
    }

    void OnDisable()
    {
        if (UnitRegistry.Instance != null) UnitRegistry.Instance.Unregister(this);
    }
}
