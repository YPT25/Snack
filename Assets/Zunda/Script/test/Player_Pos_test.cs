using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Pos_test : MonoBehaviour
{
    Rigidbody rb;
    float speed = 3.0f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            rb.velocity = transform.forward * speed;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            rb.velocity = -transform.forward * speed;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            rb.velocity = transform.right * speed;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            rb.velocity = -transform.right * speed;
        }
        
    }

}
