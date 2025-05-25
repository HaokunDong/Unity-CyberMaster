using System.Collections.Generic;
using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using UnityEngine;

namespace Plugins.ParadoxNotion.CanvasCore.Extend
{
    //等待降频后的下次Update
    public class WaitForNextGraphUpdate : CustomYieldInstruction
    {
        private int m_recordUpdateCount;
        private ITaskSystem m_behaviourTree;

        public override bool keepWaiting
        {
            get
            {
                if (m_behaviourTree.UpdateCount != m_recordUpdateCount || !m_behaviourTree.agent)
                {
                    m_behaviourTree = null;
                    m_recordUpdateCount = 0;
                    s_cache.Push(this);
                    return false;
                }

                return true;
            }
        }
        
        private WaitForNextGraphUpdate()
        {
            
        }

        private static readonly Stack<WaitForNextGraphUpdate> s_cache = new Stack<WaitForNextGraphUpdate>();
        public static WaitForNextGraphUpdate Get(ITaskSystem ownerSystem)
        {
            var result = s_cache.Count > 0 ? s_cache.Pop() : new WaitForNextGraphUpdate();
            result.m_recordUpdateCount = ownerSystem.UpdateCount;
            result.m_behaviourTree = ownerSystem;
            return result;
        }
    }
}