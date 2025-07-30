using UnityEngine;

public class PlayerInputTrack : EditorSkillTrackBase<SkillPlayerInputClip>
{
    protected override void CreateItem(int frameIndex, SkillPlayerInputClip clip)
    {
        PlayerInputTrackItem trackItem = new PlayerInputTrackItem();
        trackItem.Init(this, trackStyle, frameIndex, frameWidth, clip, new Color(0f, 0, 1, 0.5f), new Color(0f, 0, 1, 1f));
        trackItemDic.Add(frameIndex, trackItem.itemStyle);
    }
}
