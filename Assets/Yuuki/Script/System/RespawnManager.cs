using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RespawnManager : MonoBehaviour
{
    [Header("���X�|�[���ݒ�")]
    [SerializeField] private List<GameObject> playerPrefabs; // ���X�|�[�����
    [SerializeField] private Transform respawnPoint;          // ���X�|�[���ʒu
    [SerializeField] private GameObject respawnUI;            // ���X�|�[��UI�iCanvas�z���j
    [SerializeField] private Button buttonPrefab;             // �I���{�^���̃v���n�u

    private GameObject currentPlayer;
    private bool isWaitingForSelection = false;

    private void Start()
    {
        if (respawnUI != null) respawnUI.SetActive(false);
        SpawnPlayer(0); // ���ōŏ��̃L����
    }

    public void OnPlayerDeath()
    {
        Debug.Log("�v���C���[���S�B���X�|�[���I��UI��\���B");
        ShowRespawnUI();
    }

    /// <summary>
    /// ���݃X�e�[�W�ɑ��݂���NPC(�G)�̃^�C�v���擾
    /// </summary>
    private HashSet<NPCBase.EnemyType> GetExistingEnemyTypes()
    {
        HashSet<NPCBase.EnemyType> existingTypes = new();
        var enemies = FindObjectsOfType<NPCBase>();

        foreach (var enemy in enemies)
        {
            if (enemy.GetEnemyType() != NPCBase.EnemyType.TYPE_NULL)
                existingTypes.Add(enemy.GetEnemyType());
        }

        return existingTypes;
    }

    /// <summary>
    /// �L�����I��UI��\���i�G�̃^�C�v�ɉ����ăt�B���^�j
    /// </summary>
    private void ShowRespawnUI()
    {
        if (respawnUI == null || buttonPrefab == null)
        {
            Debug.LogError("Respawn UI �܂��� �{�^���v���n�u ���ݒ肳��Ă��܂���I");
            return;
        }

        // �����{�^�����폜
        foreach (Transform child in respawnUI.transform)
        {
            Destroy(child.gameObject);
        }

        // �X�e�[�W��̓G�^�C�v���擾
        HashSet<NPCBase.EnemyType> allowedTypes = GetExistingEnemyTypes();
        Debug.Log($"[RespawnManager] ���݂̓G�^�C�v: {string.Join(", ", allowedTypes)}");


        // �Ή��v���C���[�̂݃{�^������
        for (int i = 0; i < playerPrefabs.Count; i++)
        {
            var playerPrefab = playerPrefabs[i];
            
            if (!playerPrefab.TryGetComponent<MPlayerBase>(out var playerBase))
            {
                Debug.LogWarning($"[{playerPrefab.name}] �� PlayerBase ���A�^�b�`����Ă��܂���B");
                continue;
            }

            var pType = playerBase.GetEnemyType();

            // �G�̃^�C�v�ɑ��݂���v���C���[�̂݃{�^������
            if (allowedTypes.Contains(pType))
            {
                var newButton = Instantiate(buttonPrefab, respawnUI.transform);
                var textComp = newButton.GetComponentInChildren<TMP_Text>();
                var imageComp = newButton.GetComponentInChildren<Image>();

                // �e�L�X�g�ݒ�
                if (textComp != null)
                    textComp.text = playerPrefab.name;

                // �摜�ݒ�i�{�^���� Image ������ꍇ�j
                var icon = playerBase.GetRespawnIcon();
                if (imageComp != null && icon != null)
                    imageComp.sprite = icon;

                int index = i; // �N���[�W���[�΍�
                newButton.onClick.AddListener(() => OnCharacterSelected(index));
                Debug.Log("���X�|�[���{�^�������F" + playerPrefab.name);
            }
        }

        respawnUI.SetActive(true);
        isWaitingForSelection = true;
    }

    /// <summary>
    /// �v���C���[�I����̏���
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
    /// �v���C���[����
    /// </summary>
    private void SpawnPlayer(int index)
    {
        if (currentPlayer != null)
            Destroy(currentPlayer);

        GameObject prefab = playerPrefabs[index];
        currentPlayer = Instantiate(prefab, respawnPoint.position, Quaternion.identity);

        Debug.Log($"�v���C���[[{prefab.name}]�����X�|�[�����܂����B");
    }
}
