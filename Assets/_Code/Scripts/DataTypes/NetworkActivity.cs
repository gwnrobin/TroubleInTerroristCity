using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class NetworkActivity : Activity
{
    private bool owner;

    public NetworkActivity(bool owner)
    {
        this.owner = owner;

        if (owner)
        {
            AddStartListener(UpdateClientServerRpc);
        }
    }
    
    [ServerRpc]
    private void UpdateClientServerRpc()
    {
        //UpdateClientClientRpc();
        Debug.Log(owner);
    }
    
    [ClientRpc]
    private void UpdateClientClientRpc()
    {
        
    }
}
