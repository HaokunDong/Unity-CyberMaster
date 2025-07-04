using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class HitCalUnit
{
    public SkillHitBoxClip clip;
    public SkillDriver skillDriver;

    public void Set(SkillHitBoxClip c, SkillDriver sd)
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
    private static int prepareSkillCount;
    private static int playingSkillCount;
    private static HitCalUnit[] hitCalUnits = null;

    public static int PlayingSkillCount
    {
        get => playingSkillCount;
        set
        {
            playingSkillCount = value;
        }
    }

    private static Dictionary<uint, HitCalUnit> enemyHitCalUnitDict = null;
    private static HitCalUnit playerHitCalUnit = null;
    private static LayerMask PlayerLayerMask = LayerMask.GetMask("Player");
    private static LayerMask EnemyLayerMask = LayerMask.GetMask("Enemy");
    private static List<Collider2D> hits;

    public void Init()
    {
        PlayingSkillCount = 0;
        enemyHitCalUnitDict ??= new ();
        hits ??= new ();
        hitCalUnits = new HitCalUnit[10];
        for(int i = 0; i < hitCalUnits.Length; i++)
        {
            hitCalUnits[i] = new HitCalUnit();
        }
        Loop().Forget();
    }

    private async UniTask Loop()
    {
        while (true)
        {
            prepareSkillCount = 0;
            enemyHitCalUnitDict.Clear();
            playerHitCalUnit = null;
            await UniTask.WaitUntil(() => prepareSkillCount == playingSkillCount);
            if(prepareSkillCount > 0)
            {
                Check();
            }
            await UniTask.WaitForFixedUpdate();
        }
    }

    private void Check()
    {
        if(playerHitCalUnit != null)
        {

        }
        else
        {
            CheckHitPlayerBody();
        }
    }

    private void CheckHitPlayerBody()
    {
        foreach (var kv in enemyHitCalUnitDict)
        {
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
                sd.OnHit(HitResType.EnemyHitPlayerBody, kv.Key, 0, clip.HitDamageValue);
                sd.skillConfig.SkillAttackTimeWindowData.Hit(sd.CurrentFrame);
            }
        }
    }

    public static void Register(SkillHitBoxClip clip, SkillDriver sd)
    {
        if (clip != null)
        {
            hitCalUnits[prepareSkillCount].Set(clip, sd);
            if (typeof(GamePlayEnemy).IsAssignableFrom(sd.ShillOwnerGPType))
            {
                enemyHitCalUnitDict[sd.SkillOwnerGPId] = hitCalUnits[prepareSkillCount];
            }
            else if(typeof(GamePlayPlayer).IsAssignableFrom(sd.ShillOwnerGPType))
            {
                playerHitCalUnit = hitCalUnits[prepareSkillCount];
            }
        }
        prepareSkillCount++;
    }
}
