using Mirror;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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

        // Host / Client どちらの場合でも Mirror 標準の AddPlayer を使用
        if (NetworkClient.active)
        {
            // すでにプレイヤーが存在しない場合のみ AddPlayer を実行
            if (NetworkClient.localPlayer == null)
            {
                NetworkClient.AddPlayer();
                Debug.Log("AddPlayer() を呼び出しました（Host/Client 共通処理）");

                // 🔹 プレイヤー生成完了を待ってから名前を設定するコルーチンを開始
                StartCoroutine(SetPlayerNameToPlayer());
            }
            else
            {
                Debug.LogWarning("すでにローカルプレイヤーが存在しています。AddPlayer() は呼び出されません。");
            }
        }
        else
        {
            Debug.LogError("NetworkClient がアクティブではありません。接続状態を確認してください。");
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

    // ----------------------------------------------
    // プレイヤー生成後に名前をセットする処理
    // ----------------------------------------------
    private IEnumerator SetPlayerNameToPlayer()
    {
        // 生成完了を待つ
        yield return new WaitUntil(() => NetworkClient.localPlayer != null);

        // ローカルプレイヤーを取得
        var player = NetworkClient.localPlayer.gameObject;

        // プレイヤー側のスクリプト（例: Player_Tanabe）を取得
        var playerScript = player.GetComponent<Player_Tanabe>();

        // スクリプトが存在する場合のみ名前を設定
        if (playerScript != null)
        {
            playerScript.SetPlayerName(playerName);
            Debug.Log($"プレイヤー名を設定しました: {playerName}");
        }
        else
        {
            Debug.LogWarning("Player_Tanabe がプレイヤーに見つかりませんでした。");
        }
    }

}
