using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpawnManager : NetworkSingleton<SpawnManager>
{
    public List<GameObject> spawnables = new();
    
    public void RequestNetworkSpawn(GameObject gameObject, Vector3 position, Vector3 force)
    {
        NetworkSpawnServerRPC(spawnables.IndexOf(gameObject), position, force);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void NetworkSpawnServerRPC(int gameObjectIndex, Vector3 position, Vector3 force)
    {
        GameObject gameObject = Instantiate(spawnables[gameObjectIndex], position, Quaternion.identity);
        gameObject.GetComponent<NetworkObject>().Spawn();
        gameObject.GetComponent<Rigidbody>().AddForce(force);
        Debug.DrawRay(position, force, Color.green, 60f );
    }
}
