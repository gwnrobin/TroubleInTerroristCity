using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkSingleton<PlayerManager>
{
    public Dictionary<ulong, Humanoid> Players = new Dictionary<ulong, Humanoid>();
    public override void OnNetworkSpawn()
    {
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Players.Add(clientId, NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<Humanoid>());
        }
    }
}
