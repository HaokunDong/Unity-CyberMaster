using System.Diagnostics;

namespace GameBase.Reflection
{
    //仅支持挂载于静态方法
    public class CallByEditorMsgAttribute : EditorFunctionAttribute
    {
        public string msgName;

        public CallByEditorMsgAttribute(string msgName)
        {
            this.msgName = msgName;
        }
    }
}