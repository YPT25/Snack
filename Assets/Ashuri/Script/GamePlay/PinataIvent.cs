using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class PinataIvent : NetworkBehaviour
{
    // ===============================
    // 生成するピニャータ
    // ===============================

    [Header("生成するピニャータ")]
    [Tooltip("生成するピニャータのプレハブ")]
    [SerializeField] private GameObject _pinata;

    // ===============================
    // ピニャータを生成する時間
    // ===============================

    [Header("ピニャータを生成する時間")]
    [Tooltip("ピニャータを生成するゲーム経過時間（秒）")]
    [SerializeField] private List<float> _iventTime = new List<float>();

    [Tooltip("1回のイベントで生成するピニャータの数")]
    [SerializeField] private int _pinataNum = 1;

    // ===============================
    // 生成するポジション
    // ===============================

    [Header("生成するポジション")]
    [Tooltip("ピニャータを生成する位置")]
    [SerializeField] private List<Transform> _iventPosition = new List<Transform>();

    // ===============================
    // ゲームマネージャー
    // ===============================

    [Header("ゲームマネージャー")]
    [Tooltip("ゲーム時間を管理しているGameManager")]
    [SerializeField] private GameManager gameManager;

    // ===============================
    // イベント表示用Image（単体）
    // ===============================

    [Header("ピニャータのイベントImage")]
    [Tooltip("画面に表示するImage（1つだけ）")]
    [SerializeField] private Image _eventImage;

    // ===============================
    // 表示するSpriteリスト
    // ===============================

    [Header("表示するSprite")]
    [Tooltip("順番に表示するSprite")]
    [SerializeField] private List<Sprite> _eventSprites = new List<Sprite>();

    // ===============================
    // Image演出設定
    // ===============================

    [Header("Image演出設定")]
    [Tooltip("画面外（上）の位置")]
    [SerializeField] private Vector2 _imageHidePos = new Vector2(0, 800);

    [Tooltip("画面内の表示位置")]
    [SerializeField] private Vector2 _imageShowPos = new Vector2(0, 300);

    [Tooltip("上下移動にかかる時間")]
    [SerializeField] private float _moveTime = 0.5f;

    [Tooltip("Sprite1枚あたりの表示時間")]
    [SerializeField] private float _spriteChangeTime = 1.5f;

    // ===============================
    // イベント実行済み判定
    // ===============================

    // 各イベント時間が実行済みかを管理する
    private List<bool> _isEventExecuted = new List<bool>();

    // ===============================
    // 初期化処理
    // ===============================

    void Start()
    {
        // イベント時間分の実行フラグを初期化する
        for (int i = 0; i < _iventTime.Count; i++)
        {
            _isEventExecuted.Add(false);
        }

        // Imageを非表示にする
        _eventImage.gameObject.SetActive(false);

        // Imageの位置を画面外に設定する
        _eventImage.rectTransform.anchoredPosition = _imageHidePos;
    }

    // ===============================
    // 毎フレーム処理
    // ===============================

    void Update()
    {
        // サーバー以外では処理を行わない
        if (!isServer)
        {
            return;
        }

        // GameManagerが取得できていない場合は処理しない
        if (gameManager == null)
        {
            return;
        }

        // 現在のゲーム経過時間を取得する
        float currentTime = gameManager.CurrentGameTime;

        // イベント時間を順番にチェックする
        for (int i = 0; i < _iventTime.Count; i++)
        {
            // 未実行かつ指定時間に到達していたら実行する
            if (!_isEventExecuted[i] && currentTime >= _iventTime[i])
            {
                // ピニャータを生成する
                SpawnPinata();

                // Image演出を全クライアントで再生する
                RpcPlayEventImage();

                // 実行済みにする
                _isEventExecuted[i] = true;
            }
        }
    }

    // ===============================
    // ピニャータ生成処理（位置は被らない）
    // ===============================

    [Server]
    private void SpawnPinata()
    {
        // 使用可能な生成位置をコピーする
        List<Transform> availablePositions = new List<Transform>(_iventPosition);

        // 実際に生成する数を決める
        int spawnCount = Mathf.Min(_pinataNum, availablePositions.Count);

        // 指定数分生成する
        for (int i = 0; i < spawnCount; i++)
        {
            // ランダムで生成位置を選ぶ
            int index = Random.Range(0, availablePositions.Count);

            // 使用する生成位置を取得する
            Transform spawnPoint = availablePositions[index];

            // 使用済みの位置を削除する
            availablePositions.RemoveAt(index);

            // ピニャータを生成する
            GameObject pinata = Instantiate(_pinata, spawnPoint.position, spawnPoint.rotation);

            // ネットワーク上にスポーンする
            NetworkServer.Spawn(pinata);
        }
    }

    // ===============================
    // Image演出（全クライアント）
    // ===============================

    [ClientRpc]
    private void RpcPlayEventImage()
    {
        // Image演出のコルーチンを開始する
        StartCoroutine(EventImageSequence());
    }

    // ===============================
    // Image演出シーケンス
    // ===============================

    private IEnumerator EventImageSequence()
    {
        // Imageを表示状態にする
        _eventImage.gameObject.SetActive(true);

        // 上から下へ移動させる
        yield return StartCoroutine(MoveImage(_imageHidePos, _imageShowPos));

        // Spriteを順番に切り替える
        for (int i = 0; i < _eventSprites.Count; i++)
        {
            // 表示するSpriteを設定する
            _eventImage.sprite = _eventSprites[i];

            // 表示時間分待機する
            yield return new WaitForSeconds(_spriteChangeTime);
        }

        // 下から上へ戻す
        yield return StartCoroutine(MoveImage(_imageShowPos, _imageHidePos));

        // Imageを非表示にする
        _eventImage.gameObject.SetActive(false);
    }

    // ===============================
    // Image移動処理
    // ===============================

    private IEnumerator MoveImage(Vector2 startPos, Vector2 endPos)
    {
        // 経過時間を初期化する
        float elapsed = 0f;

        // 指定時間まで補間移動する
        while (elapsed < _moveTime)
        {
            // 経過時間を加算する
            elapsed += Time.deltaTime;

            // Imageの位置を補間する
            _eventImage.rectTransform.anchoredPosition =
                Vector2.Lerp(startPos, endPos, elapsed / _moveTime);

            // 次のフレームまで待機する
            yield return null;
        }

        // 最終位置を確定させる
        _eventImage.rectTransform.anchoredPosition = endPos;
    }
}
