using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �v���C���[�̒��j�����BNetworkIdentity��ێ����A
/// �N���C�A���g���́E�J�Z�b�g�ؑցE������S������B
/// </summary>
public class MPlayerBase : NetworkBehaviour
{
    [Header("���p�\�ȃ��W���[���i�J�Z�b�g�j�v���n�u")]
    [SerializeField] private List<GameObject> modulePrefabs = new();

    [Header("���W���[���z�u��i�L�������f���̐e�j")]
    [SerializeField] private Transform moduleRoot;

    [SyncVar(hook = nameof(OnModuleChanged))]
    private int currentModuleIndex = -1;

    private PlayerModule currentModule;

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        Debug.Log("[PlayerBase] ���L�����擾���܂����iLocalPlayer�j�B");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (currentModuleIndex >= 0)
            SpawnModule(currentModuleIndex);
    }

    /// <summary>
    /// ���W���[���ύX�v���i�N���C�A���g �� �T�[�o�[�j
    /// </summary>
    [Command]
    public void CmdChangeModule(int newIndex)
    {
        if (newIndex < 0 || newIndex >= modulePrefabs.Count) return;
        currentModuleIndex = newIndex;
    }

    /// <summary>
    /// SyncVar�ύX���i�T�[�o�[ �� �S�N���C�A���g�j
    /// </summary>
    private void OnModuleChanged(int oldIndex, int newIndex)
    {
        SpawnModule(newIndex);
    }

    /// <summary>
    /// ���W���[�������^�����ւ�
    /// </summary>
    private void SpawnModule(int index)
    {
        if (moduleRoot == null) moduleRoot = transform;

        // �����W���[�����폜
        if (currentModule != null)
        {
            Destroy(currentModule.gameObject);
            currentModule = null;
        }

        // �V���W���[���𐶐�
        var prefab = modulePrefabs[index];
        var instance = Instantiate(prefab, moduleRoot);
        currentModule = instance.GetComponent<PlayerModule>();
        currentModule.OnAttach(this);
        Debug.Log($"[PlayerBase] ���W���[���ؑ�: {prefab.name}");
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
            // ���S���A�đI���ł���悤�Ƀ��X�|�[���}�l�[�W���֒ʒm
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