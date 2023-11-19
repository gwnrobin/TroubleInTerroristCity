using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class TroubleInTerroristGamemode : NetworkSingleton<TroubleInTerroristGamemode>
{
    public UnityEvent StartPreRound;
    public UnityEvent EndPreRound;
    
    public UnityEvent StartRound;
    public UnityEvent EndRound;

    public UnityEvent TraitorWin;
    public UnityEvent InnocentWin;
    
    public UnityEvent PlayerDied;

    private Dictionary<string, UnityEvent> _events = new Dictionary<string, UnityEvent>();
    
    [SerializeField] private float _minimalPlayers;
    [SerializeField] private float _roundDuration;
    [SerializeField] private float _getReadyDuration;
    
    private bool _gamemodeStarted = false;
    
    private List<ulong> _innocents = new List<ulong>();
    private List<ulong> _traitors = new List<ulong>();
    
    [SerializeField]
    private List<ulong> _playersAlive = new List<ulong>();
    [SerializeField]
    private List<ulong> _playersDead = new List<ulong>();
    
    public List<ulong> GetInnocents => _innocents;
    public List<ulong> GetTraitors => _traitors;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        _events.Add("StartPreRound", StartPreRound);
        _events.Add("EndPreRound", EndPreRound);
        _events.Add("StartRound", StartRound);
        _events.Add("EndRound", EndRound);
        
        _events.Add("TraitorWin", TraitorWin);
        _events.Add("InnocentWin", InnocentWin);
    }

    public void PlayerDie(ulong id)
    {
        PlayerDied?.Invoke();
        PlayerDieServerRPC(id);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerDieServerRPC(ulong id)
    {
        if (!_gamemodeStarted)
            return;

        DeletePlayerPrefab(id);
        CheckGameState();
    }

    public void DeletePlayerPrefab(ulong id)
    {
        var player = PlayerManager.Instance.GetPlayerByNetworkId(id).GetComponent<NetworkObject>();
        player.RemoveOwnership();
        player.Despawn();
        _playersAlive.Remove(id);
        
        PlayerRegisterDeath(id);
    }

    public void PlayerRegisterDeath(ulong id)
    {
        _playersDead.Add(id);
    }

    public void CheckGameState()
    {
        if (AreAllPlayersDeadOfRole(_innocents))
        {
            StopAllCoroutines();
            OnRoundEnd();
            SendEventClientRPC("TraitorWin");
        }
        else if(AreAllPlayersDeadOfRole(_traitors))
        {
            StopAllCoroutines();
            OnRoundEnd();
            SendEventClientRPC("InnocentWin");
        }
    }
    
    [ClientRpc]
    private void SendEventClientRPC(string eventName, ClientRpcParams clientRpcParams = default)
    {
        if (_events.TryGetValue(eventName, out UnityEvent serverEvent))
        {
            serverEvent?.Invoke();
        }
    }

    public void CheckGameReady()
    {
        if (!IsHost)
            return;

        if (PlayerManager.Instance.Players.Count < _minimalPlayers)
            return;
        
        StartCoroutine((GetReadyCoroutine()));
    }
    
    private IEnumerator GetReadyCoroutine()
    {
        SendEventClientRPC("StartPreRound");

        foreach (var deadPlayer in _playersDead)
        {
            PlayerManager.Instance.SetNewPrefab(deadPlayer);
        }
        
        SceneWeaponManager.Instance.SpawnWeapons();
        _playersDead.Clear();
        yield return new WaitForSeconds(_getReadyDuration);
        
        SendEventClientRPC("EndPreRound");
        StartCoroutine((RoundCoroutine()));
    }
    
    private IEnumerator RoundCoroutine()
    {
        GenerateRoles();
        _gamemodeStarted = true;
        
        SendEventClientRPC("StartRound");

        // Wait for the round duration
        yield return new WaitForSeconds(_roundDuration);
        
        SendEventClientRPC("EndRound");
        SceneWeaponManager.Instance.ClearWeapons();
        // End the round when the duration is over
        OnRoundEnd();
    }

    private void OnRoundEnd()
    {
        _gamemodeStarted = false;

        _innocents.Clear();
        _traitors.Clear();

        CheckGameReady();
    }
    
    private bool AreAllPlayersDeadOfRole(List<ulong> allPlayers)
    {
        // Check if all players in the combined list are also in the _playersAlive list
        foreach (ulong playerId in allPlayers)
        {
            if (_playersAlive.Contains(playerId))
            {
                // If any player is still alive, return false
                return false;
            }
        }

        // If all players are not in _playersAlive list, return true (all are dead)
        return true;
    }

    private void GenerateRoles()
    {
        List<ulong> players = new List<ulong>(PlayerManager.Instance.GetAllNetworkIds());
        _playersAlive = new List<ulong>(players);
        int totalPlayers = players.Count;
        int numberOfTraitors = Mathf.Max(1, totalPlayers / 3);

        for (int i = 0; i < numberOfTraitors; i++)
        {
            int randomIndex = Random.Range(0, players.Count);
            _traitors.Add(players[randomIndex]);
            players.RemoveAt(randomIndex);
        }
        
        foreach (var player in players)
        {
            _innocents.Add(player);
        }
        players.Clear();
    }
}
