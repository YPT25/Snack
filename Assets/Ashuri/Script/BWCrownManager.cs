using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BWCrownManager : NetworkBehaviour
{
    // プレイヤーの Transform を保持する変数
    private Transform _playerPosition;

    // 王冠を落とすスピード（Inspector で変更可能）
    [Header("王冠を落とすスピード")]
    [Tooltip("リザルトの王冠を落とすスピード")]
    [SerializeField] private float _speedCrown = 1.0f;

    // 王冠が停止状態かどうか
    private bool _crownStop = false;

    void Start()
    {
        // 王冠の初期処理はプレイヤー登録後に動くため、ここでは特に何もしない
    }

    void Update()
    {
        // プレイヤー情報が登録されていなければ何もしない
        if (_playerPosition == null) return;

        // 王冠が停止していない場合の処理
        if (!_crownStop)
        {
            // 現在の王冠座標を取得
            Vector3 newPos = this.transform.position;

            // 王冠のX座標をプレイヤーに合わせる
            newPos.x = _playerPosition.position.x;

            // 王冠を落とす処理
            newPos.y -= _speedCrown * Time.deltaTime;

            // 王冠のZ座標をプレイヤーに合わせる
            newPos.z = _playerPosition.position.z;

            // 変更した座標を王冠へ反映
            this.transform.position = newPos;
        }
        else
        {
            // 王冠がプレイヤーにくっついている状態の処理
            this.transform.position = _playerPosition.position + new Vector3(0f,1.1f,0.0f);

            // 1つ上のコメント：プレイヤーのY方向の角度だけ取得する
            float y = _playerPosition.eulerAngles.y;

            // 1つ上のコメント：王冠の回転をYだけプレイヤーに合わせる
            this.transform.rotation = Quaternion.Euler(-90, y, 0);

        }
    }

    // ----------------------------------------------------
    // プレイヤー登録処理（プレイヤー側が呼び出す）
    // ----------------------------------------------------

    // プレイヤー位置をセットするメソッド
    public void SetPlayerPosition(Transform player)
    {
        // プレイヤーの Transform を保存
        _playerPosition = player;

        // プレイヤーが登録されたら位置を合わせる
        Vector3 newPos = this.transform.position;
        newPos.x = _playerPosition.position.x;
        newPos.y = 15.0f;
        newPos.z = _playerPosition.position.z;
        this.transform.position = newPos;
    }   

    // プレイヤー位置を取得するメソッド
    public Transform GetPlayerPosition()
    {
        return _playerPosition;
    }

    // ----------------------------------------------------
    // プレイヤーが触れたら王冠が停止する処理
    // ----------------------------------------------------
    private void OnTriggerEnter(Collider other)
    {
        // プレイヤーのタグと一致するか確認する
        if (other.CompareTag("Player"))
        {
            // 王冠を停止状態に変更
            _crownStop = true;
        }
    }
}
