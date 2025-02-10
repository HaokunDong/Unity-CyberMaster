using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimationTriggers : MonoBehaviour
{
    private Enemy enemy => GetComponentInParent<Enemy>();

    private void FinishTrigger()
    {
        enemy.AnimationFinishTrigger();
    }

    private void PreProcessTrigger()
    {
        enemy.PreProcessTrigger();
    }

    private void AttackTrigger()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll
            (enemy.attackCheck[enemy.attackCount].position, enemy.attackCheckRadius[enemy.attackCount]);

        foreach (var hit in colliders)
        {
            if (hit.GetComponent<Player>() != null)
            {
                hit.GetComponent<Player>().HitTarget(enemy);
            }
        }
    }

    private void CanBeBouncedAttackTrigger() => enemy.CanBeBouncedAttack();
    private void CanNotBeBouncedAttackTrigger() => enemy.CanNotBeBouncedAttack();
}
