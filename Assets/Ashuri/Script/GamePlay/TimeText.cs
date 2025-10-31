using UnityEngine; // Unity基本機能
using Mirror; // Mirrorネットワーク
using TMPro; // TextMeshPro用UI表示

public class TimeText : NetworkBehaviour
{
    [Header("UI参照場所")]
    [Tooltip("残り時間を表示するTextMeshProUGUIコンポーネント。")]
    [SerializeField]
    private TextMeshProUGUI timeDisplay;

    private GameManager gameManager; // GameManagerへの参照

    // ===============================
    // クライアント開始時処理
    // ===============================
    public override void OnStartClient()
    {
        base.OnStartClient();
        // クライアントが開始されたときにGameManagerへの参照を取得
        TryFindGameManager();
        // 初期表示を一度更新
        if (gameManager != null)
        {
            UpdateTimeDisplay(gameManager.remainingGameTime);
        }
        else
        {
            // GameManagerが見つからない場合は0で初期表示
            UpdateTimeDisplay(0);
        }
    }

    void Update()
    {
        // クライアントの場合のみUIを更新
        if (isClient)
        {
            // GameManagerがまだ取得できていない場合、毎フレーム探す
            if (gameManager == null)
            {
                TryFindGameManager();
            }

            // GameManagerが見つかれば残り時間を表示
            if (gameManager != null)
            {
                UpdateTimeDisplay(gameManager.remainingGameTime);
            }
        }
    }

    // ===============================
    // GameManagerを探すメソッド
    // ===============================
    private void TryFindGameManager()
    {
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            // Debug.LogWarning("TimeText: GameManager not found yet. Retrying next frame."); // デバッグ用
        }
    }

    // ===============================
    // 時間表示更新メソッド
    // ===============================
    void UpdateTimeDisplay(float timeToDisplay)
    {
        if (timeDisplay != null)
        {
            int minutes = Mathf.FloorToInt(timeToDisplay / 60);
            int seconds = Mathf.FloorToInt(timeToDisplay % 60);
            timeDisplay.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    // ===============================
    // スクリプト無効化時：GameManager参照クリア
    // ===============================
    void OnDisable()
    {
        gameManager = null;
    }
}
