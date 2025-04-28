using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMover : MonoBehaviour
{
    public enum MoveDirection { Left, Right }
    public MoveDirection moveDirection = MoveDirection.Right;

    public float speed = 2f;            // 移动速度
    public float resetPositionX = -10f; // 重置到起点的位置
    public float startPositionX = 10f;  // 起点位置（初始位置）

    private Vector3 moveVector;

    private void Start()
    {
        moveVector = (moveDirection == MoveDirection.Right) ? Vector2.right : Vector2.left;
    }

    private void Update()
    {
        transform.Translate(moveVector * speed * Time.deltaTime);

        if (moveDirection == MoveDirection.Right && transform.position.x > startPositionX)
        {
            Vector3 pos = transform.position;
            pos.x = resetPositionX;
            transform.position = pos;
        }
        else if (moveDirection == MoveDirection.Left && transform.position.x < resetPositionX)
        {
            Vector3 pos = transform.position;
            pos.x = startPositionX;
            transform.position = pos;
        }
    }
}
