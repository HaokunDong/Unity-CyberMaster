using DG.Tweening;

public static class DOTweenExtension
{
    public static T IgnoreTimeScale<T>(this T tween, bool ignore = true) where T : Tween
    {
        tween.SetUpdate(ignore);
        return tween;
    }
}