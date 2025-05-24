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
            Debug.Log("生成！");
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
            CameraManager.Ins.CameraFollow(player.gameObject);
        }
    }

    public void OnUpdate()
    {

    }

}