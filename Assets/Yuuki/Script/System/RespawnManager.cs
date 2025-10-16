using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RespawnManager : MonoBehaviour
{
    // TODO: �����I�Ɂu��������NPC�̎�ށv�Ɠ����L���������I���\�ɂ���
    //       FindObjectsOfType<NPCBase>() �Ń��X�g�����Ĕ��肷��\��

    [Header("���X�|�[���ݒ�")]
    [SerializeField] private List<GameObject> playerPrefabs; // ���X�|�[�����i�v���C���[�̃v���n�u�j
    [SerializeField] private Transform respawnPoint;          // ���X�|�[���ʒu
    [SerializeField] private GameObject respawnUI;            // ���X�|�[��UI�iCanvas�z���̐e�j
    [SerializeField] private Button buttonPrefab;             // �I���{�^���̃v���n�u�i���[�g��Button�j

    private GameObject currentPlayer; // ���݂̃v���C���[
    private bool isWaitingForSelection = false;

    private void Start()
    {
        // ����N������UI���\��
        if (respawnUI != null)
            respawnUI.SetActive(false);

        // �����v���C���[�����i���ɍŏ��̃L�����j
        SpawnPlayer(0);
    }

    /// <summary>
    /// �v���C���[���S���ɌĂяo�����iMPlayerBase����j
    /// </summary>
    public void OnPlayerDeath()
    {
        Debug.Log("�v���C���[���S�B�L�����I����ʂ�\���B");
        ShowRespawnUI();
    }

    /// <summary>
    /// �L�����I��UI��\��
    /// </summary>
    private void ShowRespawnUI()
    {
        if (respawnUI == null || buttonPrefab == null) return;

        respawnUI.SetActive(true);
        isWaitingForSelection = true;

        // �����{�^�����N���A
        foreach (Transform child in respawnUI.transform)
            Destroy(child.gameObject);

        // ���݃X�e�[�W�ɑ��݂���NPC�̎�ނ��擾
        var existingTypes = GetExistingNPCTypes();

        // �{�^���𐶐�
        for (int i = 0; i < playerPrefabs.Count; i++)
        {
            string playerName = playerPrefabs[i].name;
            if (!existingTypes.Contains(playerName))
            {
                // ���݂��Ȃ���ނ̓X�L�b�v
                continue;
            }

            var newButton = Instantiate(buttonPrefab, respawnUI.transform);

            // �{�^�����ݒ�
            TMP_Text textComp = newButton.GetComponentInChildren<TMP_Text>();
            if (textComp != null)
                textComp.text = playerName;

            // Button �R���|�[�l���g�擾��Listener�o�^
            Button btn = newButton.GetComponent<Button>();
            if (btn == null)
                btn = newButton.GetComponentInChildren<Button>();

            if (btn != null)
            {
                int index = i; // �N���[�W���΍�
                btn.onClick.AddListener(() => OnCharacterSelected(index));
            }
        }
    }

    /// <summary>
    /// �L�����I����̏����i�{�^�������ŌĂ΂��j
    /// </summary>
    private void OnCharacterSelected(int index)
    {
        if (!isWaitingForSelection) return;

        Debug.Log($"�v���C���[[{playerPrefabs[index].name}]��I���B���X�|�[�����܂��B");

        respawnUI.SetActive(false);
        isWaitingForSelection = false;

        SpawnPlayer(index);
    }

    /// <summary>
    /// �v���C���[�𐶐�
    /// </summary>
    private void SpawnPlayer(int index)
    {
        // �����v���C���[��j��
        if (currentPlayer != null)
            Destroy(currentPlayer);

        // �V�����v���C���[�𐶐�
        GameObject prefab = playerPrefabs[index];
        currentPlayer = Instantiate(prefab, respawnPoint.position, Quaternion.identity);

        Debug.Log($"�v���C���[[{prefab.name}]�����X�|�[�����܂����B");
    }

    /// <summary>
    /// �X�e�[�W���ɑ��݂��Ă���NPC�̖��O���擾
    /// </summary>
    private HashSet<string> GetExistingNPCTypes()
    {
        HashSet<string> existingTypes = new HashSet<string>();
        var npcs = FindObjectsOfType<NPCBase>();

        foreach (var npc in npcs)
        {
            string npcName = npc.name.Replace("(Clone)", "").Trim();
            existingTypes.Add(npcName);
        }

        return existingTypes;
    }
}
