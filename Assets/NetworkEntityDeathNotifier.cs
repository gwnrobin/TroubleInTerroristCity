using Unity.Netcode;
using UnityEngine;

public class NetworkEntityDeathNotifier : NetworkEntityComponent
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        Entity.Death.AddListener(() => EntityDieServerRPC());
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void EntityDieServerRPC()
    {
        GetComponent<NetworkObject>().Despawn();
    }
}
