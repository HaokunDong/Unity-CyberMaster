using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections.Generic;
using System;
using UnityEngine;

public class SkillAnimationClip : SkillClipBase
{
    public AnimationClip AnimationClip;
    public float TransitionTime = 0.25f;
    public bool ApplyRootMotion;
}

[Serializable]
public class SkillAnimationTrack
{
    [NonSerialized, OdinSerialize]
    [DictionaryDrawerSettings(KeyLabel = "帧数", ValueLabel = "动画数据")]
    public Dictionary<int, SkillAnimationClip> skillAnimationClipDict = new Dictionary<int, SkillAnimationClip>();
}