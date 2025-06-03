using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using Sirenix.Serialization;

[CreateAssetMenu(menuName = "技能", fileName = "SkillConfig")]
public class SkillConfig : SerializedScriptableObject
{
    [LabelText("技能名称")] public string SkillName;
    [LabelText("帧数上限") ,ReadOnly] public int FrameCount = 100;
    [LabelText("帧率"), ReadOnly] public int FrameRate = 30;

    [NonSerialized, OdinSerialize, ReadOnly]
    public SkillAnimationTrack SkillAnimationData = new SkillAnimationTrack();
    [NonSerialized, OdinSerialize, ReadOnly]
    public SkillHitBoxTrack SkillHitBoxData = new SkillHitBoxTrack();
    [NonSerialized, OdinSerialize, ReadOnly]
    public SkillVelocityTrack SkillVelocityData = new SkillVelocityTrack();

#if UNITY_EDITOR
    private static Action onSkillConfigValidate;

    public static void SetValidateAction(Action action)
    {
        onSkillConfigValidate = action;
    }

    private void OnValidate()
    {
        onSkillConfigValidate?.Invoke();
    }
#endif

}

[Serializable]
public abstract class SkillClipBase
{
    public int DurationFrame;
}