using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button serverBtn;
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;
    [SerializeField] private TMP_InputField ip;
    [SerializeField] private UnityTransport transport;

    private string originText;
    private void Awake()
    {
        originText = ip.text;
        
        serverBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
            Cursor.lockState = CursorLockMode.Locked;
        });
        hostBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            Cursor.lockState = CursorLockMode.Locked;
        });
        clientBtn.onClick.AddListener(() =>
        {
            if (ip.text != originText)
            {
                transport.ConnectionData.Address = ip.text;
            }
            NetworkManager.Singleton.StartClient();
        });
    }
}
