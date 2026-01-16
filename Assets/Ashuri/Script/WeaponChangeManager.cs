using Mirror;
using UnityEngine;

public class WeaponChangeManager : NetworkBehaviour
{
    // ===============================
    // プレイヤーの武器番号
    // ===============================
    [Header("プレイヤーの武器番号")]
    [Tooltip("0:銃・1:ハンマー")]
    [SerializeField] private int weaponNumber;

    // ===============================
    // 効果音
    // ===============================
    [Header("SE")]
    [Tooltip("ボタンを押した音")]
    [SerializeField] private AudioClip sound1;

    // ===============================
    // AudioSource
    // ===============================
    private AudioSource audioSource;

    private void Start()
    {
        // AudioSourceコンポーネントを取得
        audioSource = GetComponent<AudioSource>();
    }

    // ----------------------------------------------------
    // プレイヤーが触れたときの処理（クライアント側）
    // ----------------------------------------------------
    private void OnTriggerEnter(Collider other)
    {
        // PlayerWeaponManagerを取得
        PlayerWeaponManager manager = other.GetComponent<PlayerWeaponManager>();
        if (manager == null) return;

        // ローカルプレイヤーのみ処理
        if (!manager.isLocalPlayer) return;

        // プレイヤーの武器変更処理
        manager.TryChangePlayer(weaponNumber);

        // サーバーへSE再生要求を送る
        CmdRequestPlaySE();
    }

    // ===============================
    // クライアントからサーバーへ通知
    // ===============================
    [Command]
    private void CmdRequestPlaySE()
    {
        // サーバーから全クライアントへSE再生
        RpcPlaySE();
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
