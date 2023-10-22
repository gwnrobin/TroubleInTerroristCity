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
    [SerializedDictionary("Name", "Roles")]
    public SerializedDictionary<string, List<ulong>> _roles = new SerializedDictionary<string, List<ulong>>();
    
    public UnityEvent StartPreRound;
    public UnityEvent EndPreRound;
    
    public UnityEvent StartRound;
    public UnityEvent EndRound;

    public UnityEvent TraitorWin;
    public UnityEvent InnocentWin;
    
    [SerializedDictionary("name", "Events")]
    public SerializedDictionary<string, UnityEvent> Events = new SerializedDictionary<string, UnityEvent>();

    private Dictionary<string, UnityEvent> _events = new Dictionary<string, UnityEvent>();
    
    [SerializeField] private float _minimalPlayers;
    [SerializeField] private float _roundDuration;
    [SerializeField] private float _getReadyDuration;
    
    private bool _gamemodeStarted = false;
    
    
    [SerializeField]
    private List<ulong> _playersAlive = new List<ulong>();
    
    public List<ulong> GetPeopleFromRole(string role) => _roles.TryGetValue(role, out List<ulong> list) ? list : null;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        _events.Add("StartPreRound", StartPreRound);
        _events.Add("EndPreRound", EndPreRound);
        _events.Add("StartRound", StartRound);
        _events.Add("EndRound", EndRound);
        
        Events.Add("StartPreRound", StartPreRound);
        Events.Add("EndPreRound", EndPreRound);
        Events.Add("StartRound", StartRound);
        Events.Add("EndRound", EndRound);

        foreach (var role in _roles)
        {
            Events.Add(role.Key, new UnityEvent());
        }
        
        _events.Add("TraitorWin", TraitorWin);
        _events.Add("InnocentWin", InnocentWin);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerDieServerRPC(ulong id)
    {
        if (!_gamemodeStarted)
            return;
        
        _playersAlive.Remove(id);
        CheckGameState();
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
        print(eventName);
        if (_events.TryGetValue(eventName, out UnityEvent serverEvent))
        {
            serverEvent?.Invoke();
        }
    }

    public void CheckGameReady()
    {
        if (!IsHost)
            return;

        if (PlayerManager.Instance.Players.Count + 1 < _minimalPlayers)
            return;

        StartCoroutine((GetReadyCoroutine()));
    }
    
    private IEnumerator GetReadyCoroutine()
    {
        SendEventClientRPC("StartPreRound");
        
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
