using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : SingletonComp<CameraManager>
{

    public CinemachineVirtualCamera mainCam;
    public CinemachineImpulseSource impulseSource;

    #region 镜头跟随
    public void CameraFollow(GameObject player)
    {
        mainCam.LookAt = player.transform;
        mainCam.Follow = player.transform;
    }
    #endregion

    #region 镜头缩放
    public void SmoothZoomCamera(float startSize, float targetSize, float duration)
    {
        StartCoroutine(SmoothZoom(startSize, targetSize, duration));
    }

    public void ZoomCameraToDefault()
    {
        mainCam.m_Lens.OrthographicSize = 10f;
    }

    IEnumerator SmoothZoom(float startSize, float targetSize, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            mainCam.m_Lens.OrthographicSize =  Mathf.Lerp(startSize, targetSize, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    #endregion

    #region 镜头抖动
    public void ShakeCameraWithForce(float force)
    {
        impulseSource?.GenerateImpulse(force);
    }
    #endregion
}
