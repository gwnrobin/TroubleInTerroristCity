using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class NetworkTrigger : NetworkBehaviour
{
    public UnityEvent Triggered;
    
    public void Trigger()
    {
        TriggerServerRPC();
    }

    [ClientRpc]
    private void TriggerClientRPC()
    {
        Triggered.Invoke();
    }

    [ServerRpc(RequireOwnership = false)]
    private void TriggerServerRPC()
    {
        TriggerClientRPC();
    }
}
