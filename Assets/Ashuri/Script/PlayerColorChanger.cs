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
        // すべての MeshRenderer を取得
        var renderers = GetComponentsInChildren<MeshRenderer>();

        // 1つ上にコメント：プレイヤーのパーツだけを追加する（武器は除外）
        foreach (var r in renderers)
        {
            // 武器を除外する（例：WEAPON に "Weapon" タグを付けておく）
            if (r.gameObject.layer == LayerMask.NameToLayer("Hammer") || r.gameObject.layer == LayerMask.NameToLayer("Gun"))
                continue;

            if (r.CompareTag("Eye"))
                continue;

            _parts.Add(r);
        }
    }

    // ----------------------------------------------------
    // Start時にStatePlayer_Ashuriから保存された色を復元
    // ----------------------------------------------------
    private void Start()
    {
        // サーバーのみ保存された色を復元
        var stateManager = FindObjectOfType<StatePlayer_Ashuri>();
        if (stateManager != null && isServer)
        {
            int saved = stateManager.GetSavedMaterial(connectionToClient);

            // -1 なら保存なし → 元の色のまま
            if (saved >= 0)
            {
                currentMaterialIndex = saved;
                ApplyMaterial(currentMaterialIndex);
            }
        }
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
        // ↑ インデックスが間違っていたら処理しない
        if (index < 0 || index >= availableMaterials.Length)
            return;

        // ↑ すべてのパーツに対してマテリアルを設定する
        foreach (var part in _parts)
        {
            // ★ material を使うと灰色化するので使用禁止
            // part.material = availableMaterials[index];

            // ★ 必ず sharedMaterial を使う
            part.sharedMaterial = availableMaterials[index];
        }
    }

}
