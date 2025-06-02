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
public class SkillAnimationTrack : BaseSkillTrack<SkillAnimationClip>
{
}