using System;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 特效类型
/// </summary>
public enum EffectType
{
    FrameAnimation,    // 帧动画特效
    Particle,          // 粒子特效
    SequenceFrame,     // 序列帧特效
    Composite,         // 复合特效
    UI                 // UI特效
}

/// <summary>
/// 特效播放模式
/// </summary>
public enum EffectPlayMode
{
    Once,              // 播放一次
    Loop,              // 循环播放
    PingPong,          // 往返播放
    Manual             // 手动控制
}

/// <summary>
/// 特效附着类型
/// </summary>
public enum EffectAttachType
{
    World,             // 世界坐标
    FollowTarget,      // 跟随目标
    AttachToBone,      // 附着到骨骼点
    ScreenSpace,       // 屏幕空间
    UISpace            // UI空间
}

/// <summary>
/// 特效配置
/// </summary>
[CreateAssetMenu(menuName = "特效系统/特效配置", fileName = "EffectConfig")]
public class EffectConfig : ScriptableObject
{
    [LabelText("特效名称")] public string effectName;
    [LabelText("特效类型")] public EffectType effectType;
    [LabelText("播放模式")] public EffectPlayMode playMode = EffectPlayMode.Once;
    [LabelText("持续时间(帧)")] public int durationFrames = 30;
    [LabelText("帧率")] public int frameRate = 24;
    [LabelText("时间组")] public TimeGroup timeGroup = TimeGroup.VFX;
    [LabelText("排序层")] public string sortingLayerName = "Effect";
    [LabelText("排序顺序")] public int sortingOrder = 0;

    [ShowIf("effectType", EffectType.FrameAnimation)]
    [LabelText("动画剪辑")] public AnimationClip animationClip;

    [ShowIf("effectType", EffectType.Particle)]
    [LabelText("粒子预制体")] public GameObject particlePrefab;

    [ShowIf("effectType", EffectType.SequenceFrame)]
    [LabelText("序列帧精灵")] public Sprite[] frameSprites;

    [ShowIf("effectType", EffectType.Composite)]
    [LabelText("子特效")] public EffectElement[] subEffects;

    [LabelText("音效配置")] public EffectAudioConfig audioConfig;
    [LabelText("缩放配置")] public EffectScaleConfig scaleConfig;
    [LabelText("移动配置")] public EffectMovementConfig movementConfig;
}

/// <summary>
/// 特效元素(用于复合特效)
/// </summary>
[Serializable]
public class EffectElement
{
    [LabelText("子特效配置")] public EffectConfig effectConfig;
    [LabelText("延迟帧数")] public int delayFrames;
    [LabelText("相对位置")] public Vector3 relativePosition;
    [LabelText("相对旋转")] public Vector3 relativeRotation;
    [LabelText("相对缩放")] public Vector3 relativeScale = Vector3.one;
}

/// <summary>
/// 特效音效配置
/// </summary>
[Serializable]
public class EffectAudioConfig
{
    [LabelText("播放音效")] public bool playAudio;
    [LabelText("音效名称")] public string audioEventName;
    [LabelText("延迟播放(帧)")] public int delayFrames;
}

/// <summary>
/// 特效缩放配置
/// </summary>
[Serializable]
public class EffectScaleConfig
{
    [LabelText("启用缩放动画")] public bool enableScale;
    [LabelText("缩放曲线")] public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 1);
    [LabelText("起始缩放")] public Vector3 startScale = Vector3.one;
    [LabelText("结束缩放")] public Vector3 endScale = Vector3.one;
}

/// <summary>
/// 特效移动配置
/// </summary>
[Serializable]
public class EffectMovementConfig
{
    [LabelText("启用移动")] public bool enableMovement;
    [LabelText("移动曲线")] public AnimationCurve movementCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [LabelText("移动方向")] public Vector3 moveDirection = Vector3.up;
    [LabelText("移动距离")] public float moveDistance = 1f;
}
