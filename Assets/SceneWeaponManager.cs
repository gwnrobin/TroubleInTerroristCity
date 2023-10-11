using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class SceneWeaponManager : NetworkSingleton<SceneWeaponManager>
{
    public List<Transform> spawnPoints = new();
    public List<GameObject> items = new();
    
    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;
        
        foreach (var spawnTransform in spawnPoints)
        {
            GameObject go = Instantiate(items[Random.Range(0, items.Count)], spawnTransform.position, Quaternion.identity);
            go.GetComponent<NetworkObject>().Spawn();
        }
    }
}
