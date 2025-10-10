using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Managers;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 特效管理器
/// </summary>
public class EffectManager : SingletonComp<EffectManager>
{
    [LabelText("特效预制体路径")] public string effectPrefabPath = "Prefabs/Effects/";
    [LabelText("最大特效实例数")] public int maxEffectInstances = 200;

    private uint nextEffectId = 1;
    private Dictionary<string, EffectInstance> effectPools;
    private Dictionary<uint, EffectInstance> activeEffects;
    private Transform effectParent;

    protected override void OnSingletonInit()
    {
        effectPools = new Dictionary<string, EffectInstance>();
        activeEffects = new Dictionary<uint, EffectInstance>();

        var go = new GameObject("Effects");
        go.transform.SetParent(transform);
        effectParent = go.transform;
    }

    public uint GenerateEffectId()
    {
        return nextEffectId++;
    }

    /// <summary>
    /// 播放特效
    /// </summary>
    public async UniTask<EffectInstance> PlayEffect(EffectConfig config, Vector3 position, Quaternion rotation = default)
    {
        var instance = await CreateEffect(config, effectParent);
        instance.transform.position = position;
        instance.transform.rotation = rotation;
        instance.SetAttachment(EffectAttachType.World);

        activeEffects[instance.effectId] = instance;
        instance.PlayAsync().Forget();

        return instance;
    }

    /// <summary>
    /// 播放跟随特效
    /// </summary>
    public async UniTask<EffectInstance> PlayFollowEffect(EffectConfig config, Transform target, Vector3 offset = default)
    {
        var instance = await CreateEffect(config, effectParent);
        instance.SetAttachment(EffectAttachType.FollowTarget, target, offset);

        activeEffects[instance.effectId] = instance;
        instance.PlayAsync().Forget();

        return instance;
    }

    /// <summary>
    /// 播放UI特效
    /// </summary>
    public async UniTask<EffectInstance> PlayUIEffect(EffectConfig config, Transform uiParent, Vector3 localPosition = default)
    {
        var instance = await CreateEffect(config, uiParent);
        instance.transform.localPosition = localPosition;
        instance.SetAttachment(EffectAttachType.UISpace);

        activeEffects[instance.effectId] = instance;
        instance.PlayAsync().Forget();

        return instance;
    }

    /// <summary>
    /// 创建特效实例
    /// </summary>
    public async UniTask<EffectInstance> CreateEffect(EffectConfig config, Transform parent)
    {
        EffectInstance instance = GetFromPool(config.effectName);

        if (instance == null)
        {
            var prefab = await LoadEffectPrefab(config);
            var go = Instantiate(prefab, parent);
            instance = go.GetComponent<EffectInstance>();
            if (instance == null)
            {
                instance = go.AddComponent<EffectInstance>();
            }
        }
        else
        {
            instance.transform.SetParent(parent);
            instance.gameObject.SetActive(true);
        }

        instance.Initialize(config, null);
        return instance;
    }

    private async UniTask<GameObject> LoadEffectPrefab(EffectConfig config)
    {
        string prefabPath = ZString.Concat(effectPrefabPath, config.effectName);
        return await ResourceManager.LoadAssetAsync<GameObject>(prefabPath, ResType.Prefab);
    }

    private EffectInstance GetFromPool(string effectName)
    {
        if (effectPools.ContainsKey(effectName))
        {
            var instance = effectPools[effectName];
            effectPools.Remove(effectName);
            return instance;
        }
        return null;
    }

    public void ReturnToPool(EffectInstance instance)
    {
        if (instance == null) return;

        activeEffects.Remove(instance.effectId);
        instance.gameObject.SetActive(false);

        string poolKey = instance.config.effectName;
        if (!effectPools.ContainsKey(poolKey))
        {
            effectPools[poolKey] = instance;
        }
        else
        {
            Destroy(instance.gameObject);
        }
    }

    /// <summary>
    /// 停止指定特效
    /// </summary>
    public async UniTask StopEffect(uint effectId)
    {
        if (activeEffects.TryGetValue(effectId, out var instance))
        {
            await instance.Stop();
        }
    }

    /// <summary>
    /// 停止所有特效
    /// </summary>
    public async UniTask StopAllEffects()
    {
        var tasks = new List<UniTask>();
        foreach (var instance in activeEffects.Values)
        {
            tasks.Add(instance.Stop());
        }
        await UniTask.WhenAll(tasks);
    }

    /// <summary>
    /// 暂停/恢复指定组的特效
    /// </summary>
    public void SetEffectGroupPause(TimeGroup group, bool pause)
    {
        foreach (var instance in activeEffects.Values)
        {
            if (instance.config.timeGroup == group)
            {
                if (pause)
                    instance.Pause();
                else
                    instance.Resume();
            }
        }
    }

    private void Update()
    {
        CleanupDestroyedEffects();
        CheckEffectLimit();
    }

    private void CleanupDestroyedEffects()
    {
        var toRemove = new List<uint>();
        foreach (var kv in activeEffects)
        {
            if (kv.Value == null)
            {
                toRemove.Add(kv.Key);
            }
        }
        foreach (var id in toRemove)
        {
            activeEffects.Remove(id);
        }
    }

    private void CheckEffectLimit()
    {
        if (activeEffects.Count > maxEffectInstances)
        {
            var oldestEffect = activeEffects.Values.OrderBy(e => e.effectId).First();
            oldestEffect.Stop().Forget();
        }
    }
}
