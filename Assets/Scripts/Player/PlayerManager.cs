using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : SingletonComp<PlayerManager>
{
    public GameObject playerPrefab;
    public Player player;

    public Player CreatePlayer(GameObject playerSpawnPoint)
    {
        if(player != null)
        {
            return player;
        }
        if(player == null && playerPrefab != null)
        {
            var obj = GameObject.Instantiate(playerPrefab, playerSpawnPoint.transform.position, Quaternion.identity);
            player = obj.GetComponent<Player>();
            return player;
        }
        return null;
    }
}
