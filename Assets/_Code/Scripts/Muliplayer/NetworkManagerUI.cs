using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;
    [SerializeField] private TMP_InputField ip;
    [SerializeField] private UnityTransport transport;

    public UnityEvent JoinedServer;

    private string originText;
    private void Awake()
    {
        originText = ip.text;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        
        hostBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            JoinedServer?.Invoke();
        });
        clientBtn.onClick.AddListener(() =>
        {
            if (ip.text != originText)
            {
                transport.ConnectionData.Address = ip.text;
            }
            NetworkManager.Singleton.StartClient();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            JoinedServer?.Invoke();
        });
    }
}
