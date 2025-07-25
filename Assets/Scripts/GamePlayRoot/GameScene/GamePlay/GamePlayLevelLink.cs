using Cysharp.Text;
using UnityEngine;

public class GamePlayLevelLink : GamePlayEntity
{
    public uint RootId;
    public Vector2 loadCenter;
    public Vector2 loadSize;
    public Vector2 enterCenter;
    public Vector2 enterSize;
    public Vector2 lockPoint;

#if UNITY_EDITOR
    public override bool GetHierarchyComment(out string name, out Color color)
    {
        name = ZString.Concat("¹Ø¿¨Á¬Í¨: ", GamePlayId);
        color = Color.green;
        return true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube((Vector2)transform.position + loadCenter, loadSize);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube((Vector2)transform.position + enterCenter, enterSize);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere((Vector2)transform.position + lockPoint, 1f);
    }
#endif
}
