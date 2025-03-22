using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackEffectTrigger : MonoBehaviour
{
    public GameObject attackEffectPrefab; // 预设的攻击特效
    public Transform effectSpawnPoint;   // 特效生成位置
    
    public AudioClip defaultAttackSFX;  // 默认攻击音效
    public AudioGrp attackSFXGrp;       // 攻击音效组
    public float sfxVolume = 0.5f;      // 音量调整

    private void PlayAttackEffect()
    {
        if (attackEffectPrefab != null && effectSpawnPoint != null)
        {
            Instantiate(attackEffectPrefab, effectSpawnPoint.position, effectSpawnPoint.rotation);
        }
    }

    // 播放单个音效
    public void PlaySFX(AudioClip clip)
    {
        if (AudioManager.Instance != null && clip != null)
        {
            AudioManager.Instance.PlaySFX(clip, sfxVolume);
        }
    }
    
    // 播放默认攻击音效
    public void PlayDefaultSFX()
    {
        PlaySFX(defaultAttackSFX);
    }

    // 复刻 PlayerAnimationTriggers 的音效组播放逻辑
    public void PlaySFXGrp(AudioGrp grp)
    {
        if (grp != null && grp.clips.Count > 0 && AudioManager.Instance != null)
        {
            AudioClip clip = grp.GetRandomClip(); // ✅ 这里改为调用 GetRandomClip()
            AudioManager.Instance.PlaySFX(clip, sfxVolume);
        }
    }
}