using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipPistol : NetworkPlayerComponent
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        StartCoroutine(LateStart());
    }

    private IEnumerator LateStart()
    {
        yield return new WaitForSeconds(1f);
        //Player.EquipItem.Try(Player.Inventory.GetContainerWithName("Pistol").Slots[0].Item, true);
    }
}
