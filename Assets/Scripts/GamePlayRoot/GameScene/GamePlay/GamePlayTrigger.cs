using Cysharp.Text;
using GameBase.Log;
using GameScene.FlowNode.Base;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class GamePlayTrigger : GamePlayEntity
{
    //public LayerMask triggerLayerMask = LayerMask.GetMask("Player");
    public bool onceTrigger = true;

    private BoxCollider2D boxCollider;

    private static Collider2D[] overlapResults = new Collider2D[10];

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.enabled = true;
    }

    public void Check()
    {
        if(enabled)
        {
            ContactFilter2D filter = new();
            filter.SetLayerMask(LayerMask.GetMask("Player"));
            filter.useTriggers = true;

            int count = boxCollider.OverlapCollider(filter, overlapResults);
            if (count > 0)
            {
                if(onceTrigger)
                {
                    Disable();
                }

                GamePlayRoot.Current?.SendGamePlayMsg(new OnTriggerEventMsg
                {
                    TriggerGamePlayId = GamePlayId,
                });
            }
        }
    }

    public void Disable()
    {
        enabled = false;
        boxCollider.enabled = false;
    }

#if UNITY_EDITOR
    public override bool GetHierarchyComment(out string name, out Color color)
    {
        name = ZString.Concat("´¥·¢: ", GamePlayId);
        color = GamePlayId <= 0 ? Color.red : Color.green;
        return true;
    }

    private void OnDrawGizmos()
    {
        if(boxCollider == null)
        {
            boxCollider = GetComponent<BoxCollider2D>();
        }
        if (boxCollider != null)
        {
            Gizmos.color = Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(boxCollider.offset, boxCollider.size);

            Handles.BeginGUI();
            Vector3 worldPos = transform.position;
            Vector2 screenPos = HandleUtility.WorldToGUIPoint(worldPos);

            GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
            style.fontSize = 12;
            style.normal.textColor = Color.green;
            GUI.Label(new Rect(screenPos.x, screenPos.y, 200, 60), ZString.Concat("´¥·¢ ", "\r\n", GamePlayId), style);
            Handles.EndGUI();
        }
    }
#endif
}
