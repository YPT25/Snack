using Mirror;
using Mirror.Discovery;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserNameManager : MonoBehaviour
{
    [Header("UI 参照")]
    [Tooltip("名前の変更するボタン")]
    [SerializeField] private Button NameSetButton;

    [Tooltip("ネットワークに戻るボタン")]
    [SerializeField] private Button NetworkModeButton;

    [Tooltip("名前の入力欄")]
    [SerializeField] private TMP_InputField nameInput;

    [Tooltip("背景")]
    [SerializeField] private GameObject namePanel;

    [Tooltip("ネットワークのキャンバス")]
    [SerializeField] private Canvas networkModeCanvas;
    // Start is called before the first frame update
    void Start()
    {
        // ------------------------------
        // ボタンのクリックイベント登録
        // ------------------------------
        NetworkModeButton.onClick.AddListener(OnNetworkClick);
        NameSetButton.onClick.AddListener(OnNetworkClick);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// ネットワークボタンが押されたとき
    /// </summary>
    private void OnNetworkClick()
    {
        //このCanvasを非表示
        this.gameObject.SetActive(false);
        //NetworkCanvasを表示
        networkModeCanvas.gameObject.SetActive(true);
    }

    private void OnSetNameOnClick()
    {

    }
}
