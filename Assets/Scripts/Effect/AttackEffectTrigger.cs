using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackEffectTrigger : MonoBehaviour
{
    public GameObject attackEffectPrefab; // 预设的攻击特效
    public Transform effectSpawnPoint;   // 特效生成位置
    
    // 允许在Inspector中手动分配不同的音效
    public AudioClip defaultAttackSFX;
    public float sfxVolume = 1.0f; // 在Inspector中调整音量

    private void PlayAttackEffect()
    {
        if (attackEffectPrefab != null && effectSpawnPoint != null)
        {
            Instantiate(attackEffectPrefab, effectSpawnPoint.position, effectSpawnPoint.rotation);
        }
    }

    // 播放音效的方法，可供动画事件调用，允许手动分配音效
    public void PlaySFX(AudioClip clip)
    {
        if (AudioManager.Instance != null && clip != null)
        {
            AudioManager.Instance.PlaySFX(clip, sfxVolume);
        }
    }
    
    // 直接使用默认音效的方法，可绑定到动画事件
    public void PlayDefaultSFX()
    {
        PlaySFX(defaultAttackSFX);
    }
}