using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GearSync : PlayerNetworkComponent
{
    [SerializeField] private EquipmentController equipmentController;
    [SerializeField] private Transform weaponBone;

    private Dictionary<int, Item> items = new Dictionary<int, Item>();
    private void Start()
    {
        foreach (var weapon in weaponBone.GetComponents<Item>())
        {
            items.Add(weapon.Id, weapon);
            print(weapon.Id + " - " + weapon.name);
        }
        
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
        print(itemId);
        if (!items.TryGetValue(itemId, out var item))
            return;
        print("sync2");
        equipmentController.Equip(item);
    }
}
