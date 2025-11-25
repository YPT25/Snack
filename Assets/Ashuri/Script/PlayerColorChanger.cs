using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColorChanger : NetworkBehaviour
{
    [Header("マテリアルを変更したいプレイヤーのパーツ")]
    [Tooltip("自動で子の MeshRenderer を取得します")]
    private List<MeshRenderer> _parts = new List<MeshRenderer>();

    [Header("利用可能なマテリアルリスト")]
    [Tooltip("配列の順番とインデックスが一致します")]
    [SerializeField] private Material[] availableMaterials;

    // ★ この値がサーバー → 全クライアントへ同期される
    [SyncVar(hook = nameof(OnMaterialIndexChanged))]
    private int currentMaterialIndex = 0;

    // ----------------------------------------------------
    // オブジェクト生成時に MeshRenderer を回収
    // ----------------------------------------------------
    private void Awake()
    {
        // 子オブジェクトにある MeshRenderer をすべてリストへ追加
        _parts.AddRange(GetComponentsInChildren<MeshRenderer>());
    }

    // ----------------------------------------------------
    // Start時にStatePlayer_Ashuriから保存された色を復元
    // ----------------------------------------------------
    private void Start()
    {
        // シーン切り替え後も保存された色を取得して適用
        var stateManager = FindObjectOfType<StatePlayer_Ashuri>();
        if (stateManager != null && isServer)
        {
            // サーバー側で保存されたマテリアルを取得して適用
            currentMaterialIndex = stateManager.GetSavedMaterial(connectionToClient);
        }

        // 現在のマテリアルを適用
        ApplyMaterial(currentMaterialIndex);
    }

    // ----------------------------------------------------
    // クライアント → サーバー へ送るコマンド
    // ----------------------------------------------------
    [Command]
    public void CmdChangeMaterial(int materialIndex)
    {
        // 範囲チェック
        if (materialIndex < 0 || materialIndex >= availableMaterials.Length)
            return;

        // サーバーでマテリアルのインデックスを変更
        // → SyncVar が自動で全クライアントへ通知してくれる
        currentMaterialIndex = materialIndex;

        // StatePlayer_Ashuri にも保存
        var stateManager = FindObjectOfType<StatePlayer_Ashuri>();
        if (stateManager != null)
        {
            stateManager.SavePlayerMaterial(connectionToClient, materialIndex);
        }
    }

    // ----------------------------------------------------
    // ★ SyncVar の値が変わったときに全クライアントで呼ばれる処理
    // ----------------------------------------------------
    private void OnMaterialIndexChanged(int oldIndex, int newIndex)
    {
        // 新しいインデックスのマテリアルを適用
        ApplyMaterial(newIndex);
    }

    // ----------------------------------------------------
    // 全 MeshRenderer にマテリアルを適用
    // ----------------------------------------------------
    private void ApplyMaterial(int index)
    {
        // 範囲外なら何もしない
        if (index < 0 || index >= availableMaterials.Length)
            return;

        // 全パーツへ適用
        foreach (var part in _parts)
        {
            part.material = availableMaterials[index];
        }
    }
}
