using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpritMenber : NetworkBehaviour
{
    [Header("参加状況のSpriteRenderer")]
    [Tooltip("配置しているSpriteRenderer")]
    [SerializeField] private List<SpriteRenderer> _spriteSpriteRenderer = new List<SpriteRenderer>();

    [Header("描画する画像")]
    [Tooltip("プレートに乗っているときに表示する画像")]
    [SerializeField] private Sprite _redPlayer;

    [Tooltip("プレートに乗っていないときに表示する画像")]
    [SerializeField] private Sprite _whitePlayer;

    [Header("取得したい testPlayerMenberCheck をアタッチ")]
    [Tooltip("人数情報を取得したいオブジェクトにある testPlayerMenberCheck を指定")]
    [SerializeField] private testPlayerMenberCheck _targetCheck;

    //最大人数
    int maxMenber = 0;

    // Start is called before the first frame update
    void Start()
    {
        if(_redPlayer || _whitePlayer)
        {
            Debug.LogError("画像が入っていません");
        }
    }
    
    void Update()
    {
        // 上の処理：人数をチェックして同期文字列を更新するのはサーバーだけ
        if (isServer)
        {
            // 上の処理：触れている人数を取得
            int count = _targetCheck.GetTouchPlayerCount();

            // 参加可能な人数を取得
            maxMenber = NetworkManager.singleton.numPlayers - 1;
        }
    }
}
