using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;

public interface ISkillTrack
{
    void Init(SkillConfig config, Object o);
    void Update(int frame);

    void OnSkillEnd();
}

public abstract class BaseSkillTrack<T> : ISkillTrack where T : SkillClipBase
{
    [NonSerialized, OdinSerialize]
    [DictionaryDrawerSettings(KeyLabel = "帧数")]
    public Dictionary<int, T> skillClipDict = new Dictionary<int, T>();

    public SkillConfig skillConfig { get; private set; } = null;
    public T currentClip { get; private set; } = null;

    private SortedList<int, T> sortedClips;

    public virtual void Init(SkillConfig config, Object o)
    {
        skillConfig = config;
        currentClip = null;
        BuildSortedClips();
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
                currentClip.OnClipFirstFrame(frame);
            }
            currentClip.OnClipUpdate(frame);
            var next = TryGetHitBoxClipAtFrameBinary(frame + 1);
            if(next != currentClip)
            {
                currentClip.OnClipLastFrame(frame);
            }
        }
    }

    public virtual void OnSkillEnd()
    {
    }

#if UNITY_EDITOR
    public virtual void OnSave()
    {
        BuildSortedClips();
    }
#endif

    private void BuildSortedClips()
    {
        sortedClips ??= new ();
        sortedClips.Clear();
        foreach (var kvp in skillClipDict)
        {
            if (!sortedClips.ContainsKey(kvp.Key))
            {
                sortedClips.Add(kvp.Key, kvp.Value);
            }
        }
    }

    public virtual T TryGetHitBoxClipAtFrameBinary(int frame)
    {
        if(sortedClips == null)
        {
            BuildSortedClips();
        }

        if(sortedClips == null || sortedClips.Count <= 0)
            return null;
        // frame 比所有片段都小，返回 null
        if (frame < sortedClips.Keys[0])
            return null;

        int left = 0;
        int right = sortedClips.Count - 1;
        int index = -1;

        // 二分找最大起始帧 <= frame
        while (left <= right)
        {
            int mid = (left + right) / 2;
            if (sortedClips.Keys[mid] <= frame)
            {
                index = mid;
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }

        if (index == -1)
            return null;

        var candidate = sortedClips.Values[index];
        int startFrame = sortedClips.Keys[index];
        int endFrame = startFrame + candidate.DurationFrame - 1;

        if (frame >= startFrame && frame <= endFrame)
        {
            return candidate;
        }

        return null;
    }
}
