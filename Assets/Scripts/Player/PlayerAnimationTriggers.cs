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

    private void BeStunnedTrigger()
    {
        /*Collider2D colliders = Physics2D.OverlapCircle(player.attackCheck[1].position, player.attackCheckRadius[1]);
        if(colliders.GetComponent<Enemy>() != null)
        {
            Debug.Log("111111111");
            colliders.GetComponent<Enemy>().HitTarget(player);
        }*/

        Collider2D[] colliders = Physics2D.OverlapCircleAll
            (player.attackCheck[0].position, player.attackCheckRadius[0]);

        foreach (var hit in colliders)
        {
            Debug.Log(hit.gameObject.name);
            if (hit.GetComponent<Enemy>() != null)  
            {
                Debug.Log("111111111");
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
