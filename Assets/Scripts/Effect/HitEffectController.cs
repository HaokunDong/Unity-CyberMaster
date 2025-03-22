using UnityEngine;

public class HitEffectController : Singleton<HitEffectController>
{
    public static void Create(Vector2 pos, HitEffectInfo info)
    {
        GameObject go = GlobalRef.Ins.hitEffectPrefab.OPGet();
        go.transform.SetParent(GlobalRef.Ins.hitEffectFolder);
        go.transform.position = pos;

        float randomRotation = Random.Range(0f, 180f);
        go.transform.rotation = Quaternion.Euler(0f, 0f, randomRotation);

        go.GetComponent<HitEffect>().Init(info);

        // 触发屏幕抖动
        if (CameraShake.Instance != null)
        {
            float magnitude = 0.2f;
            float duration = 0.1f;

            switch (info.type)
            {
                case HitEffectType.BlockHit:
                    magnitude = 0.2f;
                    duration = 0.15f;
                    break;
                case HitEffectType.BounceHit:
                    magnitude = 0.5f;
                    duration = 0.2f;
                    break;
                case HitEffectType.BounceHit2:
                    magnitude = 0.5f;
                    duration = 0.3f;
                    break;
            }

            Debug.Log($"Triggering Camera Shake: Duration={duration}, Magnitude={magnitude}");
            CameraShake.Instance.Shake(duration, magnitude);
        }
        else
        {
            Debug.LogError("CameraShake.Instance is NULL!");
        }
    }
}
