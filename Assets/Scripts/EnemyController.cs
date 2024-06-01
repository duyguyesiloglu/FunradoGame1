using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private List<Enemy> enemies = new List<Enemy>();

    private void Start()
    {
        
        enemies.AddRange(FindObjectsOfType<Enemy>());
    }

    public void EnemyDefeated(Enemy enemy)
    {
        
        if (enemies.Contains(enemy))
        {
            enemies.Remove(enemy);
            Debug.Log("Enemy defeated: " + enemy.name);
        }
    }

    public bool AreAllEnemiesDead()
    {
        
        return enemies.Count == 0;
    }
}
