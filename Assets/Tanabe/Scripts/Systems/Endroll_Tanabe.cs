using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class Endroll_Tanabe : MonoBehaviour
{
    [SerializeField] private RectTransform[] m_rectTransform;
    [SerializeField] private RectTransform m_readStoppedTransform;
    [SerializeField, Range(0f, 300f)] private float m_speed;

    private bool m_isPlaying = true;
    private bool m_isStopped = false;

    [SerializeField] private TMP_Text targetText;   // TextMeshProの参照
    [SerializeField] private string filePath;       // StreamingAssets内のファイル名とか

    void Start()
    {
        //LoadAndSetText();
    }

    private void LoadAndSetText()
    {
        // ファイルのフルパスを作る
        string fullPath = Path.Combine(Application.streamingAssetsPath, filePath);

        if (!File.Exists(fullPath))
        {
            Debug.LogError("ファイルが見つからんぞコラァ！ path: " + fullPath);
            return;
        }

        // テキストを読み込む
        string textData = File.ReadAllText(fullPath);

        // TextMeshPro にぶち込む
        targetText.text = textData;
    }

    // Update is called once per frame
    void Update()
    {
        float UpSpeed = 1f;
        if(Input.GetMouseButton(0) || /*Input.GetKey(KeyCode.Space) || */Input.GetButton("Jump"))
        {
            UpSpeed = 2f;
        }

        if(Input.GetKey(KeyCode.P) && Input.GetMouseButtonDown(0))
        {
            m_isPlaying = !m_isPlaying;
        }

        if(!m_isPlaying || m_isStopped) { return; }
        // 4715f
        for (int i = 0; i < m_rectTransform.Length; i++)
        {
            Vector3 position = m_rectTransform[i].localPosition;
            position.y += m_speed * UpSpeed * Time.deltaTime;
            m_rectTransform[i].localPosition = position;
        }

        if(m_readStoppedTransform.localPosition.y >= /*4715f*/1482f)
        {
            m_isStopped = true;
        }
    }

    public bool GetIsStopped()
    {
        return m_isStopped;
    }
}
