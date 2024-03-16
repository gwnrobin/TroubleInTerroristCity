using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public List<Transform> spawnPoints = new();

    public Transform GetRandomSpawnPoint()
    {
        return spawnPoints[Random.Range(0, spawnPoints.Count - 1)].transform;
    }

    public void Leave()
    {
        NetworkManager.Singleton.Shutdown();
        ProjectSceneManager.Instance.ToMainMenu();
    }
}
