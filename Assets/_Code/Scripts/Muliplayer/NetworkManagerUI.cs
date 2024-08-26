using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private UnityTransport transport;

    private string originText;
    
    public void Host()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void Client()
    {
        //transport.ConnectionData.Address = "217.62.31.230";
        transport.ConnectionData.Address = "192.168.178.180";
        NetworkManager.Singleton.StartClient();
        
        print("Client try connect");
    }
}
