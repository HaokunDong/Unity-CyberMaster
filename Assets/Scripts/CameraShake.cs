using System.Collections;
using UnityEngine;
using Cinemachine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;
    private CinemachineBasicMultiChannelPerlin perlin;
    private CinemachineVirtualCamera virtualCamera;

    private void Awake()
    {
        Instance = this;
        virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();

        if (virtualCamera != null)
        {
            perlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            
            // 🚀 确保游戏启动时震动归零
            if (perlin != null)
            {
                perlin.m_AmplitudeGain = 0f;
                perlin.m_FrequencyGain = 0f;
            }
        }
    }


    public void Shake(float duration, float magnitude)
    {
        if (perlin == null)
        {
            Debug.LogError("Cinemachine Perlin Noise not found!");
            return;
        }

        StopAllCoroutines();
        StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        //Debug.Log($"[Shake Started] Duration={duration}, Magnitude={magnitude}");

        perlin.m_AmplitudeGain = magnitude * 2f;  // 🚀 增大震动强度
        perlin.m_FrequencyGain = 3.0f;  // 🚀 增大震动速度

        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            yield return null;  // 🚀 确保不会瞬间归零
        }

        perlin.m_AmplitudeGain = 0f;  // 震动结束后恢复
        perlin.m_FrequencyGain = 0f;
        
        //Debug.Log("[Shake Ended]");
    }

}
