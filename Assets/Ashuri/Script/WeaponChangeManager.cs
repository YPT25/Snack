using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponChangeManager : NetworkBehaviour
{
    [Header("プレイヤーの武器番号")]
    [Tooltip("0:銃・1:ハンマー")]
    [SerializeField] private int weaponNumber;

    [Header("SE")]
    [Header("ボタンを押した音")]
    [SerializeField] public AudioClip sound1;

    [Tooltip("AudioSource")] private AudioSource audioSource;
    private void Start()
    {
        //Componentを取得
        audioSource = GetComponent<AudioSource>();
    }

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

        //音声を流す
        this.RpcPlaySE();
    }

    // ===============================
    // サーバーから全クライアントへSE再生
    // ===============================
    [ClientRpc]
    private void RpcPlaySE()
    {
        // AudioSourceが存在しない場合は処理しない
        if (audioSource == null) return;

        // 効果音を再生
        audioSource.PlayOneShot(sound1);
    }
}
