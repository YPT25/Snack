using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test_ch : MonoBehaviour
{
    [SerializeField] ParticleSystem ps;

    // Start is called before the first frame update
    void Start()
    {
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        //Sprite sprite = renderer.sprite;
        renderer.material.color = Color.blue;

        Debug.Log(renderer.material);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
