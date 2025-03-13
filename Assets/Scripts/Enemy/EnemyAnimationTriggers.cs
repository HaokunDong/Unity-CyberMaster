using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimationTriggers : MonoBehaviour
{
    private Enemy enemy => GetComponentInParent<Enemy>();

    public AudioGrp sfxGroup;     // 音效组
    public float sfxVolume = 0.5f; // 音量调整

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

    // ✅ 仅播放传入的音效
    public void PlaySFX(AudioClip clip)
    {
        if (AudioManager.Instance != null && clip != null)
        {
            AudioManager.Instance.PlaySFX(clip, sfxVolume);
        }
    }

    // ✅ 播放音效组中的随机音效
    public void PlaySFXGrp(AudioGrp grp)
    {
        if (grp != null && grp.clips.Count > 0)
        {
            AudioClip clip = grp.GetRandomClip();
            if (clip != null)
            {
                PlaySFX(clip);
            }
        }
    }
}
