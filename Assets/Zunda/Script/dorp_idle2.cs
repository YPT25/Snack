using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dorp_idle2 : MonoBehaviour
{

    public float WaveSpeed = 1.0f;
    public float WaveHeight = 0.1f;

    float baseY;

    // Start is called before the first frame update
    void Start()
    {
        baseY = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        float newY = baseY + Mathf.Sin(Time.time * WaveSpeed) * WaveHeight;

        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
