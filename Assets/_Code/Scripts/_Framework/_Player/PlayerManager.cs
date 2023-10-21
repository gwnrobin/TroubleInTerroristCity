using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class PlayerManager : NetworkSingleton<PlayerManager>
{
    public UnityEvent<Player> PlayerJoined;
    
    [SerializedDictionary("id", "player")]
    public SerializedDictionary<ulong, Player> Players = new SerializedDictionary<ulong, Player>();
    [SerializeField]
    private List<ulong> _playerNetworkIds = new List<ulong>();
    [SerializeField]
    private List<ulong> _playerPrefabIds = new List<ulong>();
    
    private ulong _localPrefabId = 0;
    
    public Player GetPlayerByObjectId(ulong objectId)
    {
        return Players.TryGetValue(objectId, out Player player) ? player : null;
    }
    
    public Player GetPlayerByNetworkId(ulong networkId)
    {
        return GetPlayerByObjectId(_playerPrefabIds[_playerNetworkIds.IndexOf(networkId)]);
    }

    public List<ulong> GetAllNetworkIds()
    {
        return _playerNetworkIds;
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

    private void HandlePlayerJoin(ulong playerId)
    {
        ulong prefabId = NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject.GetComponent<NetworkObject>().NetworkObjectId;
        
        _playerNetworkIds.Add(playerId);
        _playerPrefabIds.Add(prefabId);
        
        ulong[] prefabIds = _playerPrefabIds.ToArray();
        ulong[] networkIds = _playerNetworkIds.ToArray();

        PlayerJoined.Invoke(GetPlayerByNetworkId(playerId));
        
        SendPlayerIdsToClient(prefabIds, networkIds, playerId);
        ClientConnectedClientRPC(prefabId, playerId);
    }
    
    private void HandlePlayerLeave(ulong playerId)
    {
        int index = _playerNetworkIds.IndexOf(playerId);
        _playerNetworkIds.RemoveAt(index);
        ulong prefabId = _playerPrefabIds[index];
        _playerPrefabIds.RemoveAt(index);
        Players.Remove(prefabId);
    }

    private void SendPlayerIdsToClient(ulong[] prefabIds, ulong[] networkIds, ulong receiver)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { receiver }
            }
        };
        ClientConnectedClientRPC(prefabIds, networkIds, clientRpcParams);
    }
    
    [ClientRpc]
    private void ClientConnectedClientRPC(ulong[] prefabIds, ulong[] networkIds, ClientRpcParams clientRpcParams = default)
    {
        _localPrefabId = prefabIds[prefabIds.Length - 1];
        for (int i = 0; i < prefabIds.Length; i++)
        {
            RegisterPlayer(prefabIds[i], networkIds[i]);
        }
    }
    
    [ClientRpc]
    private void ClientConnectedClientRPC(ulong prefabId, ulong networkId)
    {
        if (prefabId == _localPrefabId)
            return;
        
        RegisterPlayer(prefabId, networkId);
    }

    private void RegisterPlayer(ulong prefabId, ulong networkId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(prefabId, out NetworkObject playerNetworkObject))
        {
            Players.Add(prefabId, playerNetworkObject.GetComponent<Player>());
            if (IsHost)
                return;
            
            _playerPrefabIds.Add(prefabId);
            _playerNetworkIds.Add(networkId);
        }
    }
}