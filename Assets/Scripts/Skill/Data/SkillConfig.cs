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
    [LabelText("帧率")] public int FrameRate = 24;
    [LabelText("是否能在空中释放的技能")] public bool isAnAirSkill = false;

    [NonSerialized, OdinSerialize, ReadOnly]
    public SkillAnimationTrack SkillAnimationData = new SkillAnimationTrack();
    [NonSerialized, OdinSerialize, ReadOnly]
    public SkillAttackTimeWindowTrack SkillAttackTimeWindowData = new SkillAttackTimeWindowTrack();
    [NonSerialized, OdinSerialize, ReadOnly]
    public SkillHitBoxTrack SkillHitBoxData = new SkillHitBoxTrack();
    [NonSerialized, OdinSerialize, ReadOnly]
    public SkillVelocityTrack SkillVelocityData = new SkillVelocityTrack();
    [NonSerialized, OdinSerialize, ReadOnly]
    public SkillBlockBoxTrack SkillBlockBoxData = new SkillBlockBoxTrack();
    [NonSerialized, OdinSerialize, ReadOnly]
    public SkillJumpFrameTrack SkillJumpFrameData = new SkillJumpFrameTrack();
    [NonSerialized, OdinSerialize, ReadOnly]
    public SkillPlayerInputTrack SkillPlayerInputData = new SkillPlayerInputTrack();

    [LabelText("技能结束连招"), GUIColor(0.3f, 0.8f, 0.8f, 1f), ShowIf("@AfterSkillCommandInputStateDict != null")] public Dictionary<CommandInputState[], string> AfterSkillCommandInputStateDict;
#if UNITY_EDITOR
    [Button("添加 技能结束连招"), GUIColor(0, 1, 0, 1), ShowIf("@AfterSkillCommandInputStateDict == null")]
    private void AddAfterSkillCommandInputStateDict()
    {
        AfterSkillCommandInputStateDict ??= new();
    }
    [Button("清除 技能结束连招"), GUIColor(1, 0, 0, 1), ShowIf("@AfterSkillCommandInputStateDict != null")]
    private void ClearAfterSkillCommandInputStateDict()
    {
        AfterSkillCommandInputStateDict?.Clear();
        AfterSkillCommandInputStateDict = null;
    }
#endif

    [LabelText("技能中变招"), GUIColor(0.3f, 0.8f, 0.8f, 1f), ShowIf("@ChangeSkillCommandInputStateDict != null")] public Dictionary<CommandInputState[], string> ChangeSkillCommandInputStateDict;
#if UNITY_EDITOR
    [Button("添加 技能中变招"), GUIColor(0, 1, 0, 1), ShowIf("@ChangeSkillCommandInputStateDict == null")]
    private void AddChangeSkillCommandInputStateDict()
    {
        ChangeSkillCommandInputStateDict ??= new();
    }
    [Button("清除 技能中变招"), GUIColor(1, 0, 0, 1), ShowIf("@ChangeSkillCommandInputStateDict != null")]
    private void ClearChangeSkillCommandInputStateDict()
    {
        ChangeSkillCommandInputStateDict?.Clear();
        ChangeSkillCommandInputStateDict = null;
    }
#endif

    [LabelText("技能中跳帧"), GUIColor(0.3f, 0.8f, 0.8f, 1f), ShowIf("@SkillJumpFrameCommandInputStateDict != null")] public Dictionary<CommandInputState[], int> SkillJumpFrameCommandInputStateDict;
#if UNITY_EDITOR
    [Button("添加 技能中跳帧"), GUIColor(0, 1, 0, 1), ShowIf("@SkillJumpFrameCommandInputStateDict == null")]
    private void AddSkillJumpFrameCommandInputStateDict()
    {
        SkillJumpFrameCommandInputStateDict ??= new();
    }
    [Button("清除 技能中跳帧"), GUIColor(1, 0, 0, 1), ShowIf("@SkillJumpFrameCommandInputStateDict != null")]
    private void ClearSkillJumpFrameCommandInputStateDict()
    {
        SkillJumpFrameCommandInputStateDict?.Clear();
        SkillJumpFrameCommandInputStateDict = null;
    }
#endif

    [LabelText("取消操作"), GUIColor(0.3f, 0.8f, 0.8f, 1f),] public CommandInputState cancelCommandInputState;

    [NonSerialized, ShowInInspector, ReadOnly]
    public List<ISkillTrack> Tracks;

    [NonSerialized, ShowInInspector, ReadOnly]
    public GamePlayEntity owner;
    [NonSerialized, ShowInInspector, ReadOnly]
    public SkillDriver skillDriver;
    public event Func<int> OnGetFaceDir;
    public event Action OwnFacePlayer;

    public List<ISkillTrack> GetTracks()
    {
        Tracks ??= new();
        Tracks.Clear();
        if (!Tracks.Contains(SkillAnimationData) && HasData(SkillAnimationData)) Tracks.Add(SkillAnimationData);
        if (!Tracks.Contains(SkillAttackTimeWindowData) && HasData(SkillAttackTimeWindowData)) Tracks.Add(SkillAttackTimeWindowData);
        if (!Tracks.Contains(SkillHitBoxData)) Tracks.Add(SkillHitBoxData ??= new());
        if (!Tracks.Contains(SkillVelocityData) && HasData(SkillVelocityData)) Tracks.Add(SkillVelocityData);
        if (!Tracks.Contains(SkillBlockBoxData)) Tracks.Add(SkillBlockBoxData ??= new());
        if (!Tracks.Contains(SkillJumpFrameData) && HasData(SkillJumpFrameData)) Tracks.Add(SkillJumpFrameData);
        if (!Tracks.Contains(SkillPlayerInputData) && HasData(SkillPlayerInputData)) Tracks.Add(SkillPlayerInputData);
        return Tracks;
    }

    private bool HasData<T>(BaseSkillTrack<T> bst) where T : SkillClipBase
    {
        return (bst != null && bst.skillClipDict != null && bst.skillClipDict.Count > 0);
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