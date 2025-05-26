using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GamePlaySpawnPoint : GamePlayEntity
{
    public abstract T Spawn<T>(uint id) where T : GamePlayEntity;
}


public abstract class GamePlaySpawnPoint<T> : GamePlaySpawnPoint
    where T : GamePlayEntity
{
}