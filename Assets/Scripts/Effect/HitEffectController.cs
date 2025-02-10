using UnityEngine;
public class HitEffectController : Singleton<HitEffectController>
{
    public static void Create(Vector2 pos, HitEffectInfo info)
    {
        GameObject go = GlobalRef.Ins.hitEffectPrefab.OPGet();
        go.transform.SetParent(GlobalRef.Ins.hitEffectFolder);
        go.transform.position = pos;
        go.GetComponent<HitEffect>().Init(info);
    }
}
