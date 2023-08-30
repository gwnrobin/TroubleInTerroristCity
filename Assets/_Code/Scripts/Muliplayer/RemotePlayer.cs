using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RemotePlayer : NetworkBehaviour
{
    [SerializeField] private Camera camera;
    [SerializeField] private List<MonoBehaviour> toDelete = new();

    private void Start()
    {
        if(!IsLocalPlayer)
        {
            foreach (MonoBehaviour component in toDelete)
            {
                Destroy(component);
            }
            camera.enabled = false;
        }
    }
}
