using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuParallax : MonoBehaviour
{
    public float followStrength = 0.5f; // 跟随幅度，越小越微妙
    private Vector3 initialPosition;

    private void Start()
    {
        initialPosition = transform.position;
    }

    private void Update()
    {
        Vector2 mouseViewportPos = new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);

        float offsetX = (mouseViewportPos.x - 0.5f) * followStrength;
        float offsetY = (mouseViewportPos.y - 0.5f) * followStrength;

        transform.position = new Vector3(initialPosition.x + offsetX, initialPosition.y + offsetY, initialPosition.z);
    }
}

