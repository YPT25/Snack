using Mirror;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ユーザーネーム入力と、入力後のプレイヤースポーンを制御するクラス
/// Host / Client 両対応
/// </summary>
public class UserNameManager : MonoBehaviour
{
    [Header("UI 参照")]
    [Tooltip("名前セットボタン")]
    [SerializeField] private Button NameSetButton;

    [Tooltip("名前入力欄")]
    [SerializeField] private TMP_InputField nameInput;

    [Tooltip("名前入力パネル")]
    [SerializeField] private GameObject namePanel;

    [Tooltip("ボタンのUI")]
    [SerializeField] private Canvas stringButton;

    // 入力されたプレイヤー名
    private string playerName;

    /// <summary>
    /// 初期化処理
    /// 名前セットボタンにクリックイベントを登録
    /// </summary>
    private void Start()
    {
        NameSetButton.onClick.AddListener(OnSetNameOnClick);
    }

    /// <summary>
    /// 名前セットボタン押下時の処理
    /// 名前チェック後、プレイヤーをSpawn
    /// </summary>
    private void OnSetNameOnClick()
    {
        // 入力欄の文字を取得
        playerName = nameInput.text;

        // 入力が空なら処理を終了
        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("名前が入力されていません。");
            return;
        }

        // 名前パネルを非表示
        namePanel.SetActive(false);
        NameSetButton.gameObject.SetActive(false);
        nameInput.gameObject.SetActive(false);
        stringButton.gameObject.SetActive(false);

        // ------------------------------
        // プレイヤーSpawn処理
        // ------------------------------

        // Hostの場合
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            // NetworkManager を AshuriNetworkManager にキャスト
            var customNM = NetworkManager.singleton as AshuriNetworkManager;
            if (customNM != null)
            {
                // Host用Spawn関数を呼ぶ
                customNM.SpawnLocalPlayer();
            }
        }
        // Clientの場合
        else if (NetworkClient.active)
        {
            // Client側でAddPlayerを呼ぶ
            NetworkClient.AddPlayer();
        }

        Debug.Log($"名前入力完了: {playerName}（プレイヤーをSpawnしました）");
    }

    /// <summary>
    /// 入力されたプレイヤー名を取得
    /// </summary>
    /// <returns>プレイヤー名</returns>
    public string GetPlayerName()
    {
        return playerName;
    }
}
