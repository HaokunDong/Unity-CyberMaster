using GameBase.Log;
using Sirenix.OdinInspector;
using System;
using Tools;
using UnityEngine;

public abstract class GamePlayEntity : MonoBehaviour, ICustomHierarchyComment
{
    [ReadOnly]
    public uint GamePlayId;
    [ReadOnly, NonSerialized]
    public uint RootId = 0;
#if UNITY_EDITOR
    [Button("¸´ÖÆGamePlayId")]
    private void CopyGamePlayId()
    {
        LogUtils.Trace($"¸´ÖÆGamePlayId: {GamePlayId} ({name})", LogChannel.Message);
        GUIUtility.systemCopyBuffer = GamePlayId.ToString();
    }
#endif

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
        if(World.Ins.Player != null)
        {
            float dirToPlayer = World.Ins.Player.transform.position.x - transform.position.x;
            return (dirToPlayer > 0 && facingDir > 0) || (dirToPlayer < 0 && facingDir < 0);
        }
        return true;
    }

    public virtual bool isFacing(GamePlayEntity oe)
    {
        if (oe != null)
        {
            float dirToPlayer = oe.transform.position.x - transform.position.x;
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

    public virtual void Flip(float x)
    {
        if (x > 0 && !facingRight)
        {
            Flip();
        }
        else if (x < 0 && facingRight)
        {
            Flip();
        }
    }

    public virtual void Flip()
    {
        facingDir = facingDir * -1;
        facingRight = !facingRight;
        transform.Rotate(0, 180, 0);
    }

    public void FlipData()
    {
        facingDir = facingDir * -1;
        facingRight = !facingRight;
    }

    public uint GetRootId()
    {
        if(RootId <= 0)
        {
            RootId = GamePlayId / GamePlayRoot.GAP_L;
        }
        return RootId;
    }

    public GamePlayRoot GetRoot()
    {
        return World.Ins.GetRootByRootId(RootId);
    }

    public virtual void OnDispose()
    {
        
    }

    public virtual void OnHitBoxTrigger(HitResType hitRestype, uint attackerGPId, uint beHitterGPId, float damageBaseValue)
    {

    }

#if UNITY_EDITOR
    public abstract bool GetHierarchyComment(out string name, out Color color);
#endif
}
