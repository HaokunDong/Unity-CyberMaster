using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GamePlaySpawnPoint : GamePlayEntity
{
    public uint spawnEntityTableId;
    public bool SpawnInInit = true;

    protected bool spawned = false;
    protected bool canSpawn = true;

    public abstract UniTask Spawn();

    public abstract UniTask<T> Spawn<T>() where T : GamePlayEntity;

    public bool CheckCanSpawn()
    {
        canSpawn = spawnEntityTableId > 0 && !spawned && CustomCheck();
        return canSpawn;
    }

    public virtual bool CustomCheck()
    {
        return true;
    }
}


public abstract class GamePlaySpawnPoint<T> : GamePlaySpawnPoint
    where T : GamePlayEntity
{
    [NonSerialized, ReadOnly, ShowInInspector]
    public T spawnedEntity;

    public sealed override async UniTask Spawn()
    {
        //先检查存档等条件决定是否要产生
        if(CheckCanSpawn())
        {
            spawnedEntity = await Spawn<T>();
            spawnedEntity.TableId = spawnEntityTableId;
            spawned = true;
            GamePlayRoot.Current.AfterAnEntitySpawned(spawnedEntity, this);
            AfterSpawn();
        }
    }

    public virtual void AfterSpawn() { }
}