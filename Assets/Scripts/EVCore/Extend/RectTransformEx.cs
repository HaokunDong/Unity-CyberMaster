#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Everlasting.Extend
{
    public enum RectPositionType
    {
        Center = 0,

        Top = 1,
        Bottom = 2,
        Left = 3,
        Right = 4,

        TopLeft = 5,
        TopRight = 6,

        BottomLeft = 7,
        BottomRight = 8,
    }

    public static class RectTransformEx
    {
        public static void SetSizeAsParent(this RectTransform rect)
        {
            rect.localPosition = Vector3.zero;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.localRotation = Quaternion.identity;
        }

        //获取自己相对targetRect的位置
        public static Vector2 GetRelativeLocalPosSelf(this Transform rect, RectTransform targetRect)
        {
            Vector2 screenP = RectTransformUtility.WorldToScreenPoint(null, rect.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetRect, screenP, null,
                out var localP);
            return localP;
        }

        public static Vector2 GetLocalRectPosition(this RectTransform target, RectPositionType offsetType)
        {
            Vector2 result = Vector2.zero;
            Vector2 targetSize = target.rect.size;
            Vector2 targetPivot = target.pivot;

            Vector2 pivotOffset = Vector2.one * 0.5f - targetPivot;
            pivotOffset.x *= targetSize.x;
            pivotOffset.y *= targetSize.y;

            result += pivotOffset;

            switch (offsetType)
            {
                case RectPositionType.Top:
                    result.y += targetSize.y * 0.5f;
                    break;
                case RectPositionType.Bottom:
                    result.y -= targetSize.y * 0.5f;
                    break;
                case RectPositionType.Left:
                    result.x -= targetSize.x * 0.5f;
                    break;
                case RectPositionType.Right:
                    result.x += targetSize.x * 0.5f;
                    break;

                case RectPositionType.TopLeft:
                    result.y += targetSize.y * 0.5f;
                    result.x -= targetSize.x * 0.5f;
                    break;
                case RectPositionType.TopRight:
                    result.y += targetSize.y * 0.5f;
                    result.x += targetSize.x * 0.5f;
                    break;

                case RectPositionType.BottomLeft:
                    result.y -= targetSize.y * 0.5f;
                    result.x -= targetSize.x * 0.5f;
                    break;

                case RectPositionType.BottomRight:
                    result.y -= targetSize.y * 0.5f;
                    result.x += targetSize.x * 0.5f;
                    break;

                default:
                    break;
            }

            return result;
        }

        public static bool IsOutOfRectBound(this RectTransform boundRect, RectTransform targetRect, RectPositionType prevRectPosition, Vector2 extraOffset = default)
        {
            Vector3 checkWorldPoint;
            switch (prevRectPosition)
            {
                case RectPositionType.Top:
                    checkWorldPoint = RectPositionToWorld(targetRect, RectPositionType.Top, extraOffset);
                    return !RectTransformUtility.RectangleContainsScreenPoint(boundRect, checkWorldPoint);
                case RectPositionType.Bottom:
                    checkWorldPoint = RectPositionToWorld(targetRect, RectPositionType.Bottom, extraOffset);
                    return !RectTransformUtility.RectangleContainsScreenPoint(boundRect, checkWorldPoint);
                case RectPositionType.Left:
                    checkWorldPoint = RectPositionToWorld(targetRect, RectPositionType.Left, extraOffset);
                    return !RectTransformUtility.RectangleContainsScreenPoint(boundRect, checkWorldPoint);
                case RectPositionType.Right:
                    checkWorldPoint = RectPositionToWorld(targetRect, RectPositionType.Right, extraOffset);
                    return !RectTransformUtility.RectangleContainsScreenPoint(boundRect, checkWorldPoint);
                default:
                    break;
            }

            checkWorldPoint = RectPositionToWorld(targetRect, RectPositionType.Top, extraOffset);
            bool isTopOut = !RectTransformUtility.RectangleContainsScreenPoint(boundRect, checkWorldPoint);
            checkWorldPoint = RectPositionToWorld(targetRect, RectPositionType.Bottom, extraOffset);
            bool isBottomOut = !RectTransformUtility.RectangleContainsScreenPoint(boundRect, checkWorldPoint);
            checkWorldPoint = RectPositionToWorld(targetRect, RectPositionType.Left, extraOffset);
            bool isLeftOut = !RectTransformUtility.RectangleContainsScreenPoint(boundRect, checkWorldPoint);
            checkWorldPoint = RectPositionToWorld(targetRect, RectPositionType.Right, extraOffset);
            bool isRightOut = !RectTransformUtility.RectangleContainsScreenPoint(boundRect, checkWorldPoint);

            switch (prevRectPosition)
            {
                case RectPositionType.TopLeft:
                    return isTopOut || isLeftOut;
                case RectPositionType.TopRight:
                    return isTopOut || isRightOut;
                case RectPositionType.BottomLeft:
                    return isBottomOut || isLeftOut;
                case RectPositionType.BottomRight:
                    return isBottomOut || isRightOut;
                default:
                    break;
            }

            return isTopOut || isBottomOut || isLeftOut || isRightOut;
        }

        public static Vector3 RectPositionToWorld(this RectTransform rect, RectPositionType offsetType)
        {
            return RectPositionToWorld(rect, offsetType, Vector2.zero);
        }

        public static Vector3 RectPositionToWorld(this RectTransform rect, RectPositionType offsetType, Vector2 extraOffset)
        {
            Vector2 localPosition = GetPointInRect(rect.rect, offsetType);
            localPosition += extraOffset;
            return rect.TransformPoint(localPosition);
        }

        public static Vector2 GetPointInRect(this Rect rect, RectPositionType offsetType)
        {
            switch (offsetType)
            {
                case RectPositionType.Center: return rect.center;
                case RectPositionType.Top: return new Vector2(rect.center.x, rect.yMax);
                case RectPositionType.Bottom: return new Vector2(rect.center.x, rect.yMin);
                case RectPositionType.Left: return new Vector2(rect.xMin, rect.center.y);
                case RectPositionType.Right: return new Vector2(rect.xMax, rect.center.y);
                case RectPositionType.TopLeft: return new Vector2(rect.xMin, rect.yMax);
                case RectPositionType.TopRight: return rect.max;
                case RectPositionType.BottomLeft: return rect.min;
                case RectPositionType.BottomRight: return new Vector2(rect.xMax, rect.yMin);
                default: return rect.center;
            }
        }

        public static void ConvertToAnchorMode(this RectTransform self)
        {
            RectTransform parent = self.parent as RectTransform;
            if (parent == null)
            {
                return;
            }
#if UNITY_EDITOR
            Undo.RecordObject(self, "Set Anchors");
#endif
            Vector2 anchorMin = new Vector2(self.anchorMin.x + self.offsetMin.x / parent.rect.width,
                self.anchorMin.y + self.offsetMin.y / parent.rect.height);

            Vector2 anchorMax = new Vector2(self.anchorMax.x + self.offsetMax.x / parent.rect.width,
                self.anchorMax.y + self.offsetMax.y / parent.rect.height);

            self.anchorMin = anchorMin;
            self.anchorMax = anchorMax;
            self.offsetMin = self.offsetMax = Vector2.zero;
        }

#if UNITY_EDITOR
        [MenuItem("CONTEXT/RectTransform/Convert size to anchor")]
        public static void ConvertSize2Anchor(MenuCommand command)
        {
            RectTransform self = command.context as RectTransform;
            if (self == null || self.parent == null) return;
            ConvertToAnchorMode(self);
        }

        //         [MenuItem("CONTEXT/RectTransform/设置Anchor占满")]
        //         public static void RectTransformSetSizeAsParent(MenuCommand cmd)
        //         {
        //             var rect = cmd.context as RectTransform;
        //             SetSizeAsParent(rect);
        //             EditorUtility.SetDirty(rect);
        //         }
#endif

        public static bool IsPointInRectTransform(this RectTransform rectTransform, Vector2 uiPos)
        {
            var rect = rectTransform.rect;
            return uiPos.x >= rect.xMin && uiPos.x <= rect.xMax && uiPos.y >= rect.yMin && uiPos.y <= rect.yMax;
        }
    }
}