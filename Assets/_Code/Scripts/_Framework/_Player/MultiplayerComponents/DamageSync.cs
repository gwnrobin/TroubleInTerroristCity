using Unity.Netcode;
using UnityEngine;

public class DamageSync : NetworkPlayerComponent 
{
    private void Start()
    {
        if(!IsOwner)
            return;
        
        if (IsHost)
        {
            Player.DealDamage.AddListener(DamageSyncManager.Instance.SendDataToClient);
        }
        else if(IsClient)
        {
            Player.DealDamage.AddListener(DamageSyncManager.Instance.SendDataToServer);
        }
    }
}