using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderGraphTest_Tanabe : MonoBehaviour
{
    [SerializeField] Texture m_texture2D;

    // Start is called before the first frame update
    void Start()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material.SetTexture("_Texture2D", m_texture2D);
    }
}
