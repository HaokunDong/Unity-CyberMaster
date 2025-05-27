using System;
using System.Diagnostics;
using GameBase.Reflection;

namespace Design.Proxy.Utils
{
    [Conditional("UNITY_EDITOR")]
    public class GenFuncStaticFromIProxyAttribute : BaseAttribute
    {
        // public string getTargetFuncName;
        //
        // public GenFuncStaticFromIProxyAttribute(string getTargetFuncName)
        // {
        //     this.getTargetFuncName = getTargetFuncName;
        // }
    }
    
    //将函数自动翻译为FlowCanvas节点
    [Conditional("UNITY_EDITOR")]
    public class GenFlowActionFromStaticFuncAttribute : BaseAttribute
    {
        public Type interfaceType;

        public GenFlowActionFromStaticFuncAttribute(){}
        
        public GenFlowActionFromStaticFuncAttribute(Type interfaceType)
        {
            this.interfaceType = interfaceType;
        }
    }

    //对应方法不生成
    [Conditional("UNITY_EDITOR")]
    public class NotGenerateForGamePlay : EditorFunctionAttribute
    {
        
    }
}