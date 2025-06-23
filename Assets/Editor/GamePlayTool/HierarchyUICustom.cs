using GameBase.Log;
using Tools;
using UnityEditor;
using UnityEngine;

namespace GamePlayTool.Editor
{
    [InitializeOnLoad]
    public static class HierarchyUICustom
    {
        static HierarchyUICustom()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyChange;
        }

        private static void OnHierarchyChange(int instanceid, Rect selectionrect)
        {
            var go = EditorUtility.InstanceIDToObject(instanceid) as GameObject;
            if (go == null) return;
            Color defaultColor = GUI.color;
            {
                var node = go.GetComponent<ICustomHierarchyComment>();
                if (node != null && node.GetHierarchyComment(out string name, out Color color))
                {
                    var gpe = go.GetComponent<GamePlayEntity>();
                    if (gpe != null)
                    {
                        if(GUI.Button(new Rect(selectionrect.xMin + 0.8f * selectionrect.width, selectionrect.yMin, 0.15f * selectionrect.width, selectionrect.height), "复制GPId"))
                        {
                            LogUtils.Trace($"复制GamePlayId: {gpe.GamePlayId} ({gpe.name})", LogChannel.Message);
                            EditorGUIUtility.systemCopyBuffer = gpe.GamePlayId.ToString();
                        }
                        GUI.Label(new Rect(selectionrect.xMin, selectionrect.yMin, 0.8f * selectionrect.width, selectionrect.height), name, LabelStyleRight(color));
                    }
                    else
                    {
                        GUI.Label(new Rect(selectionrect.xMin, selectionrect.yMin, 0.95f * selectionrect.width, selectionrect.height), name, LabelStyleRight(color));
                    }
                }
            }

            //检测root脚本是否存在
            //if (string.Equals(go.name, "GamePlay", StringComparison.OrdinalIgnoreCase) && go.GetComponent<GamePlayRoot>() == null)
            //{
            //    if (go.GetComponentInChildren<BaseGamePlayNode>())
            //    {
            //        GUI.Label(selectionrect, "缺少GamePlayRoot脚本", LabelStyleRight(Color.red));
            //    }
            //}
            
            GUI.color = defaultColor;
        }
        
        private static GUIStyle s_labelStyleCache;
        private static GUIStyle LabelStyle(Color color) {
            if (s_labelStyleRightCache == null)
            {
                s_labelStyleRightCache = new GUIStyle("Label");
                s_labelStyleRightCache.padding = new RectOffset()
                {
                    //跳过名字前icon
                    left = EditorStyles.label.padding.left + 17,
                };
            }

            s_labelStyleRightCache.normal.textColor = color;
            return s_labelStyleRightCache;
        }

        private static GUIStyle s_labelStyleRightCache;
        private static GUIStyle LabelStyleRight(Color color) {
            if (s_labelStyleRightCache == null)
            {
                s_labelStyleRightCache = new GUIStyle("Label");
                s_labelStyleRightCache.alignment = TextAnchor.MiddleRight;
            }

            s_labelStyleRightCache.normal.textColor = color;
            return s_labelStyleRightCache;
        }
    }
}