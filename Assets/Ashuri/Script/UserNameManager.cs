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
        // ------------------------------
        // 名前セットボタンに専用処理を登録
        // ------------------------------
        NameSetButton.onClick.AddListener(OnSetNameOnClick);
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
        // このCanvasを非表示
        this.gameObject.SetActive(false);

        // NetworkCanvasを表示
        networkModeCanvas.gameObject.SetActive(true);
    }

    // ------------------------------------------------
    // 名前セットボタンが押されたときの処理
    // ------------------------------------------------
    private void OnSetNameOnClick()
    {
        // 入力欄の文字を取得
        string enteredName = nameInput.text;

        // 入力が空なら処理を終了（警告ログを表示）
        if (string.IsNullOrEmpty(enteredName))
        {
            Debug.LogWarning("名前が入力されていません。");
            return;
        }

        // PlayerNameHolder に保存
        PlayerNameHolder.SetPlayerName(enteredName);

        // コンソールに保存確認を表示
        Debug.Log($"プレイヤー名を保存しました: {enteredName}");

        //// 名前パネルを非表示
        //namePanel.SetActive(false);

        //// ネットワーク画面を表示
        //networkModeCanvas.gameObject.SetActive(true);
    }
}
