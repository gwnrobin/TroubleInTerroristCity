using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class PlayerManager : NetworkSingleton<PlayerManager>
{
    public UnityEvent<Player> OnOwnerSpawn;
    
    public Dictionary<ulong, Player> Players = new Dictionary<ulong, Player>();
    private List<ulong> _playerServerList = new List<ulong>();
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsHost)
        { 
            NetworkManager.Singleton.OnClientConnectedCallback += AddPlayerToList;
        }
    }

    private void AddPlayerToList(ulong id)
    {
        _playerServerList.Add(NetworkManager.Singleton.ConnectedClients[id].PlayerObject.GetComponent<NetworkObject>().NetworkObjectId);
        
        ulong[] players = new ulong[_playerServerList.Count];

        for (int i = 0; i < _playerServerList.Count; i++)
        {
            players[i] = _playerServerList[i];
        }
        
        SendPlayerPrefabIdsToClient(players, id);
    }

    private void SendPlayerPrefabIdsToClient(ulong[] id, ulong receiver)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[]{receiver}
            }
        };
        ClientConnectedClientRPC(id, clientRpcParams);
    }
 
    [ClientRpc]
    private void ClientConnectedClientRPC(ulong[] prefabId, ClientRpcParams clientRpcParams = default)
    {
        if (IsHost)
            return;
        
        foreach (var id in prefabId)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(id, out NetworkObject playerNetworkObject))
            {
                Players.Add(id, playerNetworkObject.GetComponent<Player>());
                if (clientRpcParams.Send.TargetClientIds[0] == OwnerClientId)
                {
                    if (Players.TryGetValue(id, out Player player))
                    {
                        OnOwnerSpawn.Invoke(player);
                    }
                }
            }
            
        }
    }
}
