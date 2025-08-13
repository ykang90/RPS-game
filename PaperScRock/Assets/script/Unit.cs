using UnityEngine;



public class Unit:MonoBehaviour
{
    public RpsType unitType;
    public TeamType team;
    public float speed = 100f;
    public int hp = 1;
    
    void Update()
    {
        Vector2 dir = (team == TeamType.Player) ? Vector2.up : Vector2.down;
        transform.Translate(dir * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Unit enemy = other.GetComponent<Unit>();
        if(enemy != null && enemy.team != this.team)
        {
            int result = BattleSystem.Compare(this.unitType, enemy.unitType);
            if (result > 0)
            {
                Destroy(enemy.gameObject);   
            }
            else if (result < 0 )
            {
                Destroy(this.gameObject);
            }
            else
            {
                Destroy(enemy.gameObject);
                Destroy(this.gameObject);
            }
        }
        Tower tower = other.GetComponent<Tower>();
        if (tower != null && tower.team != this.team)
        {
       
            tower.TakeDamage(1);
            Destroy(this.gameObject);
        }


    }
}
