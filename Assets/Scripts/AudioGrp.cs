using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioGrp", menuName = "AudioGrp", order = 0)]
public class AudioGrp : ScriptableObject
{
    public List<AudioClip> clips = new();
    private int lastIndex = -1; // 记录上次播放的音效索引

    public AudioClip GetRandomClip()
    {
        if (clips.Count == 0) return null;
        if (clips.Count == 1) return clips[0]; // 只有一个音效时直接返回

        int newIndex;
        do
        {
            newIndex = Random.Range(0, clips.Count);
        } while (newIndex == lastIndex && clips.Count > 1); // 确保新音效和上次不同

        lastIndex = newIndex;
        return clips[newIndex];
    }
}