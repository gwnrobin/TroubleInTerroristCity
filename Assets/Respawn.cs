using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawn : PlayerNetworkComponent
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        GetComponent<PlayerMovement>().enabled = false;
        transform.position = GameManager.Instance.GetRandomSpawnPoint().position;
        GetComponent<PlayerMovement>().enabled = true;
    }


    public void ForceRespawn()
    {
        GetComponent<PlayerMovement>().enabled = false;
        transform.position = GameManager.Instance.GetRandomSpawnPoint().position;
        GetComponent<PlayerMovement>().enabled = true;
    }
}
