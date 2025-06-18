using System.Collections.Generic;
using UnityEngine;
using ParadoxNotion.Design;
using ParadoxNotion;
using FlowCanvas.Nodes;
using Cysharp.Text;

namespace FlowCanvas.Nodes
{
    [Category("Switch")]
    [ContextDefinedInputs(typeof(Flow))]
    public abstract class SwitchGeneric<T> : FlowControlNode
    {
        [SerializeField]
        [ExposeField]
        [GatherPortsCallback]
        [MinValue(1)]
        [DelayedField]
        private int _portCount = 2;

        private ValueInput<T> valueInput;
        private List<ValueInput<T>> caseInput;
        private List<FlowOutput> caseOutput;
        private T current;

        protected override void RegisterPorts()
        {
            caseInput = new ();
            caseOutput = new ();

            valueInput = AddValueInput<T>(" ‰»Î");
            for (var i = 0; i < _portCount; i++)
            {
                caseInput.Add(AddInput(i));
                caseOutput.Add(AddFlowOutput(StringUtils.GetAlphabetLetter(i)));
            }

            AddValueOutput("Current", () => { return current; });
            AddFlowInput("In", Enter);
        }

        void Enter(Flow f)
        {
            var value = valueInput.value;
            var index = -1;
            for(int i = 0; i < caseInput.Count; i++)
            {
                if(value.Equals(caseInput[i].value))
                {
                    index = i;
                    break;
                }
            }

            if (index >= 0)
            {
                caseOutput[index].Call(f);
            }
        }

        protected abstract ValueInput<T> AddInput(int i);
    }
}

// Concrete implementations for common types
[Category("Switch")]
[Name("Switch Uint")]
public class SwitchOnUint : SwitchGeneric<uint>
{
    protected override FlowCanvas.ValueInput<uint> AddInput(int i)
    {
        return AddValueInput<uint>(ZString.Concat("Case ", StringUtils.GetAlphabetLetter(i))).SetDefaultAndSerializedValue((uint)i);
    }
}

[Category("Switch")]
[Name("Switch Int")]
public class SwitchOnInt : SwitchGeneric<int> 
{
    protected override FlowCanvas.ValueInput<int> AddInput(int i)
    {
        return AddValueInput<int>(ZString.Concat("Case ", StringUtils.GetAlphabetLetter(i))).SetDefaultAndSerializedValue(i);
    }
}

[Category("Switch")]
[Name("Switch String")]
public class SwitchOnString : SwitchGeneric<string> 
{
    protected override FlowCanvas.ValueInput<string> AddInput(int i)
    {
        return AddValueInput<string>(ZString.Concat("Case ", StringUtils.GetAlphabetLetter(i))).SetDefaultAndSerializedValue(StringUtils.GetAlphabetLetter(i));
    }
}

[Category("Switch")]
[Name("Switch Bool")]
public class SwitchOnBool : SwitchGeneric<bool> 
{
    protected override FlowCanvas.ValueInput<bool> AddInput(int i)
    {
        return AddValueInput<bool>(ZString.Concat("Case ", StringUtils.GetAlphabetLetter(i))).SetDefaultAndSerializedValue(i % 2 == 0 ? true : false);
    }
}