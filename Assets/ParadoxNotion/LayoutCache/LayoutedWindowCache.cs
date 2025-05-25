using UnityEngine;

#if UNITY_EDITOR

//优化原有的GC开销
namespace ParadoxNotion.LayoutCache
{
    public class LayoutedWindowCache
    {
        private GUI.WindowFunction m_Func;
        private Rect m_ScreenRect;
        private GUILayoutOption[] m_Options;
        private GUIStyle m_Style;
        public GUI.WindowFunction windowCB;
        public LayoutedWindowCache()
        {
            windowCB = DoWindow;
        }
        
        private void DoWindow(int windowID)
        {
            GUILayoutGroup topLevel = GUILayoutUtility.current.topLevel;
            if (Event.current.type == EventType.Layout)
            {
                topLevel.resetCoords = true;
                topLevel.rect = this.m_ScreenRect;
                if (this.m_Options != null)
                    topLevel.ApplyOptions(this.m_Options);
                topLevel.isWindow = true;
                topLevel.windowID = windowID;
                topLevel.style = this.m_Style;
            }
            else
                topLevel.ResetCursor();
            this.m_Func(windowID);
        }

        public void Reset(GUI.WindowFunction f,
            Rect screenRect,
            GUIContent content,
            GUILayoutOption[] options,
            GUIStyle style)
        {
            this.m_Func = f;
            this.m_ScreenRect = screenRect;
            this.m_Options = options;
            this.m_Style = style;
        }
    }
}

#endif