using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawn : PlayerNetworkComponent
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
            return;
        
        StartCoroutine(ReSpawn());
    }


    public void ForceRespawn()
    {
        if (!IsOwner)
            return;

        StartCoroutine(ReSpawn());
    }

    private IEnumerator ReSpawn()
    {
        yield return new WaitForEndOfFrame();
        
        print("test");
        GetComponent<PlayerMovement>().enabled = false;
        transform.position = GameManager.Instance.GetRandomSpawnPoint().position;
        GetComponent<PlayerMovement>().enabled = true;
    }
}
