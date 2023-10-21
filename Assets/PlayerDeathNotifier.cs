using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeathNotifier : PlayerNetworkComponent
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Player.Death.AddListener(() => TroubleInTerroristGamemode.Instance.PlayerDieServerRPC(OwnerClientId));
    }
}
