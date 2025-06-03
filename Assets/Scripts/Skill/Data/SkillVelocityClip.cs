using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillVelocityClip : SkillClipBase
{
    public bool FacePlayerFirst = true;
    public bool UseCurrentFacing = true;
    public Vector2 velocity;
}

[Serializable]
public class SkillVelocityTrack : BaseSkillTrack<SkillVelocityClip>
{
}
