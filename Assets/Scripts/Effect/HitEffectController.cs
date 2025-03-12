using UnityEngine;
public class HitEffectController : Singleton<HitEffectController>
{
    public static void Create(Vector2 pos, HitEffectInfo info)
    {
        GameObject go = GlobalRef.Ins.hitEffectPrefab.OPGet();
        go.transform.SetParent(GlobalRef.Ins.hitEffectFolder);
        go.transform.position = pos;

        // ✅ 这里添加随机旋转
        float randomRotation = Random.Range(0f, 180f);
        go.transform.rotation = Quaternion.Euler(0f, 0f, randomRotation);

        go.GetComponent<HitEffect>().Init(info);
    }
}
