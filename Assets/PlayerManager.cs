using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
    public Dictionary<ulong, Humanoid> Players = new Dictionary<ulong, Humanoid>();
    private void Start()
    {
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Players.Add(clientId, NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<Humanoid>());
        }
        
    }
    
}
