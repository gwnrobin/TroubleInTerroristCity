using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Multiplayer.Playmode;
using Unity.Netcode;
using UnityEngine;

public class InstantGameStarter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var mppmTag = CurrentPlayer.ReadOnlyTags();

        if (mppmTag.Contains("Client"))
        {
            StartCoroutine(StartClient());
        }
        else
        {
            NetworkManager.Singleton.StartHost();
        }
    }
    
    private IEnumerator StartClient()
    {
        yield return new WaitForSeconds(5);

        print("startClient");
        NetworkManager.Singleton.StartClient();
    }
}

