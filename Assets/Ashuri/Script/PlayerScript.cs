using UnityEngine; // Unityエンジン機能
using Mirror; // Mirrorネットワーキング機能
using TMPro; // TextMeshProを使用するために追加

// ネットワーク対応のプレイヤームーブメント
public class PlayerScript : NetworkBehaviour
{
    [Header("UIの参照場所")] // UI参照のセクションヘッダー
    [Tooltip("プレイヤーの頭上に表示される名前のTextMeshProコンポーネント。")] // playerNameTextのツールチップ
    public TextMeshPro playerNameText; // TextMeshProに型を変更
    [Tooltip("プレイヤーの頭上に表示される情報UIのルートGameObject。")] // floatingInfoのツールチップ
    public GameObject floatingInfo; // 頭上の情報UI

    private Material playerMaterialClone; // プレイヤーマテリアル

    [Header("ネットワークで同期するプレイヤー情報")] // ネットワーク同期されるプレイヤーデータのセクションヘッダー
    [Tooltip("プレイヤーの名前。サーバーからクライアントへ同期されます。")] // playerNameのツールチップ
    [SyncVar(hook = nameof(OnNameChanged))] // 名前変更を同期
    public string playerName;

    [Tooltip("プレイヤーの色。サーバーからクライアントへ同期されます。")] // playerColorのツールチップ
    [SyncVar(hook = nameof(OncolorChanged))] // 色変更を同期
    public Color playerColor = Color.white;

    // 名前変更時に呼ばれる
    void OnNameChanged(string _oldName, string _newName)
    {
        if (playerNameText != null)
        {
            playerNameText.text = _newName;
        }
    }

    // 色変更時に呼ばれる
    void OncolorChanged(Color _oldColor, Color _NewColor)
    {
        if (playerNameText != null)
        {
            playerNameText.color = _NewColor; // 名前の色を設定
        }

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            // マテリアルをクローンして色を変更し、元のマテリアルを直接変更しないようにする
            // これにより、Prefabのマテリアルが変更されるのを防ぐ
            playerMaterialClone = new Material(renderer.material);
            playerMaterialClone.color = _NewColor; // プレイヤーの色を設定
            renderer.material = playerMaterialClone;
        }
    }

    // ローカルプレイヤー初期設定
    public override void OnStartLocalPlayer()
    {
        // カメラ設定
        if (Camera.main != null)
        {
            Camera.main.transform.SetParent(transform); // カメラをプレイヤーの子に
            Camera.main.transform.localPosition = new Vector3(0, 3, -10); // カメラ位置調整
            Camera.main.transform.localRotation = Quaternion.Euler(15, 0, 0); // カメラの向きを少し下向きに調整する場合
        }

        // ローカルプレイヤーの場合、頭上の情報UIを非表示にする
        if (floatingInfo != null)
        {
            floatingInfo.SetActive(false); // ★ここを追加★
        }

        // ランダムな名前と色を生成
        string name = "Player" + Random.Range(100, 999);
        Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));

        // コマンドはisLocalPlayerである場合にのみ呼べる
        // CmdSetupPlayerはローカルプレイヤーがサーバーに設定を要求するためのものなので、このままでOK
        CmdSetupPlayer(name, color);
    }

    // サーバーで実行されるコマンド
    [Command]
    public void CmdSetupPlayer(string _name, Color _color)
    {
        // サーバー上でSyncVarの値を設定
        playerName = _name;
        playerColor = _color;
    }

    // 毎フレーム更新
    void Update()
    {
        // ローカルプレイヤーの場合のみ、入力処理を行う
        if (isLocalPlayer) // !isLocalPlayer ではなく isLocalPlayer
        {
            float moveX = Input.GetAxis("Horizontal") * Time.deltaTime * 110.0f; // 左右回転入力
            float moveY = Input.GetAxis("Vertical") * Time.deltaTime * 4f; // 前後移動入力

            transform.Rotate(0, moveX, 0); // Y軸回転
            transform.Translate(0, 0, moveY); // Z軸移動
        }
        else // 他のプレイヤーの場合、UIをカメラの方に向かせる
        {
            if (floatingInfo != null && Camera.main != null)
            {
                // カメラの方向を向かせることで、常に文字がプレイヤーに見えるようにする
                floatingInfo.transform.LookAt(Camera.main.transform);
                // しかし、LookAtはY軸を反転させてしまうことがあるので、より正確には以下のようにする
                floatingInfo.transform.forward = -Camera.main.transform.forward; // カメラとは逆方向を向かせる
            }
        }
    }
}