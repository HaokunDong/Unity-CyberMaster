public class PlayerInputTrackItem : TrackItemBase<PlayerInputTrack, SkillPlayerInputClip>
{
    public override void ResetView(float frameUnitWidth)
    {
        base.ResetView(frameUnitWidth);
        var str = string.Empty;
        if(clip.inputToDoFlags.Has(InputToDoFlags.CancelSkill))
        {
            str += "Ïû ";
        }
        if (clip.inputToDoFlags.Has(InputToDoFlags.ChangeSkill))
        {
            str += "±ä ";
        }
        if (clip.inputToDoFlags.Has(InputToDoFlags.NextSkill))
        {
            str += "Á¬ ";
        }
        if (clip.inputToDoFlags.Has(InputToDoFlags.JumpSkillFrame))
        {
            str += "Ìø";
        }
        itemStyle.SetTitle(str.Trim());
    }
}
