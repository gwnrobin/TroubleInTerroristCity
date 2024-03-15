using Unity.Netcode;

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
        Delta = delta;
        HitObjectId = source;
    }

    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Delta);
        serializer.SerializeValue(ref HitObjectId);
    }
}