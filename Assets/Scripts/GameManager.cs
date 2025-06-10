using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GameManager : SingletonComp<GameManager>
{
    public Player player;

    public void Start()
    {
        GameObject playerSpawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawnPoint");

        if (playerSpawnPoint != null && GameObject.FindGameObjectWithTag("Player") == null)
        {
            player = PlayerManager.Ins.CreatePlayer(playerSpawnPoint);

            //DontDestroyOnLoad(player.gameObject);
        }
        else
        {
            Debug.LogWarning("No spawn point found in scene!");
        }

        if (player.gameObject == null)
        {
            Debug.LogError("PlayerGameObject = NULL");
        }
        else
        {
            CameraFollow(player).Forget();
        }
    }

    private async UniTask CameraFollow(Player p)
    {
        await UniTask.WaitUntil(() => CameraManager.Ins != null && p != null && CameraManager.Ins.mainCam != null);
        CameraManager.Ins.CameraFollow(p.gameObject);
    }

    public void OnUpdate()
    {
        
    }

}