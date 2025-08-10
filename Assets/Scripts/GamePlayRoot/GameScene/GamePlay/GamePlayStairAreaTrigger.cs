using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Text;
using Sirenix.OdinInspector;

public class StairAreaTrigger : GamePlayTrigger
{
    public Tilemap groundTilemap;        // 地面Tilemap引用
    public Vector2Int scanPadding = new Vector2Int(1, 1); // 扩展扫描范围
    public List<Vector3> stairEdgePoints = new();

    protected override void TriggerEnter()
    {
        World.Ins.Player.normalMovement.EnterStairArea(stairEdgePoints);
    }

    protected override void TriggerExit()
    {
        World.Ins.Player.normalMovement.EnterStairArea(null);
    }

#if UNITY_EDITOR
    [Button("获取台阶数据（多Box合并）")]
    public void BakeStairData()
    {
        stairEdgePoints.Clear();
        HashSet<Vector3> set = new();

        if (groundTilemap == null)
        {
            Debug.LogWarning("请指定地面Tilemap");
            return;
        }

        // 找到当前物体及其子物体所有 BoxCollider2D 组件
        BoxCollider2D[] boxes = GetComponentsInChildren<BoxCollider2D>();
        if (boxes == null || boxes.Length == 0)
        {
            Debug.LogWarning("没有找到任何BoxCollider2D组件");
            return;
        }

        foreach (var box in boxes)
        {
            if(box == GetComponent<BoxCollider2D>())
            {
                continue;
            }
            Bounds bounds = box.bounds;

            // 先WorldToCell转换四个角，防止符号问题导致范围错误
            Vector3Int cellA = groundTilemap.WorldToCell(new Vector3(bounds.min.x, bounds.min.y, 0));
            Vector3Int cellB = groundTilemap.WorldToCell(new Vector3(bounds.max.x, bounds.max.y, 0));

            // 保证minCell是左下，maxCell是右上
            int minCellX = Mathf.Min(cellA.x, cellB.x) - scanPadding.x;
            int maxCellX = Mathf.Max(cellA.x, cellB.x) + scanPadding.x;
            int minCellY = Mathf.Min(cellA.y, cellB.y) - scanPadding.y;
            int maxCellY = Mathf.Max(cellA.y, cellB.y) + scanPadding.y;

            Vector3Int minCell = new Vector3Int(minCellX, minCellY, 0);
            Vector3Int maxCell = new Vector3Int(maxCellX, maxCellY, 0);

            for (int x = minCell.x; x <= maxCell.x; x++)
            {
                for (int y = minCell.y; y <= maxCell.y; y++)
                {
                    Vector3Int cellPos = new Vector3Int(x, y, 0);
                    TileBase tile = groundTilemap.GetTile(cellPos);

                    if (tile != null && IsStepEdgeCell(groundTilemap, cellPos))
                    {
                        Vector3 worldPos1 = groundTilemap.CellToWorld(cellPos) + new Vector3(-1f, 1f, 0);
                        Vector3 worldPos2 = groundTilemap.CellToWorld(cellPos) + new Vector3(0f, 1f, 0);

                        set.Add(worldPos1);
                        set.Add(worldPos2);
                    }
                }
            }
        }

        // 转成列表，按 x 和 y 从大到小排序
        stairEdgePoints = set.OrderByDescending(v => v.x)
                             .ThenByDescending(v => v.y)
                             .ToList();

        Debug.Log($"Bake完成，采集到台阶边缘点: {stairEdgePoints.Count}");
    }

    private bool IsStepEdgeCell(Tilemap tilemap, Vector3Int cellPos)
    {
        Vector3Int up = new Vector3Int(cellPos.x, cellPos.y + 1, 0);
        Vector3Int left = new Vector3Int(cellPos.x - 1, cellPos.y, 0);
        Vector3Int right = new Vector3Int(cellPos.x + 1, cellPos.y, 0);

        bool upEmpty = tilemap.GetTile(up) == null;
        bool leftEmpty = tilemap.GetTile(left) == null;
        bool rightEmpty = tilemap.GetTile(right) == null;

        if (!upEmpty)
            return false;

        return leftEmpty ^ rightEmpty;
    }

    public override bool GetHierarchyComment(out string name, out Color color)
    {
        name = ZString.Concat("台阶区域触发: ", GamePlayId);
        color = GamePlayId <= 0 ? Color.red : Color.green;
        return true;
    }

    private void OnDrawGizmos()
    {
        if (stairEdgePoints == null)
            return;

        Gizmos.color = Color.yellow;
        foreach (var p in stairEdgePoints)
        {
            Gizmos.DrawSphere(p, 0.2f);
        }
    }
#endif
}
