using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BWCrownManager : NetworkBehaviour
{
    // プレイヤーの Transform を保持する変数
    private Transform _playerPosition;

    [Header("王冠を落とすスピード")]
    [Tooltip("リザルトの王冠を落とすスピード")]
    [SerializeField] private float _speedCrown = 1.0f;

    void Start()
    {
        // 現在のオブジェクト（王冠）の座標を取得する
        Vector3 newPos = this.transform.position;

        // 王冠のX座標をプレイヤーのX座標に合わせる
        newPos.x = _playerPosition.position.x;

        // 王冠のZ座標をプレイヤーのZ座標に合わせる
        newPos.z = _playerPosition.position.z;

        // 変更した座標を王冠に反映する
        this.transform.position = newPos;
    }

    void Update()
    {
        // 現在のオブジェクト（王冠）の座標を取得する
        Vector3 newPos = this.transform.position;

        // 王冠のX座標をプレイヤーのX座標に合わせる
        //newPos.x = _playerPosition.position.x;
        newPos.x = transform.position.x;

        // 王冠のY座標をゆっくりと降りていく
        newPos.y -= _speedCrown;

        // 王冠のZ座標をプレイヤーのZ座標に合わせる
        //newPos.z = _playerPosition.position.z;
        newPos.z = transform.position.z;

        // 変更した座標を王冠に反映する
        this.transform.position = newPos;
    }

    // ----------------------------------------------------
    // プレイヤーポジションのゲッター・セッター
    // ----------------------------------------------------

    // プレイヤー位置をセットするメソッド
    public void SetPlayerPosition(Transform player)
    {
        _playerPosition = player;
    }

    // プレイヤー位置を取得するメソッド
    public Transform GetPlayerPosition()
    {
        return _playerPosition;
    }
}
