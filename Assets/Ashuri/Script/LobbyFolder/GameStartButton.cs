using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror; // Mirrorの機能を使用するために必要

public class GameStartButton : NetworkBehaviour // NetworkBehaviourを継承
{
    public string nextSceneName = "YourNextSceneName"; // 遷移先のシーン名

    //シーン遷移するか
    private bool isTrigger = false;

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {

    }

    // プレイヤーがこのオブジェクトに衝突したときに呼ばれる（コリジョン用）
    private void OnTriggerEnter(Collider other)
    {
        // サーバー上でのみ処理を実行し、クライアントへ同期させる
        if (!isServer) return; // サーバーでなければ処理しない
        if (isTrigger) return;
        // ★変更点：衝突したオブジェクトのGameObjectを取得し、タグを確認
        if (other.gameObject.CompareTag("Player")) // 触れたのがPlayerタグのオブジェクトか確認
        {
            Debug.Log("Player collided with the button! Starting fade and scene transition.");
            // 全てのクライアントにフェードアウトとシーン遷移を指示する
            RpcRequestSceneChange();
            //トリガーを発動させる
            isTrigger = true;
        }
    }

    // クライアントでフェードアウトとシーン遷移を開始するRPC
    [ClientRpc]
    void RpcRequestSceneChange()
    {
        if (FadeManager.Instance != null)
        {
            // フェードアウトとシーン遷移を開始
            // コルーチンはモノビヘイビアからしか実行できないため、FadeManagerのInstanceから呼び出す
            //StartCoroutine(FadeManager.Instance.FadeOutAndLoadScene(nextSceneName));
        }
        else
        {
            Debug.LogError("FadeManager.Instance not found! Make sure FadeManager is on an active Canvas and has been initialized.");
            // フェードマネージャーが見つからない場合でも、シーン遷移だけは試みる（フェードなし）
            if (isServer)
            {
                NetworkManager.singleton.ServerChangeScene(nextSceneName);
            }
        }
        // プレイヤー番号をリセット
        ((AshuriNetworkManager)NetworkManager.singleton).PlayerNumberReset();

    }
}