using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーの中核部分。NetworkIdentityを保持し、
/// クライアント入力・カセット切替・同期を担当する。
/// </summary>
public class MPlayerBase : NetworkBehaviour
{
    [Header("利用可能なモジュール（カセット）プレハブ")]
    [SerializeField] private List<GameObject> modulePrefabs = new();

    [Header("モジュール配置先（キャラモデルの親）")]
    [SerializeField] private Transform moduleRoot;

    [SyncVar(hook = nameof(OnModuleChanged))]
    private int currentModuleIndex = -1;

    private PlayerModule currentModule;

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        Debug.Log("[PlayerBase] 所有権を取得しました（LocalPlayer）。");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (currentModuleIndex >= 0)
            SpawnModule(currentModuleIndex);
    }

    /// <summary>
    /// モジュール変更要求（クライアント → サーバー）
    /// </summary>
    [Command]
    public void CmdChangeModule(int newIndex)
    {
        if (newIndex < 0 || newIndex >= modulePrefabs.Count) return;
        currentModuleIndex = newIndex;
    }

    /// <summary>
    /// SyncVar変更時（サーバー → 全クライアント）
    /// </summary>
    private void OnModuleChanged(int oldIndex, int newIndex)
    {
        SpawnModule(newIndex);
    }

    /// <summary>
    /// モジュール生成／差し替え
    /// </summary>
    private void SpawnModule(int index)
    {
        if (moduleRoot == null) moduleRoot = transform;

        // 旧モジュールを削除
        if (currentModule != null)
        {
            Destroy(currentModule.gameObject);
            currentModule = null;
        }

        // 新モジュールを生成
        var prefab = modulePrefabs[index];
        var instance = Instantiate(prefab, moduleRoot);
        currentModule = instance.GetComponent<PlayerModule>();
        currentModule.OnAttach(this);
        Debug.Log($"[PlayerBase] モジュール切替: {prefab.name}");
    }

    private void Update()
    {
        if (!isLocalPlayer) return;
        currentModule?.HandleInput();
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;
        currentModule?.HandleMovement();
    }

    public void OnPlayerDeath()
    {
        if (isServer)
        {
            // 死亡時、再選択できるようにリスポーンマネージャへ通知
            RpcShowRespawnUI();
        }
    }

    [TargetRpc]
    private void RpcShowRespawnUI()
    {
        var rm = FindObjectOfType<RespawnManager>();
        if (rm != null)
            rm.ShowRespawnUI(this);
    }
}