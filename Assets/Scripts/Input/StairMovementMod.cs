using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class StairMovementMod : MonoBehaviour
{
    [ShowInInspector, ReadOnly, NonSerialized]
    public bool onStairs;

    private Rigidbody2D rb;
    private Collider2D col;
    private Vector2 stairInput;
    private List<Vector3> stairEdgePoints;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    public void SetData(List<Vector3> es)
    {
        stairEdgePoints = es;
        onStairs = (stairEdgePoints != null && stairEdgePoints.Count > 0);
        if(!onStairs)
        {
            col.isTrigger = false;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    public void OnStairMoveInput(Vector2 input, float speed, bool isOnGround)
    {
        stairInput = onStairs ? input : Vector2.zero;

        if (!onStairs || Mathf.Abs(stairInput.x) < 0.01f)
        {
            col.isTrigger = false;
            rb.bodyType = RigidbodyType2D.Dynamic;
            return;
        }
        col.isTrigger = true;
        if (isOnGround)
        {
            col.isTrigger = true;

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
                col.isTrigger = false;
                rb.bodyType = RigidbodyType2D.Dynamic;
                async UniTask M()
                {
                    await UniTask.DelayFrame(1);
                    rb.position = new Vector2(targetX, pos.y);
                }
                M().Forget();
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

            if(leftIndex <= 1 || leftIndex >= stairEdgePoints.Count - 2)
            {
                col.isTrigger = false;
                rb.bodyType = RigidbodyType2D.Dynamic;
            }
        }
        else
        {
            col.isTrigger = false;
        }
    }
}
