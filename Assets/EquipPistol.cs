using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipPistol : PlayerNetworkComponent
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Player.EquipItem.Try(Player.Inventory.GetContainerWithName("Pistol").Slots[0].Item, true);
    }
}
