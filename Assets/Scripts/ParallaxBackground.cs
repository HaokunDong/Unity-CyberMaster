using UnityEngine;

[ExecuteAlways]
public class ParallaxBackground : MonoBehaviour
{
    [Tooltip("要跟踪的摄像机。如果为空，将自动使用主摄像机")]
    public Transform cameraTransform;

    [Tooltip("视差因子，越小表示越远（0 = 固定不动，1 = 随相机完全移动）")]
    [Range(0f, 1f)] public float parallaxFactorX = 0.5f;
    [Range(0f, 1f)] public float parallaxFactorY = 0.5f;

    [Tooltip("是否启用无限滚动背景")]
    public bool enableInfiniteScroll = false;
    public bool scrollHorizontally = true;
    public bool scrollVertically = false;

    private Vector3 initialPosition;
    private Vector3 initialCamPosition;
    private float spriteWidth;
    private float spriteHeight;

    private void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main?.transform;

        if (cameraTransform != null)
        {
            initialPosition = transform.position;
            initialCamPosition = cameraTransform.position;
        }

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            spriteWidth = sr.bounds.size.x;
            spriteHeight = sr.bounds.size.y;
        }
    }

    private void LateUpdate()
    {
        if (cameraTransform == null) return;

        Vector3 camDelta = cameraTransform.position - initialCamPosition;
        Vector3 offset = new Vector3(
            camDelta.x * parallaxFactorX,
            camDelta.y * parallaxFactorY,
            0f);

        transform.position = initialPosition + offset;

        if (enableInfiniteScroll)
        {
            Vector3 camPos = cameraTransform.position;
            Vector3 pos = transform.position;

            if (scrollHorizontally && Mathf.Abs(camPos.x - pos.x) >= spriteWidth)
            {
                float deltaX = (camPos.x - pos.x) % spriteWidth;
                initialPosition.x += spriteWidth * Mathf.Sign(camPos.x - pos.x);
                transform.position = new Vector3(initialPosition.x + deltaX, transform.position.y, transform.position.z);
            }

            if (scrollVertically && Mathf.Abs(camPos.y - pos.y) >= spriteHeight)
            {
                float deltaY = (camPos.y - pos.y) % spriteHeight;
                initialPosition.y += spriteHeight * Mathf.Sign(camPos.y - pos.y);
                transform.position = new Vector3(transform.position.x, initialPosition.y + deltaY, transform.position.z);
            }
        }
    }
}
