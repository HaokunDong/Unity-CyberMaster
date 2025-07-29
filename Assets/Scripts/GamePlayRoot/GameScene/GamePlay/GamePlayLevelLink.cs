using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System;
using System.Threading;
using UnityEngine;

public class GamePlayLevelLink : GamePlayEntity
{
    public uint RootId;
    public string gateName;
    public Vector2 loadCenter;
    public Vector2 loadSize;
    public Vector2 enterCenter;
    public Vector2 enterSize;
    public Vector2 lockPoint;

    [NonSerialized, ShowInInspector, ReadOnly]
    public bool isPlayerInLoadArea;
    [NonSerialized, ShowInInspector, ReadOnly]
    public bool isPlayerInEnterArea;

    private CancellationTokenSource unloadCTS;

    public void CheckLoadAreaBounds()
    {
        if(World.Ins.Player != null)
        {
            var la = new Bounds((Vector2)transform.position + loadCenter, loadSize);
            bool isIn = la.Contains(World.Ins.Player.transform.position);
            if (isIn && !isPlayerInLoadArea)
            {
                isPlayerInLoadArea = true;

                // 取消原来的延迟卸载
                unloadCTS?.Cancel();
                unloadCTS = null;

                World.Ins.LoadAGamePlayRoot(RootId, GetRootId(), gateName, transform.position + new Vector3(lockPoint.x, +lockPoint.y, 0)).Forget();
            }
            else if (!isIn && isPlayerInLoadArea)
            {
                isPlayerInLoadArea = false;

                // 启动延迟卸载逻辑
                unloadCTS?.Cancel();
                unloadCTS = new CancellationTokenSource();
                var token = unloadCTS.Token;

                DelayUnload(RootId, token).Forget();
            }
        }
    }

    private async UniTaskVoid DelayUnload(uint rootId, CancellationToken token)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1.0f), cancellationToken: token);
            await World.Ins.LeaveAGamePlayRoot(rootId);
        }
        catch (OperationCanceledException)
        {
            // 被取消，不做卸载
        }
    }

    public void CheckEnterBounds()
    {
        if (World.Ins.Player != null)
        {
            var ea = new Bounds((Vector2)transform.position + enterCenter, enterSize);
            bool isIn = ea.Contains(World.Ins.Player.transform.position);
            if (isIn != isPlayerInEnterArea)
            {
                isPlayerInEnterArea = isIn;
                if (isIn)
                {
                    World.Ins.PlayerEnterGamePlayRoot(GetRoot());
                }
            }
        }
    }

#if UNITY_EDITOR
    public override bool GetHierarchyComment(out string name, out Color color)
    {
        name = ZString.Concat("关卡连通: ", GamePlayId);
        color = Color.green;
        return true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube((Vector2)transform.position + loadCenter, loadSize);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube((Vector2)transform.position + enterCenter, enterSize);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere((Vector2)transform.position + lockPoint, 1f);
    }
#endif
}
