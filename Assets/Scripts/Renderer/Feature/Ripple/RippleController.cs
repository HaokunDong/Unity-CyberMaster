using UnityEngine;

public class RippleController : SingletonComp<RippleController>
{
    [SerializeField] private Material rippleMaterial;

    const int MaxRipples = 5;

    struct Ripple
    {
        public Vector2 uv;
        public float startTime;
        public float strength;
    }

    Ripple[] ripples = new Ripple[MaxRipples];
    int rippleIndex = 0;

    // 新增三个控制参数
    [SerializeField] private float maxRadius = 1.2f;    // 最大扩散半径（UV）
    [SerializeField] private float rippleSpeed = 3.0f;  // 扩散速度（UV单位/秒）
    [SerializeField] private float duration = 0.7f;     // 持续时间（秒）

    void Update()
    {
        if (rippleMaterial == null) return;

        rippleMaterial.SetFloat("_TimeNow", Time.time);
        rippleMaterial.SetInt("_RippleCount", MaxRipples);

        rippleMaterial.SetFloat("_MaxRadius", maxRadius);
        rippleMaterial.SetFloat("_RippleSpeed", rippleSpeed);
        rippleMaterial.SetFloat("_Duration", duration);

        Vector4[] centers = new Vector4[MaxRipples];
        for (int i = 0; i < MaxRipples; i++)
        {
            centers[i] = new Vector4(ripples[i].uv.x, ripples[i].uv.y, ripples[i].startTime, ripples[i].strength);
        }

        rippleMaterial.SetVectorArray("_RippleCenters", centers);
    }

    /// <summary>
    /// 添加涟漪，worldPos 是世界坐标，cam 是相机，strength 是涟漪强度。
    /// </summary>
    public void AddRipple(Vector3 worldPos, Camera cam, float strength = 1.0f)
    {
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

        // 只处理屏幕前方的点（z>0），否则忽略
        if (screenPos.z < 0)
            return;

        Vector2 uv = new Vector2(screenPos.x / Screen.width, screenPos.y / Screen.height);

        ripples[rippleIndex] = new Ripple
        {
            uv = uv,
            startTime = Time.time,
            strength = strength
        };

        rippleIndex = (rippleIndex + 1) % MaxRipples;
    }
}
