using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkPickup : NetworkBehaviour
{
    private EquipmentPickup _equipmentPickup;
    private NetworkObject _networkObject;
    private void Start()
    {
        _equipmentPickup = GetComponent<EquipmentPickup>();
        _networkObject = GetComponent<NetworkObject>();
        
        _equipmentPickup.PickedUpEquipment.AddListener(DespawnServerRpc);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void DespawnServerRpc()
    {
        _networkObject.Despawn(false);
    }
}
