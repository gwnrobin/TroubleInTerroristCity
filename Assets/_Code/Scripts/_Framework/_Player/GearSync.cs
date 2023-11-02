using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GearSync : NetworkPlayerComponent
{
    [SerializeField] private EquipmentController equipmentController;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if(!IsOwner)
            return;
            
        if (IsHost)
        {
            Player.EquippedItem.AddChangeListener((Item item) => SyncGearClientRpc(item != null ? item.Id : 0));
        }
        else if(IsClient)
        {
            Player.EquippedItem.AddChangeListener((Item item) => SyncGearServerRpc(item != null ? item.Id : 0));
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SyncGearServerRpc(int itemId)
    {
        SyncGearClientRpc(itemId);
    }
    
    [ClientRpc]
    private void SyncGearClientRpc(int itemId)
    {
        if (IsOwner)
            return;

        if (!ItemDatabase.TryGetItemById(itemId, out var item))
            return;

        if (item == null)
        {
            equipmentController.activeEHandler.GetEquipmentItem(0);
            return;
        }
        
        //equipmentController.Equip(new Item(item, 1));
        Player.EquipItem.Try(new Item(item, 1), true);
    }
}
