using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyAttackSystem : MonoBehaviour
{
    protected float damage;

    protected float attackSpeed = 2;
    protected Timer attackTimer;
    protected bool attackReady = true;

    protected float attackRange = 2;

    protected Transform target;

    protected virtual void Awake()
    {
        attackTimer = new Timer(null, null, OnAttackReady);
        attackTimer.Start(attackSpeed, 0);

        target = PlayerController.Instance.movementController.transform;
    }

    protected virtual void Update()
    {
        // Check if the player is in attack range
        if (Vector3.SqrMagnitude(target.position - transform.position) < attackRange * attackRange)
        {
            if (attackReady)
                AttackPlayer();
        }

        attackTimer.Update(Time.deltaTime);
    }

    protected void AttackPlayer()
    {
        attackReady = false;
        PlayerController.Instance.statusController.ChangeHealth(HurtPlayerMethod);
        attackTimer.Restart();

        float HurtPlayerMethod(float oldHealth)
        {
            return oldHealth - damage;
        }
    }
    protected void OnAttackReady()
    {
        attackReady = true;
    }
}
