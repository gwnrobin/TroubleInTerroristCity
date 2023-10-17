using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetworkComponent : NetworkBehaviour
{
    public Player Player
    {
        get
        {
            if (!m_Player)
                m_Player = GetComponent<Player>();
            if (!m_Player)
                m_Player = GetComponentInParent<Player>();

            return m_Player;
        }
    }

    private Player m_Player;
}
