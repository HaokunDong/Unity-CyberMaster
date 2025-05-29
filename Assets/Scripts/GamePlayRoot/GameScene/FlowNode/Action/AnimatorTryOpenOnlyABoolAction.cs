using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Category("Animator")]
public class AnimatorTryOpenOnlyABoolAction : ActionTask<Animator>
{
    public BBParameter<string> parameter;

    protected override void OnExecute()
    {
        if(agent != null)
        {
            var b = agent.GetBool(parameter.value);
            if(!b)
            {
                var ps = agent.parameters;
                foreach (AnimatorControllerParameter param in ps)
                {
                    if (param.type == AnimatorControllerParameterType.Bool)
                    {
                        if (param.name == parameter.value)
                        {
                            agent.SetBool(parameter.value, true);
                        }
                        else 
                        {
                            agent.SetBool(param.name, false);
                        }
                    }
                }
            }
            
        }
        EndAction(true);
    }
}
