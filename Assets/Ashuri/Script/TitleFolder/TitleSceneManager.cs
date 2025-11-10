using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSceneManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //左クリックを受け付ける
        if (Input.GetMouseButtonDown(0))
        {
            SceneManager.LoadScene("LobbyScene");
        }
    }
}
