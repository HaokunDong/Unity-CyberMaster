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
                hit.GetComponent<Enemy>().HitTarget(player);
            }
        }
    }

    private void ChargeAttackMove()
    {
        player.ChargeAttackMove();
    }

    private void CanBeBouncedAttackTrigger() => player.CanBeBouncedAttack();
    private void CanNotBeBouncedAttackTrigger() => player.CanNotBeBouncedAttack();

    // 播放音效的方法，供动画事件调用
    public void PlaySFX(AudioClip clip)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(clip, clip.length);
        }
    }
}
