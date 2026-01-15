using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpPoint_Ashuri : NetworkBehaviour
{
    [Header("ワープポイント先")]
    [Tooltip("ワープ先のtransform")]
    [SerializeField] private Transform _warpPoint;

    [Header("SE")]
    [Header("ボタンを押した音")]
    [SerializeField] public AudioClip sound1;

    [Tooltip("AudioSource")] private AudioSource audioSource;
    private void Start()
    {
        //Componentを取得
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // プレイヤーに当たったら
        if(other.CompareTag("Player"))
        {
            // 当たったオブジェクトをワープポイント先に移動させる
            other.gameObject.transform.position = _warpPoint.position;

            //角度を設定する
            other.gameObject.transform.rotation = _warpPoint.rotation;

            //音声を流す
            this.RpcPlaySE();
        }
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
