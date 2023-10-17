using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RemotePlayer : NetworkBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private AudioListener _audioListener;
    [SerializeField] private List<MonoBehaviour> toDelete = new();

    private void Start()
    {
        if(!IsLocalPlayer)
        {
            foreach (MonoBehaviour component in toDelete)
            {
                Destroy(component);
            }
            _camera.enabled = false;
            _audioListener.enabled = false;
        }
    }
}
