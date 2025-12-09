using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class ParticipantsNumberScript : NetworkBehaviour
{
    public enum Menber
    {
        None,
        First,
        Third
    }

    // 人数のモードを取得する
    public Menber _menber = Menber.None;

    [Header("取得したい testPlayerMenberCheck をアタッチ")]
    [Tooltip("人数情報を取得したいオブジェクトにある testPlayerMenberCheck を指定")]
    [SerializeField] private testPlayerMenberCheck _targetCheck;

    [Header("書き換える TextMeshPro をアタッチ")]
    [Tooltip("人数情報を表示する3D TextMeshProを指定")]
    public TextMeshPro targetTMP;

    // 上の処理：サーバーから全クライアントへ同期させる人数表示用文字列
    [SyncVar(hook = nameof(OnTextChanged))]
    private string syncedText = "0 / 3";

    //参加がいいならtrue,だめならfalse
    private bool _gameStart = false;

    //最大人数
    int maxMenber = 0;

    void Start()
    {
        // 上の処理：初期表示を反映（SyncVar値がクライアントにも自動反映される）
        if (targetTMP != null)
        {
            targetTMP.text = syncedText;
        }
    }


    void Update()
    {
        // 上の処理：人数をチェックして同期文字列を更新するのはサーバーだけ
        if (isServer)
        {
            // 上の処理：触れている人数を取得
            int count = _targetCheck.GetTouchPlayerCount();

            
            //モードによって最大人数の変更
            if(_menber == Menber.First)
            {
                maxMenber = 1;
            }

            if(_menber == Menber.Third)
            {
                maxMenber = NetworkManager.singleton.numPlayers - 1;
            }

            if(_menber == Menber.None)
            {
                maxMenber = 9999;
            }


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
}
