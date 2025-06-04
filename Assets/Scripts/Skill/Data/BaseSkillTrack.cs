using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;

public interface ISkillTrack
{
    void Init(SkillConfig config, Object o);
    void Update(int frame);
}

public abstract class BaseSkillTrack<T> : ISkillTrack where T : SkillClipBase
{
    [NonSerialized, OdinSerialize]
    [DictionaryDrawerSettings(KeyLabel = "Ö¡Êý")]
    public Dictionary<int, T> skillClipDict = new Dictionary<int, T>();

    public SkillConfig skillConfig { get; private set; } = null;
    public T currentClip { get; private set; } = null;
    private Dictionary<int, T> frameToClipMap;

    public virtual void Init(SkillConfig config, Object o)
    {
        skillConfig = config;
        currentClip = null;
        BuildFrameToClipMap();
    }

    public virtual void Update(int frame)
    {
        if (frame < 0 || frame > skillConfig.FrameCount) return;
        var clip = TryGetHitBoxClipAtFrameBinary(frame);
        if(clip != null)
        {
            if(clip != currentClip)
            {
                currentClip = clip;
                currentClip.OnClipFirstFrame();
            }
            currentClip.OnClipUpdate(frame);
            var next = TryGetHitBoxClipAtFrameBinary(frame + 1);
            if(next != currentClip)
            {
                currentClip.OnClipLastFrame();
            }
        }
    }

    public virtual void OnSave()
    {
        BuildFrameToClipMap();
    }

    public virtual void BuildFrameToClipMap()
    {
        frameToClipMap ??= new Dictionary<int, T>();
        frameToClipMap.Clear();
        foreach (var kvp in skillClipDict)
        {
            int start = kvp.Key;
            T clip = kvp.Value;
            for (int i = 0; i < clip.DurationFrame; i++)
            {
                int frame = start + i;
                if (!frameToClipMap.ContainsKey(frame))
                    frameToClipMap[frame] = clip;
            }
        }
    }

    public virtual T TryGetHitBoxClipAtFrameBinary(int frame)
    {
        if (frameToClipMap == null)
        {
            BuildFrameToClipMap();
        }
        if (frameToClipMap.TryGetValue(frame, out var clip))
        {
            return clip;
        }
        return null;
    }
}
