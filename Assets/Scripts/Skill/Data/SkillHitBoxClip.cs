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

[Serializable]
public class  SkillHitBoxTrack : BaseSkillTrack<SkillHitBoxClip>
{
    [NonSerialized, ShowInInspector]
    public List<Collider2D> hits;

    public void DetectOverlaps(SkillHitBoxClip clip, Vector2 origin, int faceDir, LayerMask layerMask)
    {
        hits ??= new List<Collider2D>();
        hits.Clear();

        foreach (var box in clip.HitBoxs)
        {
            // 计算世界空间中的实际位置
            Vector2 worldCenter = origin + new Vector2(box.center.x * faceDir, box.center.y);

            // 使用 Physics2D.OverlapBox 检测是否与其他 Collider2D 有重叠
            Collider2D[] results = Physics2D.OverlapBoxAll(worldCenter, box.size, box.rotation, layerMask);

            if (results != null && results.Length > 0)
                hits.AddRange(results);
        }
    }
}
