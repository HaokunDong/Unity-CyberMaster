using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class HitCalUnit
{
    public SkillHitBoxClip clip;
    public SkillDriver skillDriver;
    public bool hasFinish;

    public void Set(SkillHitBoxClip c, SkillDriver sd)
    {
        clip = c;
        skillDriver = sd;
        hasFinish = false;
    }
}

public class BlockCalUnit
{
    public SkillBlockBoxClip clip;
    public SkillDriver skillDriver;

    public void Set(SkillBlockBoxClip c, SkillDriver sd)
    {
        clip = c;
        skillDriver = sd;
    }
}

public enum HitResType
{
    None = 0,
    EnemyHitPlayerBody = 1,
    EnemyHitPlayerBlock = 2,
    PlayerEnemyBladeFight = 3,
    PlayerHitEnemyBody = 4,
    PlayerHitEnemyBlock = 5,
}

public class SkillBoxManager
{
    private static int prepareSkillHitCount;
    private static int prepareSkillBlockCount;
    private static int playingSkillCount;
    private static HitCalUnit[] hitCalUnits = null;
    private static BlockCalUnit[] blockCalUnits = null;

    public static int PlayingSkillCount
    {
        get => playingSkillCount;
        set
        {
            playingSkillCount = value;
        }
    }

    private static Dictionary<uint, HitCalUnit> enemyHitCalUnitDict = null;
    private static Dictionary<uint, BlockCalUnit> enemyBlockCalUnitDict = null;
    private static HashSet<uint> finishedPlayerHitEnemy = null;
    private static HashSet<uint> playerHitEnemyBodyId = null;
    private static HitCalUnit playerHitCalUnit = null;
    private static BlockCalUnit playerBlockCalUnit = null;
    private static LayerMask PlayerLayerMask = LayerMask.GetMask("Player");
    private static LayerMask EnemyLayerMask = LayerMask.GetMask("Enemy");
    private static List<Collider2D> hits;
    private static Dictionary<uint, bool> charBeHit = null;
    private static Dictionary<uint, bool> charBeHitBlocked = null;
    private static Dictionary<uint, bool> charBladeFighted = null;
    private static Dictionary<uint, GamePlayEnemy> playerHittedEnemies = null;

    public void Init()
    {
        PlayingSkillCount = 0;
        enemyHitCalUnitDict ??= new();
        enemyBlockCalUnitDict ??= new();
        finishedPlayerHitEnemy ??= new();
        playerHitEnemyBodyId ??= new();
        hits ??= new ();
        charBeHit ??= new();
        charBeHitBlocked ??= new();
        charBladeFighted ??= new();
        playerHittedEnemies ??= new();
        hitCalUnits = new HitCalUnit[10];
        for (int i = 0; i < hitCalUnits.Length; i++)
        {
            hitCalUnits[i] = new HitCalUnit();
        }

        blockCalUnits = new BlockCalUnit[10];
        for (int i = 0; i < blockCalUnits.Length; i++)
        {
            blockCalUnits[i] = new BlockCalUnit();
        }
        Loop().Forget();
    }

    private async UniTask Loop()
    {
        while (true)
        {
            prepareSkillHitCount = 0;
            prepareSkillBlockCount = 0;
            enemyHitCalUnitDict.Clear();
            enemyBlockCalUnitDict.Clear();
            finishedPlayerHitEnemy.Clear();
            playerHitEnemyBodyId.Clear();
            playerHitCalUnit = null;
            playerBlockCalUnit = null;
            charBeHit.Clear();
            charBeHitBlocked.Clear();
            charBladeFighted.Clear();
            playerHittedEnemies.Clear();
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
        }
    }

    private static void Check()
    {
        if(playerHitCalUnit != null)
        {
            if (enemyHitCalUnitDict.Count > 0)
            {
                //检查拼刀
                CheckPlayerEnemyFladeFight();
            }

            if(enemyBlockCalUnitDict.Count > 0)
            {
                //检查玩家攻击敌人格挡
                CheckHitEnemyBlock();
            }
            //检查玩家攻击敌人身体
            CheckHitEnemyBody();
        }
        
        if(enemyHitCalUnitDict.Count > 0)
        {
            if (playerBlockCalUnit != null)
            {
                //检查敌人攻击玩家格挡
                CheckHitPlayerBlock();
            }
            //检查敌人攻击玩家身体
            CheckHitPlayerBody();
        }
    }

    private static void CheckPlayerEnemyFladeFight()
    {
        var player = playerHitCalUnit.skillDriver.skillConfig.owner;
        var playerClip = playerHitCalUnit.clip;
        Vector2 pOrigin = player.transform.position;
        var pFaceDir = playerHitCalUnit.skillDriver.skillConfig.GetOwnFaceDir();
        foreach (var kv in enemyHitCalUnitDict)
        {
            if(finishedPlayerHitEnemy.Contains(kv.Key) || kv.Value.hasFinish)
            {
                continue;
            }
            var finish = false;
            var clip = kv.Value.clip;
            var sd = kv.Value.skillDriver;
            var enemy = sd.skillConfig.owner;
            //双方要面对面才可能产生拼刀
            if(player.isFacing(enemy) && enemy.isFacing(player))
            {
                Vector2 origin = sd.skillConfig.owner.transform.position;
                var faceDir = sd.skillConfig.GetOwnFaceDir();
                foreach (var pBox in playerClip.HitBoxs)
                {
                    var (pc, ps, pr) = TransformBox(pOrigin, pFaceDir, pBox);
                    foreach (var eBox in clip.HitBoxs)
                    {
                        var (ec, es, er) = TransformBox(origin, faceDir, eBox);
                        if (BoxOverlap(pc, ps, pr, ec, es, er))
                        {
                            var hitPoint = (pc + ec) / 2f;
                            sd.OnHit(HitResType.PlayerEnemyBladeFight, kv.Key, 0, 1, hitPoint);
                            sd.skillConfig.SkillAttackTimeWindowData.Hit(sd.CurrentFrame);
                            kv.Value.hasFinish = true;

                            charBladeFighted[kv.Key] = true;
                            charBladeFighted[0] = true;

                            playerHitCalUnit.skillDriver.OnHit(HitResType.PlayerEnemyBladeFight, 0, kv.Key, 1, hitPoint);
                            finishedPlayerHitEnemy.Add(kv.Key);
                            finish = true;
                            break;
                        }
                    }
                    if(finish)
                    {
                        break;
                    }
                }
            }
        }
    }

    private static void CheckHitEnemyBlock()
    {
        var player = playerHitCalUnit.skillDriver.skillConfig.owner;
        var playerClip = playerHitCalUnit.clip;
        Vector2 pOrigin = player.transform.position;
        var pFaceDir = playerHitCalUnit.skillDriver.skillConfig.GetOwnFaceDir();
        foreach (var kv in enemyBlockCalUnitDict)
        {
            if (finishedPlayerHitEnemy.Contains(kv.Key))
            {
                continue;
            }
            var finish = false;
            var clip = kv.Value.clip;
            var sd = kv.Value.skillDriver;
            var enemy = sd.skillConfig.owner;
            //双方要面对面才可能产生格挡
            if (player.isFacing(enemy) && enemy.isFacing(player))
            {
                Vector2 origin = sd.skillConfig.owner.transform.position;
                var faceDir = sd.skillConfig.GetOwnFaceDir();
                foreach (var pBox in playerClip.HitBoxs)
                {
                    var (pc, ps, pr) = TransformBox(pOrigin, pFaceDir, pBox);
                    foreach (var eBox in clip.BlockBoxs)
                    {
                        var (ec, es, er) = TransformBox(origin, faceDir, eBox);
                        if (BoxOverlap(pc, ps, pr, ec, es, er))
                        {
                            playerHitCalUnit.skillDriver.OnHit(HitResType.PlayerHitEnemyBlock, 0, kv.Key, playerClip.HitDamageValue, ec);
                            sd.OnHit(HitResType.PlayerHitEnemyBlock, 0, kv.Key, playerClip.HitDamageValue, ec);
                            finishedPlayerHitEnemy.Add(kv.Key);
                            charBeHitBlocked[kv.Key] = true;
                            finish = true;
                            break;
                        }
                    }
                    if (finish)
                    {
                        break;
                    }
                }
            }
        }
    }

    private static void CheckHitEnemyBody()
    {
        playerHitEnemyBodyId.Clear();
        var player = playerHitCalUnit.skillDriver.skillConfig.owner;
        var playerClip = playerHitCalUnit.clip;
        Vector2 pOrigin = player.transform.position;
        var pFaceDir = playerHitCalUnit.skillDriver.skillConfig.GetOwnFaceDir();
        foreach (var box in playerClip.HitBoxs)
        {
            Vector2 worldCenter = pOrigin + new Vector2(box.center.x * pFaceDir, box.center.y);
            Collider2D[] results = Physics2D.OverlapBoxAll(worldCenter, box.size, box.rotation, EnemyLayerMask);
            if (results != null && results.Length > 0)
            {
                foreach(var res in results)
                {
                    var enemy = res.GetComponent<GamePlayEnemy>();
                    var eid = enemy?.GamePlayId ?? 0;
                    if(eid > 0 && !finishedPlayerHitEnemy.Contains(eid) && !playerHitEnemyBodyId.Contains(eid))
                    {
                        playerHitEnemyBodyId.Add(eid);
                        finishedPlayerHitEnemy.Add(eid);
                        playerHittedEnemies.Add(eid, enemy);
                    }
                }
            }
        }

        foreach (var id in playerHitEnemyBodyId)
        {
            playerHitCalUnit.skillDriver.OnHit(HitResType.PlayerHitEnemyBody, 0, id, playerClip.HitDamageValue, playerHittedEnemies[id].transform.position);
            playerHittedEnemies[id].OnHitBoxTrigger(HitResType.PlayerHitEnemyBody, 0, id, playerClip.HitDamageValue, playerHittedEnemies[id].transform.position);
            charBeHit[id] = true;
        }
    }

    private static void CheckHitPlayerBlock()
    {
        var player = playerBlockCalUnit.skillDriver.skillConfig.owner;
        var playerClip = playerBlockCalUnit.clip;
        Vector2 pOrigin = player.transform.position;
        var pFaceDir = playerBlockCalUnit.skillDriver.skillConfig.GetOwnFaceDir();
        foreach (var kv in enemyHitCalUnitDict)
        {
            if (kv.Value.hasFinish)
            {
                continue;
            }
            var finish = false;
            var clip = kv.Value.clip;
            var sd = kv.Value.skillDriver;
            var enemy = sd.skillConfig.owner;
            //双方要面对面才可能产生格挡
            if (player.isFacing(enemy) && enemy.isFacing(player))
            {
                Vector2 origin = sd.skillConfig.owner.transform.position;
                var faceDir = sd.skillConfig.GetOwnFaceDir();
                foreach (var pBox in playerClip.BlockBoxs)
                {
                    var (pc, ps, pr) = TransformBox(pOrigin, pFaceDir, pBox);
                    foreach (var eBox in clip.HitBoxs)
                    {
                        var (ec, es, er) = TransformBox(origin, faceDir, eBox);
                        if (BoxOverlap(pc, ps, pr, ec, es, er))
                        {
                            sd.OnHit(HitResType.EnemyHitPlayerBlock, kv.Key, 0, clip.HitDamageValue, pc);
                            playerHitCalUnit.skillDriver.OnHit(HitResType.EnemyHitPlayerBlock, kv.Key, 0, clip.HitDamageValue, pc);
                            sd.skillConfig.SkillAttackTimeWindowData.Hit(sd.CurrentFrame);
                            charBeHitBlocked[0] = true;
                            kv.Value.hasFinish = true;

                            finish = true;
                            break;
                        }
                    }
                    if (finish)
                    {
                        break;
                    }
                }
            }
        }
    }
    private static void CheckHitPlayerBody()
    {
        foreach (var kv in enemyHitCalUnitDict)
        {
            if(kv.Value.hasFinish)
            {
                continue;
            }
            hits.Clear();
            var clip = kv.Value.clip;
            var sd = kv.Value.skillDriver;

            Vector2 origin = sd.skillConfig.owner.transform.position;
            var faceDir = sd.skillConfig.GetOwnFaceDir();

            foreach (var box in clip.HitBoxs)
            {
                // 计算世界空间中的实际位置
                Vector2 worldCenter = origin + new Vector2(box.center.x * faceDir, box.center.y);

                // 使用 Physics2D.OverlapBox 检测是否与其他 Collider2D 有重叠
                Collider2D[] results = Physics2D.OverlapBoxAll(worldCenter, box.size, box.rotation, PlayerLayerMask);

                if (results != null && results.Length > 0)
                {
                    hits.AddRange(results);
                    break;
                }
            }

            if (hits.Count > 0)
            {
                sd.OnHit(HitResType.EnemyHitPlayerBody, kv.Key, 0, clip.HitDamageValue, World.Ins.Player.transform.position);
                World.Ins.Player.OnHitBoxTrigger(HitResType.EnemyHitPlayerBody, kv.Key, 0, clip.HitDamageValue, World.Ins.Player.transform.position);
                sd.skillConfig.SkillAttackTimeWindowData.Hit(sd.CurrentFrame);
                charBeHit[0] = true;
                kv.Value.hasFinish = true;
            }
        }
    }

    public static void RegisterHitBox(SkillHitBoxClip clip, SkillDriver sd)
    {
        if (clip != null)
        {
            hitCalUnits[prepareSkillHitCount].Set(clip, sd);
            if (typeof(GamePlayEnemy).IsAssignableFrom(sd.ShillOwnerGPType))
            {
                enemyHitCalUnitDict[sd.SkillOwnerGPId] = hitCalUnits[prepareSkillHitCount];
            }
            else if(typeof(GamePlayPlayer).IsAssignableFrom(sd.ShillOwnerGPType))
            {
                playerHitCalUnit = hitCalUnits[prepareSkillHitCount];
            }
        }
        prepareSkillHitCount++;

        if (prepareSkillHitCount == playingSkillCount && prepareSkillBlockCount == playingSkillCount)
        {
            
            if (prepareSkillHitCount > 0)
            {
                Check();
            }
        }
    }

    public static void RegisterBlockBox(SkillBlockBoxClip clip, SkillDriver sd)
    {
        if (clip != null)
        {
            blockCalUnits[prepareSkillBlockCount].Set(clip, sd);
            if (typeof(GamePlayEnemy).IsAssignableFrom(sd.ShillOwnerGPType))
            {
                enemyBlockCalUnitDict[sd.SkillOwnerGPId] = blockCalUnits[prepareSkillBlockCount];
            }
            else if (typeof(GamePlayPlayer).IsAssignableFrom(sd.ShillOwnerGPType))
            {
                playerBlockCalUnit = blockCalUnits[prepareSkillBlockCount];
            }
        }
        prepareSkillBlockCount++;

        if (prepareSkillHitCount == playingSkillCount && prepareSkillBlockCount == playingSkillCount)
        {

            if (prepareSkillHitCount > 0)
            {
                Check();
            }
        }
    }

    public static bool IsACharacterBladeFightThisFrame(uint GPId)
    {
        return charBladeFighted.ContainsKey(GPId);
    }

    public static bool IsACharacterBeHitAndBlockedThisFrame(uint GPId)
    {
        return charBeHitBlocked.ContainsKey(GPId);
    }

    public static bool IsACharacterBeHitThisFrame(uint GPId)
    {
        return charBeHit.ContainsKey(GPId);
    }

    private static (Vector2 center, Vector2 size, float rotation) TransformBox(Vector2 origin, int faceDir, Box localBox)
    {
        Vector2 worldCenter = origin + new Vector2(localBox.center.x * faceDir, localBox.center.y);
        return (worldCenter, localBox.size, localBox.rotation);
    }

    private static bool BoxOverlap(Vector2 centerA, Vector2 sizeA, float rotA, Vector2 centerB, Vector2 sizeB, float rotB)
    {
        if(Mathf.Abs(rotA) <= float.Epsilon && Mathf.Abs(rotB) <= float.Epsilon)
        {
            // 简化方案：忽略旋转，AABB 检测
            Rect a = new Rect(centerA - sizeA * 0.5f, sizeA);
            Rect b = new Rect(centerB - sizeB * 0.5f, sizeB);
            return a.Overlaps(b);
        }
        else
        {
            Vector2[] cornersA = GetBoxCorners(centerA, sizeA, rotA);
            Vector2[] cornersB = GetBoxCorners(centerB, sizeB, rotB);

            // 轴：A的两条边，B的两条边
            Vector2[] axes = new Vector2[4];
            axes[0] = GetEdgeNormal(cornersA[0], cornersA[1]);
            axes[1] = GetEdgeNormal(cornersA[1], cornersA[2]);
            axes[2] = GetEdgeNormal(cornersB[0], cornersB[1]);
            axes[3] = GetEdgeNormal(cornersB[1], cornersB[2]);

            foreach (var axis in axes)
            {
                ProjectBox(cornersA, axis, out float minA, out float maxA);
                ProjectBox(cornersB, axis, out float minB, out float maxB);

                if (maxA < minB || maxB < minA)
                {
                    return false; // 分离轴存在，无碰撞
                }
            }

            return true; // 没有分离轴，碰撞成立
        }
    }

    private static Vector2[] GetBoxCorners(Vector2 center, Vector2 size, float rotation)
    {
        float rad = rotation * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);

        Vector2 half = size * 0.5f;

        Vector2[] localCorners = new Vector2[]
        {
        new Vector2(-half.x, -half.y),
        new Vector2(half.x, -half.y),
        new Vector2(half.x, half.y),
        new Vector2(-half.x, half.y),
        };

        Vector2[] worldCorners = new Vector2[4];
        for (int i = 0; i < 4; i++)
        {
            float x = localCorners[i].x;
            float y = localCorners[i].y;
            worldCorners[i] = new Vector2(
                cos * x - sin * y,
                sin * x + cos * y
            ) + center;
        }

        return worldCorners;
    }

    private static Vector2 GetEdgeNormal(Vector2 p1, Vector2 p2)
    {
        Vector2 edge = p2 - p1;
        return new Vector2(-edge.y, edge.x).normalized;
    }

    private static void ProjectBox(Vector2[] corners, Vector2 axis, out float min, out float max)
    {
        float dot = Vector2.Dot(corners[0], axis);
        min = max = dot;

        for (int i = 1; i < corners.Length; i++)
        {
            dot = Vector2.Dot(corners[i], axis);
            if (dot < min) min = dot;
            if (dot > max) max = dot;
        }
    }
}
