using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillHitBoxClip : SkillClipBase
{
    [NonSerialized, OdinSerialize]
    public List<Box> HitBoxs = new List<Box>();
}

public class Box
{
    public Vector2 center;
    public Vector2 size;
    public float rotation;

#if UNITY_EDITOR
    [NonSerialized]
    public int boxIndex = 0;
#endif

    public Box()
    {
        center = Vector2.one;
        size = Vector2.one;
        rotation = 0f;
    }
}

public class  SkillHitBoxTrack
{
    [NonSerialized, OdinSerialize]
    [DictionaryDrawerSettings(KeyLabel = "帧数", ValueLabel = "打击数据")]
    public Dictionary<int, SkillHitBoxClip> skillHitBoxClipDict = new Dictionary<int, SkillHitBoxClip>();
}
