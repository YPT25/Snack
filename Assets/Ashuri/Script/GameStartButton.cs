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
        // オブジェクトがトリガーであることを確認のコメントアウトを解除し、Collision用に修正
        Collider comp = GetComponent<Collider>();
        if (comp != null && comp.isTrigger) // ★変更点：isTriggerがONだと警告を出す
        {
            Debug.LogWarning("Collider on " + gameObject.name + " is set to Trigger. For collision detection, 'Is Trigger' should be OFF. Please disable 'Is Trigger'.");
        }
        // Rigidbodyがアタッチされているか確認（コリジョンには必須）
        if (GetComponent<Rigidbody>() == null)
        {
            Debug.LogError("Rigidbody is required for collision detection on " + gameObject.name + ". Please add a Rigidbody component.");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    // プレイヤーがこのオブジェクトに衝突したときに呼ばれる（コリジョン用）
    void OnCollisionEnter(Collision collision) // ★変更点：OnCollisionEnter に変更、引数を Collision 型に
    {
        // サーバー上でのみ処理を実行し、クライアントへ同期させる
        if (!isServer) return; // サーバーでなければ処理しない
        if (isTrigger) return;
        // ★変更点：衝突したオブジェクトのGameObjectを取得し、タグを確認
        if (collision.gameObject.CompareTag("Player")) // 触れたのがPlayerタグのオブジェクトか確認
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
            StartCoroutine(FadeManager.Instance.FadeOutAndLoadScene(nextSceneName));
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
    }
}