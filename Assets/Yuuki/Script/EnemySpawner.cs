using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// 敵NPCをスポーン・管理するスクリプト（Mirror対応）
/// 特定敵タイプごとに上限数・リスポーン時間を設定可能
/// </summary>
public class EnemySpawner : NetworkBehaviour
{
    [System.Serializable]
    public class EnemySpawnSetting
    {
        [Header("敵プレハブ (NetworkIdentity付き)")]
        public GameObject enemyPrefab;

        [Header("この敵の最大同時出現数")]
        public int maxCount = 5;

        [Header("倒された敵のリスポーン待機時間（秒）")]
        public float respawnDelay = 5f;

        // 現在シーン内に存在している個体を追跡
        [HideInInspector] public List<GameObject> activeEnemies = new List<GameObject>();
    }

    [Header("スポーン位置リスト")]
    [Tooltip("敵を出現させる位置を複数指定可能")]
    [SerializeField] private List<Transform> m_spawnPoints = new List<Transform>();

    [Header("敵タイプ別設定")]
    [Tooltip("各敵タイプごとに最大数・リスポーン時間を設定")]
    [SerializeField] private List<EnemySpawnSetting> m_enemySettings = new List<EnemySpawnSetting>();

    [Header("初期スポーン間隔（秒）")]
    [SerializeField] private float m_initialSpawnInterval = 0.5f;

    // ======================================================
    // 起動時（サーバーのみ）
    // ======================================================
    [ServerCallback]
    private void Start()
    {
        StartCoroutine(InitialSpawnCoroutine());
    }

    // ======================================================
    // 定期チェック（サーバーのみ）
    // ======================================================
    [ServerCallback]
    private void Update()
    {
        foreach (var setting in m_enemySettings)
        {
            for (int i = setting.activeEnemies.Count - 1; i >= 0; i--)
            {
                if (setting.activeEnemies[i] == null)
                {
                    // null＝死亡済み → リストから削除＆リスポーン予約
                    setting.activeEnemies.RemoveAt(i);
                    StartCoroutine(RespawnCoroutine(setting));
                }
            }
        }
    }

    // ======================================================
    // 初期スポーン
    // ======================================================
    [Server]
    private IEnumerator InitialSpawnCoroutine()
    {
        // 各タイプごとに上限数まで生成
        foreach (var setting in m_enemySettings)
        {
            for (int i = 0; i < setting.maxCount; i++)
            {
                SpawnEnemy(setting);
                yield return new WaitForSeconds(m_initialSpawnInterval);
            }
        }
    }

    // ======================================================
    // スポーン処理
    // ======================================================
    [Server]
    private void SpawnEnemy(EnemySpawnSetting setting)
    {
        if (setting.enemyPrefab == null || m_spawnPoints.Count == 0) return;

        if (setting.activeEnemies.Count >= setting.maxCount)
            return; // 上限に達している

        Transform spawnPoint = m_spawnPoints[Random.Range(0, m_spawnPoints.Count)];
        GameObject enemy = Instantiate(setting.enemyPrefab, spawnPoint.position, spawnPoint.rotation);

        // NetworkSpawnで同期
        NetworkServer.Spawn(enemy);

        setting.activeEnemies.Add(enemy);

        Debug.Log($"[Server] {enemy.name} スポーン (現在 {setting.activeEnemies.Count}/{setting.maxCount})");
    }

    // ======================================================
    // リスポーン処理
    // ======================================================
    [Server]
    private IEnumerator RespawnCoroutine(EnemySpawnSetting setting)
    {
        yield return new WaitForSeconds(setting.respawnDelay);
        SpawnEnemy(setting);
    }

    // ======================================================
    // ユーティリティ
    // ======================================================

    /// <summary>
    /// 指定Prefabの現在出現数を取得
    /// </summary>
    public int GetEnemyCount(GameObject prefab)
    {
        foreach (var s in m_enemySettings)
        {
            if (s.enemyPrefab == prefab)
                return s.activeEnemies.Count;
        }
        return 0;
    }

    /// <summary>
    /// 全ての敵を強制削除
    /// </summary>
    [Server]
    public void ClearAllEnemies()
    {
        foreach (var setting in m_enemySettings)
        {
            foreach (var e in setting.activeEnemies)
            {
                if (e != null)
                    NetworkServer.Destroy(e);
            }
            setting.activeEnemies.Clear();
        }
    }
}