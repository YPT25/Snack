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
    // イベントImage
    // ===============================

    [Header("ピニャータのイベントCanvas")]
    [Tooltip("ピニャータの出現する際に表示するImage")]
    [SerializeField] private Image _pinataImage;

    // ===============================
    // Image演出設定
    // ===============================

    [Header("Image演出設定")]
    [Tooltip("Imageが画面外にある位置")]
    [SerializeField] private Vector2 _imageHidePos = new Vector2(0, 800);

    [Tooltip("Imageが表示される位置")]
    [SerializeField] private Vector2 _imageShowPos = new Vector2(0, 300);

    [Tooltip("Imageの移動時間")]
    [SerializeField] private float _moveTime = 0.5f;

    [Tooltip("表示しておく時間")]
    [SerializeField] private float _stayTime = 2f;

    // ===============================
    // イベント実行済み判定
    // ===============================

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

        // Imageを最初は画面外に移動させる
        _pinataImage.rectTransform.anchoredPosition = _imageHidePos;
    }

    // ===============================
    // 毎フレーム処理
    // ===============================

    void Update()
    {
        // サーバー以外では処理しない
        if (!isServer)
        {
            return;
        }

        // GameManagerが無い場合は処理しない
        if (gameManager == null)
        {
            return;
        }

        // 現在のゲーム経過時間を取得する
        float currentTime = gameManager.CurrentGameTime;

        // イベント時間を順番にチェックする
        for (int i = 0; i < _iventTime.Count; i++)
        {
            // 未実行かつ指定時間を超えたら実行する
            if (!_isEventExecuted[i] && currentTime >= _iventTime[i])
            {
                // ピニャータを生成する
                SpawnPinata();

                // Image演出を全クライアントに通知する
                RpcPlayPinataImage();

                // 実行済みにする
                _isEventExecuted[i] = true;
            }
        }
    }

    // ===============================
    // ピニャータ生成処理（被らない）
    // ===============================

    [Server]
    private void SpawnPinata()
    {
        // 生成候補のポジションをコピーする
        List<Transform> availablePositions = new List<Transform>(_iventPosition);

        // 実際に生成できる数を決める
        int spawnCount = Mathf.Min(_pinataNum, availablePositions.Count);

        // 指定数分ピニャータを生成する
        for (int i = 0; i < spawnCount; i++)
        {
            // ランダムでインデックスを選ぶ
            int index = Random.Range(0, availablePositions.Count);

            // 使用する生成位置を取得する
            Transform spawnPoint = availablePositions[index];

            // 使用済みなのでリストから削除する
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
    private void RpcPlayPinataImage()
    {
        // Image演出のコルーチンを開始する
        StartCoroutine(PinataImageAnimation());
    }

    // ===============================
    // Imageの上下アニメーション
    // ===============================

    private IEnumerator PinataImageAnimation()
    {
        // 上から下へ移動させる
        yield return StartCoroutine(MoveImage(_imageHidePos, _imageShowPos));

        // 表示状態で待機する
        yield return new WaitForSeconds(_stayTime);

        // 下から上へ戻す
        yield return StartCoroutine(MoveImage(_imageShowPos, _imageHidePos));
    }

    // ===============================
    // Image移動処理
    // ===============================

    private IEnumerator MoveImage(Vector2 startPos, Vector2 endPos)
    {
        // 経過時間を初期化する
        float elapsed = 0f;

        // 指定時間まで繰り返す
        while (elapsed < _moveTime)
        {
            // 経過時間を加算する
            elapsed += Time.deltaTime;

            // Imageの位置を補間して移動させる
            _pinataImage.rectTransform.anchoredPosition =
                Vector2.Lerp(startPos, endPos, elapsed / _moveTime);

            // 次のフレームまで待機する
            yield return null;
        }

        // 最終位置を確定させる
        _pinataImage.rectTransform.anchoredPosition = endPos;
    }
}
