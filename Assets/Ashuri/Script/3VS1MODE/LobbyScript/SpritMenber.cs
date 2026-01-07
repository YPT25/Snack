using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class SpritMenber : NetworkBehaviour
{
    // ===============================
    // 参加状況を表示する SpriteRenderer 一覧
    // ===============================
    [Header("参加状況のSpriteRenderer")]
    [Tooltip("人数分並べて配置しているSpriteRenderer")]
    [SerializeField] private List<SpriteRenderer> _spriteSpriteRenderer = new List<SpriteRenderer>();

    // ===============================
    // 表示する画像
    // ===============================
    [Header("描画する画像")]
    [Tooltip("プレートに乗っているときに表示する画像")]
    [SerializeField] private Sprite _redPlayer;

    [Tooltip("プレートに乗っていないときに表示する画像")]
    [SerializeField] private Sprite _whitePlayer;

    // ===============================
    // 人数チェック用スクリプト
    // ===============================
    [Header("取得したい testPlayerMenberCheck をアタッチ")]
    [Tooltip("人数情報を取得したいオブジェクトを指定")]
    [SerializeField] private testPlayerMenberCheck _targetCheck;

    // ===============================
    // 現在プレートに触れている人数（同期）
    // ===============================
    [SyncVar(hook = nameof(OnTouchCountChanged))]
    private int touchCount = 0;

    // ===============================
    // 初期化処理
    // ===============================
    void Start()
    {
        // 画像が設定されているか確認
        if (_redPlayer == null || _whitePlayer == null)
        {
            Debug.LogError("Sprite が設定されていません");
        }

        // 初期状態ですべて白にする
        UpdateSpriteView(touchCount);
    }

    // ===============================
    // 毎フレーム処理
    // ===============================
    void Update()
    {
        // サーバーのみ人数を更新する
        if (!isServer) return;

        // プレートに触れている人数を取得
        int count = _targetCheck.GetTouchPlayerCount();

        // 人数が変わったときのみ同期変数を更新
        if (touchCount != count)
        {
            touchCount = count;
        }
    }

    // ===============================
    // SyncVar が変更されたときに呼ばれる処理
    // ===============================
    void OnTouchCountChanged(int oldValue, int newValue)
    {
        // 人数に応じて表示を更新
        UpdateSpriteView(newValue);
    }

    // ===============================
    // Sprite の表示を更新する処理
    // ===============================
    void UpdateSpriteView(int count)
    {
        // SpriteRenderer を順番に確認
        for (int i = 0; i < _spriteSpriteRenderer.Count; i++)
        {
            // 人数以内なら赤、それ以外は白
            if (i < count)
            {
                _spriteSpriteRenderer[i].sprite = _redPlayer;
            }
            else
            {
                _spriteSpriteRenderer[i].sprite = _whitePlayer;
            }
        }
    }
}
