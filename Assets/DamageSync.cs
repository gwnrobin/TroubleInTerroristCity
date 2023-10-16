using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DamageSync : PlayerNetworkComponent 
{

    private void Start()
    {
        if(!IsOwner)
            return;
        
        if (IsHost)
        {
            Player.DealDamage.AddListener(SendDataToClient);
        }
        else if(IsClient)
        {
            Player.DealDamage.AddListener(SendDataToServer);
        }
    }

    private void SendDataToClient(DamageInfo info, IDamageable damageable)
    {
        SendHitToClientRpc(new NetworkDamageInfo(info.Delta, info.HitObject.GetComponent<Hitbox>().Entity.NetworkObjectId));
    }

    private void SendDataToServer(DamageInfo info, IDamageable damageable)
    {
        SendHitToServerRpc(new NetworkDamageInfo(info.Delta, info.HitObject.GetComponent<Hitbox>().Entity.NetworkObjectId));
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SendHitToServerRpc(NetworkDamageInfo info)
    {
        SendHitToClientRpc(info);
    }
    
    [ClientRpc]
    private void SendHitToClientRpc(NetworkDamageInfo info)
    {
        if (IsOwner)
            return;
        
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(info.HitObjectId, out NetworkObject playerNetworkObject))
        {
            Player player = playerNetworkObject.GetComponent<Player>();
            
            player.ChangeHealth.Try(new DamageInfo(info.Delta, DamageType.Generic, Vector3.zero));
            Debug.Log("deal damage to: " + info.HitObjectId + " amount: " + info.Delta, player);
        }
    }
}

public struct NetworkDamageInfo : INetworkSerializable
{
    /// <summary>
    /// Damage amount
    /// </summary>
    public float Delta;

    /// <summary> </summary>
    //public Entity Source;

    //public DamageType DamageType;

    public ulong HitObjectId;

    /// <summary> </summary>
    //public Vector3 HitPoint;

    /// <summary> </summary>
    //public Vector3 HitDirection;

    /// <summary> </summary>
    //public float HitImpulse;

    /// <summary> </summary>
    //public Vector3 HitNormal;


    public NetworkDamageInfo(float delta, ulong source)
    {
        this.Delta = delta;
        this.HitObjectId = source;
    }

    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Delta);
        serializer.SerializeValue(ref HitObjectId);
    }
}