using UnityEngine.UI;

namespace Everlasting.Extend
{
    public static class GraphicEx
    {
        public static void SetOpacity(this Graphic graphic, float opacity)
        {
            var color = graphic.color;
            color.a = opacity;
            graphic.color = color;
        }

        public static float GetOpacity(this Graphic graphic)
        {
            var color = graphic.color;
            return color.a;
        }
    }
}