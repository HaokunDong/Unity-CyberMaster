using System.Collections.Generic;
using UnityEngine;

public class StairMovementMod
{
    public bool onStairs;

    private NormalInputMovement movement;
    private Rigidbody2D rb;
    private Vector2 stairInput;
    private List<Vector3> stairEdgePoints;

    public void Init(NormalInputMovement movement, Rigidbody2D rb)
    {
        this.movement = movement;
        this.rb = rb;
    }

    public void SetData(List<Vector3> es)
    {
        stairEdgePoints = es;
        onStairs = (stairEdgePoints != null && stairEdgePoints.Count > 0);
    }

    public void OnStairMoveInput(Vector2 input, float speed, bool isOnGround)
    {
        stairInput = onStairs ? input : Vector2.zero;

        if (!onStairs || Mathf.Abs(stairInput.x) < 0.01f)
        {
            return;
        }
        if (isOnGround)
        {
            Vector2 pos = rb.position;
            float targetX = pos.x + stairInput.x * speed * Time.deltaTime;

            // 找出 targetX 在 stairEdgePoints 的哪两个点之间
            int leftIndex = -1;
            for (int i = 0; i < stairEdgePoints.Count - 1; i++)
            {
                float x0 = stairEdgePoints[i].x;
                float x1 = stairEdgePoints[i + 1].x;

                if ((targetX >= x0 && targetX <= x1) || (targetX <= x0 && targetX >= x1))
                {
                    leftIndex = i;
                    break;
                }
            }

            if (leftIndex == -1)
            {
                rb.position = new Vector2(targetX, pos.y);
                return;
            }

            Vector3 p0 = stairEdgePoints[leftIndex];
            Vector3 p1 = stairEdgePoints[leftIndex + 1];

            // 线性插值计算高度
            float t = (targetX - p0.x) / (p1.x - p0.x);
            float targetY = Mathf.Lerp(p0.y, p1.y, t) + 1f;

            // 检测是否需要切换高度
            if (Mathf.Abs(targetY - pos.y) < 0.1f)
            {
                Vector2 newPos = Vector2.MoveTowards(pos, new Vector2(targetX, targetY), speed * Time.deltaTime);
                rb.position = newPos;
            }
            else
            {
                rb.position = new Vector2(targetX, targetY);
            }

            if(leftIndex <= 0)
            {
                movement.SetNormalCapsuleCollider2dSize();
            }
            else
            {
                movement.SetStairCapsuleCollider2dSize();
            }
        }
    }
}
