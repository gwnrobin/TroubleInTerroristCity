using Unity.Netcode;
using UnityEngine;

public class DamageSyncManager : NetworkSingleton<DamageSyncManager>
{
    public void SendDataToClient(DamageInfo info, IDamageable damageable)
    {
        ulong networkObjectId = info.HitObject.GetComponent<Hitbox>().Entity.NetworkObjectId;
        
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[]{PlayerManager.Instance.GetPlayerIdByPlayerObject(networkObjectId)}
            }
        };
        
        SendHitToClientRpc(new NetworkDamageInfo(info.Delta, networkObjectId));
    }

    public void SendDataToServer(DamageInfo info, IDamageable damageable)
    {
        print(info.Delta);
        SendHitToServerRpc(new NetworkDamageInfo(info.Delta, info.HitObject.GetComponent<Hitbox>().Entity.NetworkObjectId));
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SendHitToServerRpc(NetworkDamageInfo info)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[]{PlayerManager.Instance.GetPlayerIdByPlayerObject(info.HitObjectId)}
            }
        };
        
        SendHitToClientRpc(info, clientRpcParams);
    }
    
    [ClientRpc]
    private void SendHitToClientRpc(NetworkDamageInfo info, ClientRpcParams clientRpcParams = default)
    {
        if (!TroubleInTerroristGamemode.Instance.gamemodeStarted)
            return;
        
        PlayerManager.Instance.GetPlayerDataByObjectId(info.HitObjectId)?.playerObject.ChangeHealth.Try(new DamageInfo(info.Delta, DamageType.Generic, Vector3.zero));
    }
}
