using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPlayerDeathNotifier : NetworkPlayerComponent
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            Player.Death.AddListener(() => TroubleInTerroristGamemode.Instance.PlayerDie(OwnerClientId));
        }
    }
}
