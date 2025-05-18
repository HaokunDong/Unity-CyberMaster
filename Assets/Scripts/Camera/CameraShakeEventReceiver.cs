using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeEventReceiver : MonoBehaviour
{
    public void TriggerShake(string param)
    {
        // 格式：duration,magnitude，例如："0.2,2.5"
        var parts = param.Split(',');
        if (parts.Length != 2) return;

        if (float.TryParse(parts[0], out float duration) &&
            float.TryParse(parts[1], out float magnitude))
        {
            CameraShake.Instance.Shake(duration, magnitude);
        }
        else
        {
            Debug.LogWarning("Shake 参数格式错误，应为 'duration,magnitude'");
        }
    }
}
