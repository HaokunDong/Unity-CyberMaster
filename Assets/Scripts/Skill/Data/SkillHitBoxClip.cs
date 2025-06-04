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
    [NonSerialized, OdinSerialize]
    public LayerMask layer = LayerMask.GetMask("Player");

    [NonSerialized]
    public SkillHitBoxTrack parentTrack;

    public override void OnClipUpdate(int frame)
    {
        base.OnClipUpdate(frame);
        var hasHit = parentTrack.skillConfig.SkillAttackTimeWindowData.HasHit(frame);
        if(!hasHit)
        {
            parentTrack.DetectOverlaps(this, layer, frame);
        }
    }
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

[Serializable]
public class  SkillHitBoxTrack : BaseSkillTrack<SkillHitBoxClip>
{
    [NonSerialized, ShowInInspector]
    public List<Collider2D> hits;

    public event Action<SkillHitBoxClip> OnHitBoxTriggered;

    public override void Init(SkillConfig config, object o)
    {
        base.Init(config, o);
        OnHitBoxTriggered = o as Action<SkillHitBoxClip> ?? throw new ArgumentException("OnHitBoxTriggered action is required for SkillHitBoxTrack initialization.");

        foreach (var clip in skillClipDict.Values)
        {
            clip.parentTrack = this;
        }
    }

    public void DetectOverlaps(SkillHitBoxClip clip, LayerMask layerMask, int frame)
    {
        hits ??= new List<Collider2D>();
        hits.Clear();

        Vector2 origin = skillConfig.owner.transform.position;
        var faceDir = skillConfig.GetOwnFaceDir();
        foreach (var box in clip.HitBoxs)
        {
            // 计算世界空间中的实际位置
            Vector2 worldCenter = origin + new Vector2(box.center.x * faceDir, box.center.y);

            // 使用 Physics2D.OverlapBox 检测是否与其他 Collider2D 有重叠
            Collider2D[] results = Physics2D.OverlapBoxAll(worldCenter, box.size, box.rotation, layerMask);

            if (results != null && results.Length > 0)
            {
                hits.AddRange(results);
                break;
            }
        }

        if (hits.Count > 0)
        {
            OnHitBoxTriggered?.Invoke(clip);
            skillConfig.SkillAttackTimeWindowData.Hit(frame);
        }
    }
}
