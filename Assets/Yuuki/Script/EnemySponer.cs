using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �GNPC���X�|�[���E�Ǘ�����X�N���v�g
/// - ������ނ̓GPrefab�������_���ɃX�|�[���\
/// - �X�|�[���ʒu�������ݒ�\
/// - �|�ꂽ�G�͈�莞�Ԍ�Ɏ����Ń��X�|�[��
/// - �g�������l������Inspector����ȒP�ɐݒ�ύX�\
/// </summary>
public class EnemySponer : MonoBehaviour
{
    [Header("�X�|�[���ʒu���X�g")]
    [Tooltip("�G���o��������ʒu�𕡐��w��\")]
    [SerializeField] private List<Transform> m_spawnPoints;

    [Header("�X�|�[������GPrefab���X�g")]
    [Tooltip("NormalBox_NPC, SphereBox_NPC, EmptyBox_NPC, CannonBox_NPC �Ȃ�")]
    [SerializeField] private List<GameObject> m_enemyPrefabs;

    [Header("�X�|�[���ݒ�")]
    [Tooltip("�����ɏo��������ő�G��")]
    [SerializeField] private int m_maxEnemies = 10;

    [Tooltip("�����X�|�[���̊Ԋu�i�b�j")]
    [SerializeField] private float m_initialSpawnInterval = 1.0f;

    [Tooltip("�|���ꂽ�G�����X�|�[������܂ł̑ҋ@���ԁi�b�j")]
    [SerializeField] private float m_respawnTime = 5f;

    // ���݃X�|�[�����Ă���G�̃��X�g
    private List<GameObject> m_spawnedEnemies = new List<GameObject>();
    // �����X�|�[��
    // Start is called before the first frame update
    void Start()
    {
        // �����X�|�[���̓R���[�`���Ŏ��ԍ�����
        StartCoroutine(InitialSpawnCoroutine());
    }
    /// <summary>
    /// �����X�|�[�������Ԃ��󂯂Đ���
    /// </summary>
    private IEnumerator InitialSpawnCoroutine()
    {
        for (int i = 0; i < m_maxEnemies; i++)
        {
            SpawnRandomEnemy();
            yield return new WaitForSeconds(m_initialSpawnInterval);
        }
    }

    // ���t���[���A�|���ꂽ�G���`�F�b�N���ă��X�|�[��
    // Update is called once per frame
    void Update()
    {
        for (int i = m_spawnedEnemies.Count - 1; i >= 0; i--)
        {
            // null �ɂȂ��Ă��遁�G���|�����Destroy����Ă���ꍇ
            if (m_spawnedEnemies[i] == null)
            {
                // ���X�g����폜
                m_spawnedEnemies.RemoveAt(i);

                // ��莞�Ԍ�ɐV�����G���X�|�[��
                StartCoroutine(RespawnCoroutine());
            }
        }
    }

    /// <summary>
    /// �����_����Prefab�ƃX�|�[���ʒu��I��œG�𐶐�
    /// </summary>
    private void SpawnRandomEnemy()
    {
        // Prefab��SpawnPoint�����ݒ�Ȃ牽�����Ȃ�
        if (m_enemyPrefabs.Count == 0 || m_spawnPoints.Count == 0) return;

        // �����_����Prefab��I��
        GameObject prefab = m_enemyPrefabs[Random.Range(0, m_enemyPrefabs.Count)];

        // �����_���ɃX�|�[���ʒu��I��
        Transform spawnPoint = m_spawnPoints[Random.Range(0, m_spawnPoints.Count)];

        // Instantiate���ăV�[���ɔz�u
        GameObject enemy = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

        // ���X�g�ɒǉ����ĊǗ�
        m_spawnedEnemies.Add(enemy);
    }

    /// <summary>
    /// �w�莞�ԑ҂��Ă���G���ăX�|�[��
    /// </summary>
    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(m_respawnTime);
        SpawnRandomEnemy();
    }
    // - �G�^�C�v���ƂɃX�|�[���Ԋu��ς���ꍇ��
    //   SpawnRandomEnemy() ����Prefab���Ƃ̐ݒ��ǂݍ���
    // - �����o������G�^�C�v�ʂɐ��䂷��ꍇ�����X�g�𕪂��ĊǗ��\

}
