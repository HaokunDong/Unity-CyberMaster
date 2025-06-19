using UnityEngine;

public class InteractHUD : MonoBehaviour
{
    public static InteractHUD Instance { get; private set; }

    public GameObject interactHintPrefab;
    public float floatAmplitude = 0.5f;  // 上下浮动的幅度
    public float floatFrequency = 1f;

    private Transform target;
    private GameObject interactHintInstance;
    private Camera mainCamera;

    private void Awake()
    {
        Instance = this;
        mainCamera = Camera.main;
        if (interactHintPrefab == null)
        {
            Debug.LogError("Interact Hint Prefab is not assigned in the inspector.");
            return;
        }
        interactHintInstance = Instantiate(interactHintPrefab, transform);
        interactHintInstance.SetActive(false);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            interactHintInstance.SetActive(false);
            return;
        }

        interactHintInstance.SetActive(true);
        Vector3 screenPos = mainCamera.WorldToScreenPoint(target.position);
        float offsetY = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        screenPos += new Vector3(0, offsetY, 0);
        interactHintInstance.transform.position = screenPos;
    }
}
