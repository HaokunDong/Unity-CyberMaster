using Cysharp.Text;
using GameBase.Log;
using GameScene.FlowNode.Base;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public enum TriggerEventType
{
    Enter,
    Exit
}

public enum TriggerType
{
    Once = 0,
    ExitReset = 1,
}

[RequireComponent(typeof(BoxCollider2D))]
public class GamePlayTrigger : GamePlayEntity
{
    [Header("设置")]
    public TriggerType triggerType = TriggerType.Once;
    public float triggerCooldown = 0f;
    public LayerMask targetLayerMask = 1 << 6;

    private BoxCollider2D boxCollider;
    private static Collider2D[] overlapResults = new Collider2D[10];

    private bool wasInside = false;
    private bool triggered = false;
    private float lastTriggerTime = -9999f;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.enabled = true;
    }

    public void Check()
    {
        if (!enabled) return;

        ContactFilter2D filter = new();
        filter.SetLayerMask(targetLayerMask);
        filter.useTriggers = true;

        int count = boxCollider.OverlapCollider(filter, overlapResults);
        bool isInside = count > 0;
        float now = Time.time;

        // 先处理离开逻辑（确保能发送 Exit）
        if (!isInside && wasInside)
        {
            TriggerExit();

            if (triggerType == TriggerType.ExitReset)
            {
                triggered = false;
            }
            else if (triggerType == TriggerType.Once && triggered)
            {
                // 一次性触发器在发送 Exit 后禁用
                Disable();
            }
        }

        // 再处理进入逻辑
        if (isInside && !wasInside)
        {
            if (!triggered && now - lastTriggerTime >= triggerCooldown)
            {
                TriggerEnter();
                lastTriggerTime = now;

                if (triggerType == TriggerType.Once)
                {
                    triggered = true; // 标记已触发，但等 Exit 后再 Disable
                }
            }
        }

        wasInside = isInside;
    }

    private void TriggerEnter()
    {
        GamePlayRoot.Current?.SendGamePlayMsg(new OnTriggerEventMsg
        {
            TriggerGamePlayId = GamePlayId,
            Type = TriggerEventType.Enter
        });
    }

    private void TriggerExit()
    {
        GamePlayRoot.Current?.SendGamePlayMsg(new OnTriggerEventMsg
        {
            TriggerGamePlayId = GamePlayId,
            Type = TriggerEventType.Exit
        });
    }

    public void Disable()
    {
        enabled = false;
        if (boxCollider != null)
        {
            boxCollider.enabled = false;
        }
    }

#if UNITY_EDITOR
    public override bool GetHierarchyComment(out string name, out Color color)
    {
        name = ZString.Concat("触发: ", GamePlayId);
        color = GamePlayId <= 0 ? Color.red : Color.green;
        return true;
    }

    private void OnDrawGizmos()
    {
        if (boxCollider == null)
        {
            boxCollider = GetComponent<BoxCollider2D>();
        }

        if (boxCollider != null)
        {
            Gizmos.color = Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(boxCollider.offset, boxCollider.size);

#if UNITY_EDITOR
            Handles.BeginGUI();
            Vector3 worldPos = transform.position;
            Vector2 screenPos = HandleUtility.WorldToGUIPoint(worldPos);

            GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                normal = { textColor = Color.green }
            };
            GUI.Label(new Rect(screenPos.x, screenPos.y, 200, 60), ZString.Concat(triggerType, " 触发器\n", GamePlayId), style);
            Handles.EndGUI();
#endif
        }
    }
#endif
}
