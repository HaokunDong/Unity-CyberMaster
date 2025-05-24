using System.Collections.Generic;
using UnityEngine;

namespace Everlasting.Extend
{
    public static class BoundsEx
    {
        public static IEnumerable<Vector3> GetCorners(this Bounds bounds)
        {
            var max = bounds.max;
            var min = bounds.min;
            yield return min;
            yield return new Vector3(min.x, min.y, max.z);
            yield return new Vector3(min.x, max.y, max.z);
            yield return new Vector3(min.x, max.y, min.z);
            yield return max;
            yield return new Vector3(max.x, min.y, max.z);
            yield return new Vector3(max.x, min.y, min.z);
            yield return new Vector3(max.x, max.y, min.z);
        }
    }
}