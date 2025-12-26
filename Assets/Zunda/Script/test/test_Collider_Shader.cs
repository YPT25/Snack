using System.Collections;
using System.Collections.Generic;
using Telepathy;
using UnityEngine;

public class test_Collider_Shader : MonoBehaviour
{
    [SerializeField] Material material;
    //[SerializeField] Shader shader;
    //[SerializeField] Renderer m_Renderer;
    Color m_Color = Color.red;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnCollisionStay(Collision collision)
    {
        Vector3 playerPos = collision.transform.position;
        //m_Renderer.material.SetVector("_PlayerPos", playerPos);
        playerPos.z -= 0.5f;
        material.SetVector("_PlayerPos", playerPos);
        Debug.Log(playerPos);
    }

    void OnCollisionExit(Collision other)
    {
        Vector3 playerPos = other.transform.position;
        playerPos.y = 500;
        material.SetVector("_PlayerPos", playerPos);

    }

    // Update is called once per frame
    void Update()
    {

        //material.SetVector("_PlayerPos", rb.velocity);
    }
}
