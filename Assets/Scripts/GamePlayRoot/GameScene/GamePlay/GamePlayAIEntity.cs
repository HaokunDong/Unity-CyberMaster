using Cysharp.Threading.Tasks;
using Everlasting.Extend;
using Managers;
using NodeCanvas.Framework;
using NodeCanvas.Tasks.Conditions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GamePlayAIEntity : GamePlayEntity
{
    public Animator animator;
    public GraphOwner graphOwner;
    public Blackboard blackboard;
    public Rigidbody2D rb;

    public string graphPath;
    public bool pauseWhenInvisible;

    protected bool isAIActive = true;

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
                    blackboard.SetVariableValue("self", this);
                    blackboard.SetVariableValue("animator", animator);
                    blackboard.SetVariableValue("rb", rb);
                    CustomAIBlackboardWriteIn(blackboard);
                }
                graphOwner.StartBehaviour();
            }
        }
    }

    public virtual void SetAIActive(bool active)
    {
        if(!pauseWhenInvisible)
        {
            return;
        }
        if (active && !isAIActive)
        {
            isAIActive = true;
            graphOwner?.StartBehaviour();
            if(animator)
            {
                animator.enabled = true;
            }
        }
        else if(!active && isAIActive)
        {
            isAIActive = false;
            graphOwner?.PauseBehaviour();
            if (animator)
            {
                animator.enabled = false;
            }
        }
    }

    public virtual void CustomAIBlackboardWriteIn(Blackboard blackboard) { }

    public virtual void RenewBlackboard<T>(Blackboard blackboard, string key, T t) 
    {
        if (blackboard != null)
        {
            blackboard.SetVariableValue(key, t);
        }
    }

    public virtual void SetVelocity(Vector2 v)
    {
        Flip(v.x);
        rb.velocity = v;
    }
}
