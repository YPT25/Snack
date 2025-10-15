using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敵NPCをスポーン・管理するスクリプト
/// - 複数種類の敵Prefabをランダムにスポーン可能
/// - スポーン位置も複数設定可能
/// - 倒れた敵は一定時間後に自動でリスポーン
/// - 拡張性を考慮してInspectorから簡単に設定変更可能
/// </summary>
public class EnemySponer : MonoBehaviour
{
    [Header("スポーン位置リスト")]
    [Tooltip("敵を出現させる位置を複数指定可能")]
    [SerializeField] private List<Transform> m_spawnPoints;

    [Header("スポーンする敵Prefabリスト")]
    [Tooltip("NormalBox_NPC, SphereBox_NPC, EmptyBox_NPC, CannonBox_NPC など")]
    [SerializeField] private List<GameObject> m_enemyPrefabs;

    [Header("スポーン設定")]
    [Tooltip("同時に出現させる最大敵数")]
    [SerializeField] private int m_maxEnemies = 10;

    [Tooltip("初期スポーンの間隔（秒）")]
    [SerializeField] private float m_initialSpawnInterval = 1.0f;

    [Tooltip("倒された敵がリスポーンするまでの待機時間（秒）")]
    [SerializeField] private float m_respawnTime = 5f;

    // 現在スポーンしている敵のリスト
    private List<GameObject> m_spawnedEnemies = new List<GameObject>();
    // 初期スポーン
    // Start is called before the first frame update
    void Start()
    {
        // 初期スポーンはコルーチンで時間差生成
        StartCoroutine(InitialSpawnCoroutine());
    }
    /// <summary>
    /// 初期スポーンを時間を空けて生成
    /// </summary>
    private IEnumerator InitialSpawnCoroutine()
    {
        for (int i = 0; i < m_maxEnemies; i++)
        {
            SpawnRandomEnemy();
            yield return new WaitForSeconds(m_initialSpawnInterval);
        }
    }

    // 毎フレーム、倒された敵をチェックしてリスポーン
    // Update is called once per frame
    void Update()
    {
        for (int i = m_spawnedEnemies.Count - 1; i >= 0; i--)
        {
            // null になっている＝敵が倒されてDestroyされている場合
            if (m_spawnedEnemies[i] == null)
            {
                // リストから削除
                m_spawnedEnemies.RemoveAt(i);

                // 一定時間後に新しい敵をスポーン
                StartCoroutine(RespawnCoroutine());
            }
        }
    }

    /// <summary>
    /// ランダムにPrefabとスポーン位置を選んで敵を生成
    /// </summary>
    private void SpawnRandomEnemy()
    {
        // PrefabやSpawnPointが未設定なら何もしない
        if (m_enemyPrefabs.Count == 0 || m_spawnPoints.Count == 0) return;

        // ランダムにPrefabを選択
        GameObject prefab = m_enemyPrefabs[Random.Range(0, m_enemyPrefabs.Count)];

        // ランダムにスポーン位置を選択
        Transform spawnPoint = m_spawnPoints[Random.Range(0, m_spawnPoints.Count)];

        // Instantiateしてシーンに配置
        GameObject enemy = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

        // リストに追加して管理
        m_spawnedEnemies.Add(enemy);
    }

    /// <summary>
    /// 指定時間待ってから敵を再スポーン
    /// </summary>
    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(m_respawnTime);
        SpawnRandomEnemy();
    }
    // - 敵タイプごとにスポーン間隔を変える場合は
    //   SpawnRandomEnemy() 内でPrefabごとの設定を読み込む
    // - 同時出現数を敵タイプ別に制御する場合もリストを分けて管理可能

}
