using System;
using System.Diagnostics;
using UnityEngine.Scripting;

namespace GameBase.Reflection
{
    //PreserveAttribute会保证对应的类在IL2CPP下不会被unitylink剥离
    public class BaseAttribute : PreserveAttribute
    {
        
    }
    
    //这个仅用于挂接在方法上，unity编辑器启动时会全部找一遍
    [AttributeUsage(AttributeTargets.Method)]
    [Conditional("UNITY_EDITOR")]
    public class EditorFunctionAttribute : Attribute
    {
        
    }
}