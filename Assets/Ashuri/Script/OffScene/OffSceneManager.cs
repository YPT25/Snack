using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OffSceneManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;

        // マウスカーソルを表示
        Cursor.visible = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!EscQuit.Instance._isOptionOpen) return;
        if (Input.GetButtonDown("Jump")|| Input.GetMouseButtonDown(0))
        {
            SceneManager.LoadScene("TitleTest");
        }
    }
}
