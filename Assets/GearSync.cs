using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GearSync : PlayerNetworkComponent
{
    [SerializeField] private EquipmentController equipmentController;
    private void Start()
    {
        if(!IsOwner)
            return;
            
        if (IsHost)
        {
            Player.EquippedItem.AddChangeListener((Item item) => SyncGearClientRpc(item.Id));
        }
        else if(IsClient)
        {
            Player.EquippedItem.AddChangeListener((Item item) => SyncGearServerRpc(item.Id));
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

        //equipmentController.Equip(new Item(item, 1));
        Player.EquipItem.Try(new Item(item, 1), true);
    }
}
