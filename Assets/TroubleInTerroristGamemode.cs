using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class TroubleInTerroristGamemode : NetworkSingleton<TroubleInTerroristGamemode>
{
    public UnityEvent StartGetReady;
    public UnityEvent EndGetReady;
    
    public UnityEvent StartRound;
    public UnityEvent EndRound;
    
    [SerializeField] private float _minimalPlayers;
    [SerializeField] private float _traitorsForEveryPlayer;
    [SerializeField] private float _roundDuration;
    [SerializeField] private float _getReadyDuration;
    
    private bool _gamemodeStarted = false;
    private bool _gamemodeReady = false;

    private float _startTimer = 0;

    [SerializeField]
    private List<ulong> _innocents = new List<ulong>();
    [SerializeField]
    private List<ulong> _traitors = new List<ulong>();

    public List<ulong> GetInnocents => _innocents;
    public List<ulong> GetTraitors => _traitors;

    public float GetCurrentTimeLeft => Mathf.Max(0f, _startTimer - Time.time);

    [ServerRpc]
    public void PlayerDieServerRPC(ulong id)
    {
        if (!_innocents.Remove(id))
        {
            _traitors.Remove(id);
        }
        CheckGameState();
    }

    public void CheckGameState()
    {
        if (_innocents.Count <= 0)
        {
            print("Innocents win");
        }
        else if(_traitors.Count <= 0)
        {
            print("trators win");
        }
    }

    public void CheckGameReady()
    {
        if (!IsHost)
            return;

        if (PlayerManager.Instance.Players.Count + 1 < _minimalPlayers)
            return;
        
        ReadyGame();
    }

    private void ReadyGame()
    {
        _gamemodeReady = true;
        StartCoroutine((GetReadyCoroutine()));
    }
    
    private IEnumerator GetReadyCoroutine()
    {
        StartGetReady?.Invoke();
        _startTimer = Time.time + _getReadyDuration;
        
        yield return new WaitForSeconds(_getReadyDuration);

        EndGetReady?.Invoke();
        StartCoroutine((RoundCoroutine()));
    }
    
    private IEnumerator RoundCoroutine()
    {
        GenerateRoles();
        
        StartRound?.Invoke();
        _startTimer = Time.time + _roundDuration;

        // Wait for the round duration
        yield return new WaitForSeconds(_roundDuration);

        EndRound?.Invoke();
        // End the round when the duration is over
        OnRoundEnd();
    }

    private void OnRoundEnd()
    {
        // Implement logic for ending the round, checking win conditions, displaying results, etc.
        Debug.Log("Round ended!");

        _innocents.Clear();
        _traitors.Clear();
        // Start the next round
        ReadyGame();
    }

    private void GenerateRoles()
    {
        List<ulong> players = new List<ulong>(PlayerManager.Instance.GetAllNetworkIds());
        int totalPlayers = players.Count;
        int numberOfTraitors = Mathf.Max(1, totalPlayers / 3);
        print(players.Count);
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
