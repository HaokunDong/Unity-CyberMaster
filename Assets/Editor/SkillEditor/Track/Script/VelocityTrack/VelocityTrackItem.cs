using UnityEngine;

public class VelocityTrackItem : TrackItemBase<VelocityTrack, SkillVelocityClip>
{
    public override void ResetView(float frameUnitWidth)
    {
        base.ResetView(frameUnitWidth);
        itemStyle.SetTitle(FormatVelocity(clip.velocity));
    }

    private static string FormatVelocity(Vector2 velocity)
    {
        string FormatFloat(float value)
        {
            return value % 1 == 0 ? ((int)value).ToString() : value.ToString("0.##");
        }

        if (Mathf.Approximately(velocity.y, 0f))
        {
            return FormatFloat(velocity.x);
        }

        return $"({FormatFloat(velocity.x)}, {FormatFloat(velocity.y)})";
    }
}
