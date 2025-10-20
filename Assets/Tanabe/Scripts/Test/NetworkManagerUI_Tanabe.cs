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

    // �z�X�g�{�^���������ɌĂ΂��
    public void OnHostButton()
    {
        m_networkManager.StartHost();
        canvas.gameObject.SetActive(false);
    }

    // �N���C�A���g�{�^���������ɌĂ΂��
    public void OnClientButton()
    {
        m_networkManager.networkAddress = "localhost"; // IP�w��
        m_networkManager.StartClient();
        canvas.gameObject.SetActive(false);
    }

    // �Z�[�o�[�{�^���������ɌĂ΂��
    public void OnServerButton()
    {
        m_networkManager.StartServer();
        canvas.gameObject.SetActive(false);
    }
}
