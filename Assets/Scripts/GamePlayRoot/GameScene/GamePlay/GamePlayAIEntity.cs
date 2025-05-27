using Cysharp.Threading.Tasks;
using Everlasting.Extend;
using Managers;
using NodeCanvas.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GamePlayAIEntity : GamePlayEntity
{
    public Animator animator;
    public GraphOwner graphOwner;
    public Blackboard blackboard;

    public string graphPath;

    public override void Init()
    {
        base.Init();
        InitAI().Forget();
    }

    public async UniTask InitAI()
    {
        if(graphOwner != null && !graphPath.IsNullOrEmpty())
        {
            var graph = await ResourceManager.LoadAssetAsync<Graph>(graphPath, ResType.AIGraph);
            if(graph != null)
            {
                graphOwner.graph = graph; 
                if (blackboard != null && animator != null)
                {
                    blackboard.SetVariableValue("gamePlayEntity", this);
                    blackboard.SetVariableValue("animator", animator);
                    CustomAIBlackboardWriteIn(blackboard);
                }
                graphOwner.StartBehaviour();
            }
        }
    }

    public virtual void CustomAIBlackboardWriteIn(Blackboard blackboard) { }
}
