using Cysharp.Text;
using UnityEngine;

public class GamePlayTrigger : GamePlayEntity
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
