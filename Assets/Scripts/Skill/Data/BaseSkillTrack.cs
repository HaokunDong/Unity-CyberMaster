using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class BaseSkillTrack<T> where T : SkillClipBase
{
    [NonSerialized, OdinSerialize]
    [DictionaryDrawerSettings(KeyLabel = "Ö¡Êý")]
    public Dictionary<int, T> skillClipDict = new Dictionary<int, T>();

    private Dictionary<int, T> frameToClipMap;

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
