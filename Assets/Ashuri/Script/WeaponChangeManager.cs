using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponChangeManager : NetworkBehaviour
{
    [Header("プレイヤーの武器番号")]
    [Tooltip("0:銃・1:ハンマー")]
    [SerializeField] private int weaponNumber;

    // ----------------------------------------------------
    // プレイヤーが触れたら変身処理を呼ぶ
    // ----------------------------------------------------
    private void OnTriggerEnter(Collider other)
    {
        // プレイヤーかチェック
        PlayerWeaponManager manager = other.GetComponent<PlayerWeaponManager>();
        if (manager == null) return;

        // ローカルプレイヤーのみ反応
        if (!manager.isLocalPlayer) return;

        Debug.Log("アイテムに触れた → プレイヤーの変更処理を呼び出す");

        // プレイヤーのメソッドを呼び出す
        manager.TryChangePlayer(weaponNumber);
    }
}
