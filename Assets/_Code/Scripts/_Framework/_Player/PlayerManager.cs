using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class PlayerManager : NetworkSingleton<PlayerManager>
{
    public UnityEvent<Player> PlayerJoined;
    
    public GameObject PlayerPrefab;
    
    [SerializedDictionary("id", "player")]
    public SerializedDictionary<ulong, PlayerData> Players = new SerializedDictionary<ulong, PlayerData>();
    [SerializeField]
    private ulong _localPrefabId = 0;
    
    public Player GetPlayerByObjectId(ulong objectId)
    {
        foreach (var player in Players)
        {
            if (player.Value.PrefabId == objectId)
            {
                return player.Value.playerObject;
            }
        }

        return null;
    }
    
    public Player GetPlayerByNetworkId(ulong networkId)
    {
        return Players.TryGetValue(networkId, out PlayerData data) ? data.playerObject : null;
    }

    public ulong[] GetAllNetworkIds()
    {
        return Players.Keys.ToArray();
    }
    
    public ulong[] GetAllObjectIds()
    {
        List<ulong> ids = new();

        foreach (var player in Players)
        {
            ids.Add(player.Value.PrefabId);
        }
        
        return ids.ToArray();
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsHost)
        { 
            NetworkManager.Singleton.OnClientConnectedCallback += HandlePlayerJoin;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandlePlayerLeave;
        }
    }

    public void SetNewPrefab(ulong id)
    {
        GameObject player = Instantiate(NetworkManager.Singleton.GetNetworkPrefabOverride(PlayerPrefab));
        player.transform.position = GameManager.Instance.GetRandomSpawnPoint().position;
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(id);


        RegisterPrefabClientRPC(id, player.GetComponent<NetworkObject>().NetworkObjectId);
    }

    [ClientRpc]
    private void RegisterPrefabClientRPC(ulong networkId, ulong objectId)
    {
        if (Players.TryGetValue(networkId, out PlayerData data))
        {
            if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject playerObject))
            {
                data.playerObject = playerObject.GetComponent<Player>();
                data.PrefabId = playerObject.GetComponent<NetworkObject>().NetworkObjectId;
                print(data.PrefabId);
            
                Players[networkId] = data;
            }
        }
    }

    private void HandlePlayerJoin(ulong networkId)
    {
        ulong prefabId = NetworkManager.Singleton.ConnectedClients[networkId].PlayerObject.GetComponent<NetworkObject>().NetworkObjectId;
        Player player = NetworkManager.Singleton.ConnectedClients[networkId].PlayerObject.GetComponent<Player>();
        
        PlayerJoined.Invoke(player);
        
        PlayerData playerData = new PlayerData();
        playerData.PrefabId = prefabId;
        playerData.playerObject = player;
        Players.Add(networkId, playerData);
        
        ulong[] networkIds = GetAllNetworkIds();
        ulong[] prefabIds = GetAllObjectIds();
        
        SendPlayerIdsToClient(networkIds, prefabIds, networkId);
        ClientConnectedClientRPC(networkId, prefabId);
    }
    
    private void HandlePlayerLeave(ulong networkId)
    {
        Players.Remove(networkId);
    }

    private void SendPlayerIdsToClient(ulong[] networkIds, ulong[] prefabIds, ulong receiver)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { receiver }
            }
        };
        ClientConnectedClientRPC(networkIds, prefabIds, clientRpcParams);
    }
    
    [ClientRpc]
    private void ClientConnectedClientRPC(ulong[] networkIds, ulong[] prefabIds, ClientRpcParams clientRpcParams = default)
    {
        for (int i = 0; i < networkIds.Length; i++)
        {
            RegisterPlayer(networkIds[i], prefabIds[i]);
        }
    }
    
    [ClientRpc]
    private void ClientConnectedClientRPC(ulong networkId, ulong prefabId)
    {
        if (prefabId == NetworkManager.LocalClient.PlayerObject.NetworkObjectId)
            return;
        
        RegisterPlayer(networkId, prefabId);
    }

    private void RegisterPlayer(ulong networkId, ulong prefabId)
    {
        if (IsHost)
            return;
        
        PlayerData playerData = new PlayerData();
        
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(prefabId, out NetworkObject playerNetworkObject))
        {
            playerData.playerObject = playerNetworkObject.GetComponent<Player>();
            playerData.PrefabId = prefabId;
        }
        Players.Add(networkId, playerData);
    }
}

[Serializable]
public struct PlayerData
{
    public ulong PrefabId;
    public Player playerObject;
}