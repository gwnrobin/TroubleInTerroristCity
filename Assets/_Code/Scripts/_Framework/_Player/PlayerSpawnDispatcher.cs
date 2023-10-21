using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class PlayerSpawnDispatcher : NetworkBehaviour
{
    public UnityEvent<Player> PlayerSpawn;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerSpawn;
    }

    private void OnPlayerSpawn(ulong id)
    {
        StartCoroutine(PlayerSpawned(id));

        NetworkManager.Singleton.OnClientConnectedCallback -= OnPlayerSpawn;
    }

    private IEnumerator PlayerSpawned(ulong id)
    {
        yield return new WaitUntil(() =>PlayerManager.Instance.Players.Count != 0);
        
        PlayerSpawn.Invoke(PlayerManager.Instance.GetPlayerByNetworkId(id).GetComponent<Player>());
    }

}
