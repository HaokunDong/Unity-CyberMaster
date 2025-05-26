using Cysharp.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayTrigger : GamePlayAIEntity
{
#if UNITY_EDITOR
    public override bool GetHierarchyComment(out string name, out Color color)
    {
        name = ZString.Concat("´¥·¢: ", GamePlayId);
        color = GamePlayId <= 0 ? Color.red : Color.green;
        return true;
    }
#endif
}
