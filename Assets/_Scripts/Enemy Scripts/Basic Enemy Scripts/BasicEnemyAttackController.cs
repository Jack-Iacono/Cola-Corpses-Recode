using UnityEngine;

public class BasicEnemyAttackController : EnemyAttackSystem
{
    protected void Update()
    {
        // Check if the player is in attack range
        if (Vector3.SqrMagnitude(target.position - transform.position) < attackRange * attackRange)
        {
            if (attackReady)
                AttackPlayer();
        }

        attackTimer.Update(Time.deltaTime);
    }
}
