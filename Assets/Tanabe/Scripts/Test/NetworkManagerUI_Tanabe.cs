using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkManagerUI_Tanabe : MonoBehaviour
{
    private NetworkManager m_networkManager;
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject[] buttons;
    [SerializeField] private GameObject parameterText;

    private void Start()
    {
        m_networkManager = GetComponent<NetworkManager>();
        parameterText.SetActive(false);
    }

    // ホストボタン押下時に呼ばれる
    public void OnHostButton()
    {
        m_networkManager.StartHost();
        //canvas.gameObject.SetActive(false);
        this.PlayerStart();
    }

    // クライアントボタン押下時に呼ばれる
    public void OnClientButton()
    {
        m_networkManager.networkAddress = "localhost"; // IP指定
        m_networkManager.StartClient();
        //canvas.gameObject.SetActive(false);
        this.PlayerStart();
    }

    // セーバーボタン押下時に呼ばれる
    public void OnServerButton()
    {
        m_networkManager.StartServer();
        //canvas.gameObject.SetActive(false);
        this.PlayerStart();
    }

    private void PlayerStart()
    {
        for(int i = 0; i < buttons.Length; i++)
        {
            buttons[i].SetActive(false);
        }
        parameterText.SetActive(true);
    }
}
