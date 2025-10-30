using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartHost_Y : MonoBehaviour
{
    public Canvas canvas;
    private NetworkManager m_netManager;

    // Start is called before the first frame update
    void Start()
    {
        m_netManager = GetComponent<NetworkManager>();
    }

    public void HostButton()
    {
        m_netManager.StartHost();
        canvas.gameObject.SetActive(false);
    }

    public void ClientButton()
    {
        m_netManager.networkAddress = "localhost"; // IPŽw’è
        m_netManager.StartClient();
        canvas.gameObject.SetActive(false);
    }


}
