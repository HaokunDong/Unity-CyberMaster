using Sirenix.OdinInspector;
using System;
using Tools;
using UnityEngine;

public abstract class GamePlayEntity : MonoBehaviour, ICustomHierarchyComment
{
    [ReadOnly]
    public uint GamePlayId;
    [NonSerialized, ReadOnly, ShowInInspector]
    public bool isGen;
    [NonSerialized, ReadOnly, ShowInInspector]
    public uint TableId;

    [NonSerialized, ReadOnly, ShowInInspector]
    public GamePlaySpawnPoint spawnPoint;

    public int facingDir { get; private set; } = 1;
    protected bool facingRight = true;

    public virtual void Init() { }

    public virtual bool isFacingPlayer()
    {
        if(GamePlayRoot.Current != null && GamePlayRoot.Current.player != null)
        {
            float dirToPlayer = GamePlayRoot.Current.player.transform.position.x - transform.position.x;
            return (dirToPlayer > 0 && facingDir > 0) || (dirToPlayer < 0 && facingDir < 0);
        }
        return true;
    }

    public virtual bool FacePlayer()
    {
        if(!isFacingPlayer())
        {
            Flip();
            return true;
        }
        return false;
    }

    public virtual void Flip()
    {
        facingDir = facingDir * -1;
        facingRight = !facingRight;
        transform.Rotate(0, 180, 0);
    }

#if UNITY_EDITOR
    public abstract bool GetHierarchyComment(out string name, out Color color);
#endif
}
