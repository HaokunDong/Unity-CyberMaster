using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HitEffectType
{
    BlockHit,//格挡攻击
    BounceHit,//弹反攻击
    BounceHit2,//弹反攻击并使对面进入硬直
}
public class HitEffectInfo
{
    public HitEffectType type;
}
public class HitEffect : MonoBehaviour
{
    public void OnFinishAnim()
    {
        gameObject.OPPush();
    }
    public void Init(HitEffectInfo info)
    {

        string clipName = "idle";
        switch (info.type)
        {
            case HitEffectType.BlockHit:
                clipName = "blockHit";
                break;
            case HitEffectType.BounceHit:
                clipName = "bounceHit";
                break;
            case HitEffectType.BounceHit2:
                clipName = "bounceHit2";
                break;
        }
        Animator animator = GetComponent<Animator>();
        animator.Play(clipName);
    }
}
