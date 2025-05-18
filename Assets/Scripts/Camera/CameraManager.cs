    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Cinemachine;

    public class CameraManager : MonoBehaviour
    {
        public static CameraManager Instance;

        public CinemachineVirtualCamera mainCam;
        public CinemachineVirtualCamera executionCam;

        private void Awake()
        {
            Instance = this;
        }

        public void SwitchToExecutionCam()
        {
            mainCam.gameObject.SetActive(false);
            executionCam.gameObject.SetActive(true);

            CameraShake.Instance.targetCamera = executionCam;
        }

        public void SwitchToMainCam()
        {
            executionCam.gameObject.SetActive(false);
            mainCam.gameObject.SetActive(true);

            CameraShake.Instance.targetCamera = mainCam;
        }


    }
