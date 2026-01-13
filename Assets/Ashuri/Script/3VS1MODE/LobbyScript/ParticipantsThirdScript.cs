using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ParticipantsThirdScript : NetworkBehaviour
{
    [Header("取得したい testPlayerMenberCheck をアタッチ")]
    [Tooltip("人数情報を取得したいオブジェクトにある testPlayerMenberCheck を指定")]
    [SerializeField] private testPlayerMenberCheck _targetCheck;

    [Header("書き換える TextMeshPro をアタッチ")]
    [Tooltip("人数情報を表示する3D TextMeshProを指定")]
    public TextMeshPro targetTMP;

    // 上の処理：サーバーから全クライアントへ同期させる人数表示用文字列
    [SyncVar(hook = nameof(OnTextChanged))]
    private string syncedText = "0 / 3";

    // 自分（First or Third）のゲーム開始条件
    private bool _gameStart = false;

    //最大人数
    int maxMenber = 0;

    // ーーー 処理を遅らせるフラグ ーーー
    private bool _isReady = false;

    void Start()
    {
        // 上の処理：初期表示を反映（SyncVar値がクライアントにも自動反映される）
        if (targetTMP != null)
        {
            targetTMP.text = syncedText;
        }

        // ● 起動直後は numPlayers が 0 のため、少し待つ
        StartCoroutine(WaitFrames());
    }

    private IEnumerator WaitFrames()
    {
        // ● 10フレーム待つ（必要なら変更OK）
        for (int i = 0; i < 10; i++)
        {
            yield return null;
        }

        _isReady = true;
    }


    void Update()
    {
        // ーーー numPlayers が安定するまで待つ ーーー
        if (!_isReady) return;

        // 上の処理：人数をチェックして同期文字列を更新するのはサーバーだけ
        if (isServer)
        {
            // 上の処理：触れている人数を取得
            int count = _targetCheck.GetTouchPlayerCount();

            // 参加可能な人数を取得
            maxMenber = 2;

            // ゲーム開始条件（残り全員が揃ったら）
            _gameStart = (count == maxMenber);

            // 上の処理：同期する文字列を作成
            syncedText = $"{count} / {maxMenber}";
        }
    }


    // 上の処理：SyncVar の値が変わったときにクライアント側で呼ばれる
    private void OnTextChanged(string oldValue, string newValue)
    {
        // 上の処理：TextMeshPro が設定されていればテキストを更新
        if (targetTMP != null)
        {
            targetTMP.text = newValue;
        }
    }

    // ーーー どちらのモードでも共通で使える ーーー
    // 自分のモードのゲーム開始判定を返す
    public bool GameStart()
    {
        return _gameStart;
    }
}
