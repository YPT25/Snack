using Mirror;
using UnityEngine;

public class SprayEffectMaterial : NetworkBehaviour
{
    // ===============================
    // このオブジェクトの Particle System
    // ===============================
    private ParticleSystem particleSystem;

    // ===============================
    // 初期化処理
    // ===============================
    private void Awake()
    {
        // Particle System を取得
        particleSystem = GetComponent<ParticleSystem>();
    }

    // ===============================
    // サーバーから色変更を指示する
    // ===============================
    [Server]
    public void SetColorServer(int index)
    {
        // 全クライアントへ色変更を通知
        RpcSetColor(index);
    }

    // ===============================
    // クライアント側で色を変更する
    // ===============================
    [ClientRpc]
    private void RpcSetColor(int index)
    {
        // ParticleSystem の MainModule を取得
        var main = particleSystem.main;

        switch (index)
        {
            case 0:
                main.startColor = Color.red;
                break;
            case 1:
                main.startColor = Color.blue;
                break;
            case 2:
                main.startColor = Color.yellow;
                break;
            case 3:
                main.startColor = new Color(1f, 0.5f, 0f, 1f); // オレンジ
                break;
            case 4:
                main.startColor = new Color(0.5f, 0f, 0.5f, 1f); // 紫
                break;
            case 5:
                main.startColor = Color.green;
                break;
            case 6:
                main.startColor = new Color(0.4f, 0.8f, 1f, 1f); // 水色
                break;
            case 7:
                main.startColor = new Color(1f, 0.4f, 0.7f, 1f); // ピンク
                break;
            case 8:
                main.startColor = Color.black;
                break;
            case 9:
                main.startColor = Color.white;
                break;
        }
    }
}
