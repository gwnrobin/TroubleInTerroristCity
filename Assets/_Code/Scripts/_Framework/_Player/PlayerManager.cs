using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class PlayerManager : NetworkSingleton<PlayerManager>
{
    public UnityEvent<Player> PlayerJoined;
    
    public GameObject PlayerPrefab;
    
    [SerializedDictionary("id", "player")]
    public SerializedDictionary<ulong, PlayerData> Players = new();
    
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
            HandlePlayerJoin(NetworkManager.LocalClient.ClientId);
            
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
                print(data.playerObject);
                data.PrefabId = playerObject.GetComponent<NetworkObject>().NetworkObjectId;
            
                Players[networkId] = data;
            }
        }
    }

    private void HandlePlayerJoin(ulong networkId)
    {
        if (Players.ContainsKey(networkId))
            return;
        
        PlayerData playerData = new PlayerData();
        Players.Add(networkId, playerData);
        
        ulong[] networkIds = GetAllNetworkIds();
        
        SendPlayerIdsToClient(networkIds, networkId);
        
        TroubleInTerroristGamemode.Instance.PlayerRegisterDeath(networkId);
        TroubleInTerroristGamemode.Instance.CheckGameReady();
    }
    
    private void HandlePlayerLeave(ulong networkId)
    {
        Players.Remove(networkId);
    }

    private void SendPlayerIdsToClient(ulong[] networkIds, ulong receiver)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { receiver }
            }
        };
        ClientConnectedClientRPC(networkIds, clientRpcParams);
    }
    
    [ClientRpc]
    private void ClientConnectedClientRPC(ulong[] networkIds, ClientRpcParams clientRpcParams = default)
    {
        for (int i = 0; i < networkIds.Length; i++)
        {
            RegisterPlayer(networkIds[i]);
        }
    }

    private void RegisterPlayer(ulong networkId)
    {
        if (IsHost)
            return;
        
        PlayerData playerData = new PlayerData();
        
        Players.Add(networkId, playerData);
    }
}

[Serializable]
public struct PlayerData
{
    public ulong PrefabId;
    public Player playerObject;
}