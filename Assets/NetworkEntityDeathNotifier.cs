using Unity.Netcode;
using UnityEngine;

public class NetworkEntityDeathNotifier : NetworkEntityComponent
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        Entity.Death.AddListener(() =>
        {
            if (Entity.Dead.Active)
                return;
            
            EntityDieServerRPC();
        });
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void EntityDieServerRPC()
    {
        EntityDieClientRPC();
    }
    
    [ClientRpc]
    private void EntityDieClientRPC()
    {
        if (IsHost)
            return;
        
        Entity.Dead.ForceStart();
        
        Entity.Death.Send();
    }
}
