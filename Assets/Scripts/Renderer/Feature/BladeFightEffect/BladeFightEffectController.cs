using UnityEngine;

public class BladeFightEffectController : SingletonComp<BladeFightEffectController>
{
    public Material bladeFightMaterial;

    private float et;
    private float dt;
    private bool isActive = false;

    private int centerCount = 0;
    private int CenterCount
    {
        get => centerCount;
        set
        {
            centerCount = Mathf.Clamp(value, 0, centers.Length);
            bladeFightMaterial.SetInt("_CenterCount", centerCount);
            bladeFightMaterial.SetVectorArray("_Centers", centers);
        }
    }
    private Vector4[] centers = new Vector4[5];
    private float radius;

    public void StartBladeFightEffect(Vector3 worldPos, Camera cam, float r, float d)
    {
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

        // 只处理屏幕前方的点（z>0），否则忽略
        if (screenPos.z < 0)
            return;

        centers[0] = new Vector2(screenPos.x / Screen.width, screenPos.y / Screen.height);
        CenterCount = 1;
        radius = r;
        dt = d;
        et = 0;
        isActive = true;
    }

    void Update()
    {
        if (bladeFightMaterial == null || !isActive) return;
        et += Time.deltaTime;
        if (et > dt)
        {
            bladeFightMaterial.SetFloat("_Radius", 0);
            isActive = false;
        }
        else
        {
            bladeFightMaterial.SetFloat("_Radius", radius);
        }
    }
}
