using Cysharp.Threading.Tasks;
using GameBase.Log;
using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class TimeLineManager
{
    public void Init()
    {
    }

    public async UniTask LoadPrefabAndPlayTimelineAsync(string path, bool destroyAfterPlay)
    {
        var prefabObj = await ResourceManager.LoadAssetAsync<GameObject>(path, ResType.Prefab);
        if (prefabObj == null)
        {
            LogUtils.Error($"LoadPrefabAndPlayTimelineAsync 加载失败: {path}");
            return;
        }

        var director = prefabObj.GetComponentInChildren<PlayableDirector>();
        if (director == null)
        {
            LogUtils.Error($"LoadPrefabAndPlayTimelineAsync 没有找到 PlayableDirector: {path}");
            ResourceManager.Destroy(prefabObj);
            return;
        }

        director.Play();
        await UniTask.WaitUntil(() => director.state != PlayState.Playing);
        ResourceManager.Destroy(prefabObj);
    }
}
