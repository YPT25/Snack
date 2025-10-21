using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DebugItemGenerator : NetworkBehaviour
{
    [SyncVar, SerializeField] GameObject m_itemPrefab;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 6 || other.gameObject.GetComponent<Player_Tanabe>() == null) { return; }
        //if (other.gameObject.layer == 6 || other.gameObject.GetComponent<Player_Tanabe>() == null) { return; }

        GameObject obj = Instantiate(m_itemPrefab);
        obj.transform.position = this.transform.position;
        NetworkServer.Spawn(obj);
    }
}
