using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationTriggers : MonoBehaviour
{
    private Player player => GetComponentInParent<Player>();

    private void AnimationTrigger()
    {
        player.AnimationTrigger();
    }

    private void AttackTrigger()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll
            (player.attackCheck[player.attackCount].position, player.attackCheckRadius[player.attackCount]);

        foreach (var hit in colliders)
        {
            if (hit.GetComponent<Enemy>() != null)
            {
                hit.GetComponent<Enemy>().OnHitFromTarget(player);
                player.HitTarget();
            }
        }
    }

    private void ChargeAttackTrigger()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(player.chargeAttackCheck.position, player.chargeAttackRadius);

        foreach (var hit in colliders)
        {
            if (hit.GetComponent<Enemy>() != null)
            {
                hit.GetComponent<Enemy>().OnHitFromTarget(player);
                player.HitTarget();
            }
        }
    }

    private void ChargeAttackMove()
    {
        player.ChargeAttackMove();
    }

    private void BeStunnedTrigger()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll
            (player.attackCheck[player.attackCount].position, player.attackCheckRadius[player.attackCount]);

        foreach (var hit in colliders)
        {
            if (hit.GetComponent<Enemy>() != null)
            {
                hit.GetComponent<Enemy>().OnHitFromTarget(player);
            }
        }
    }

    private void ExecutionEnemyTrigger()
    {

        RaycastHit2D[] hits = Physics2D.RaycastAll(player.transform.position, player.transform.right, player.executionRange);
        foreach (var hit in hits)
        {
            if (hit.collider.GetComponent<Enemy>() != null)
            {
                hit.collider.GetComponent<Enemy>().BeExecution();
            }
        }
    }

    private void CanBeBouncedAttackTrigger() => player.CanBeBouncedAttack();
    private void CanNotBeBouncedAttackTrigger() => player.CanNotBeBouncedAttack();

    private void InvinciblityTrigger() => StartCoroutine(player.Invincibility());

    public void PlaySFX(AudioClip clip)
    {
        AudioManager.Instance.PlaySFX(clip, clip.length);
    }

    public void PlaySFXGrp(AudioGrp grp)
    {
        List<AudioClip> clips = grp.clips;
        AudioClip clip = clips[Random.Range(0, clips.Count)];
        AudioManager.Instance.PlaySFX(clip, clip.length);

    }

}
