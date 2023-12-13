using Unity.Netcode;
using UnityEngine.Events;

public class RoleNetworkHandler : NetworkBehaviour
{
    public UnityEvent<string> ReceiveRole;
    
    private TroubleInTerroristGamemode _gamemode;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _gamemode = GetComponent<TroubleInTerroristGamemode>();
    }

    public void SendRoles()
    {
        if (!IsHost)
            return;
        
        ulong[] innocents = _gamemode.GetInnocents.ToArray();
        ulong[] traitors = _gamemode.GetTraitors.ToArray();
        
        ClientRpcParams clientRpcParamsInnocent = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = innocents
            }
        };
        
        ClientRpcParams clientRpcParamsTraitor = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = traitors
            }
        };

        GetRoleInnocentClientRPC(clientRpcParamsInnocent);
        GetRoleTraitorClientRPC(clientRpcParamsTraitor);
    }
    
    [ClientRpc]
    private void GetRoleInnocentClientRPC(ClientRpcParams clientRpcParams = default)
    {
        ReceiveRole.Invoke("Innocent");
    }
    
    [ClientRpc]
    private void GetRoleTraitorClientRPC(ClientRpcParams clientRpcParams = default)
    {
        ReceiveRole.Invoke("Traitor");
    }
}
