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

    public async UniTask InitAI(string path)
    {
        if(graphOwner != null && !path.IsNullOrEmpty())
        {
            var graph = await ResourceManager.LoadAssetAsync<Graph>(path, ResType.AIGraph);
            if(graph != null)
            {
                graphOwner.graph = graph;
                if (blackboard != null && animator != null)
                {
                    blackboard.SetVariableValue("animator", animator);
                }
                graphOwner.StartBehaviour();
            }
        }
    }
}
