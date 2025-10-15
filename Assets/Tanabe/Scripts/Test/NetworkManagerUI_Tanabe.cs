using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkManagerUI_Tanabe : MonoBehaviour
{
    private NetworkManager m_networkManager;
    [SerializeField] private Canvas canvas;

    private void Start()
    {
        m_networkManager = GetComponent<NetworkManager>();
    }

    // ホストボタン押下時に呼ばれる
    public void OnHostButton()
    {
        m_networkManager.StartHost();
        canvas.gameObject.SetActive(false);
    }

    // クライアントボタン押下時に呼ばれる
    public void OnClientButton()
    {
        m_networkManager.networkAddress = "localhost"; // IP指定
        m_networkManager.StartClient();
        canvas.gameObject.SetActive(false);
    }

    // セーバーボタン押下時に呼ばれる
    public void OnServerButton()
    {
        m_networkManager.StartServer();
        canvas.gameObject.SetActive(false);
    }
}
