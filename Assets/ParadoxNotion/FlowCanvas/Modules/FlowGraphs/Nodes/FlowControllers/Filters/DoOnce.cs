using System;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes
{
    [Name("DoOnce")]
    [Category("Flow Controllers/Filters")]
    [Description("Filters Out to be called only once. After the first call, Out is no longer called until Reset is called")]
    //原生不支持存档，由项目Common_TryDoOnce代替
    [Obsolete]
    public class DoOnce : FlowControlNode
    {
        private bool called;

        public override void OnGraphStarted() { called = false; }

        protected override void RegisterPorts() {
            var o = AddFlowOutput("Out");
            AddFlowInput("In", (f) =>
                {
                    if ( !called ) {
                        called = true;
                        o.Call(f);
                    }
                });
            AddFlowInput("Reset", (f) => { called = false; });
        }
    }
}