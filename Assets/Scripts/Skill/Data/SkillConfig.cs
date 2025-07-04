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
    [LabelText("帧率")] public int FrameRate = 13;

    [NonSerialized, OdinSerialize, ReadOnly]
    public SkillAnimationTrack SkillAnimationData = new SkillAnimationTrack();
    [NonSerialized, OdinSerialize, ReadOnly]
    public SkillAttackTimeWindowTrack SkillAttackTimeWindowData = new SkillAttackTimeWindowTrack();
    [NonSerialized, OdinSerialize, ReadOnly]
    public SkillHitBoxTrack SkillHitBoxData = new SkillHitBoxTrack();
    [NonSerialized, OdinSerialize, ReadOnly]
    public SkillVelocityTrack SkillVelocityData = new SkillVelocityTrack();

    public bool isLoopSkill = false;

    [NonSerialized, OdinSerialize]
    public List<ISkillTrack> Tracks = new List<ISkillTrack>();

    [NonSerialized, ShowInInspector, ReadOnly]
    public GameObject owner;
    public event Func<int> OnGetFaceDir;
    public event Action OwnFacePlayer;

    public List<ISkillTrack> GetTracks()
    {
        if (!Tracks.Contains(SkillAnimationData)) Tracks.Add(SkillAnimationData);
        if (!Tracks.Contains(SkillAttackTimeWindowData)) Tracks.Add(SkillAttackTimeWindowData);
        if (!Tracks.Contains(SkillHitBoxData)) Tracks.Add(SkillHitBoxData);
        if (!Tracks.Contains(SkillVelocityData)) Tracks.Add(SkillVelocityData);
        return Tracks;
    }

    public int GetOwnFaceDir()
    {
        return OnGetFaceDir?.Invoke() ?? 1;
    }

    public void FacePlayer()
    {
        OwnFacePlayer?.Invoke();
    }

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

    public virtual void OnClipFirstFrame(int frame) { }
    public virtual void OnClipLastFrame(int frame) { }
    public virtual void OnClipUpdate(int frame) { }
}