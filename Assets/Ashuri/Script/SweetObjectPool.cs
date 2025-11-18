using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Mirror;

/// <summary>
/// Sweetオブジェクトを生成・管理するオブジェクトプール
/// 各Prefabごとに生成確率を設定でき、合計確率を正規化
/// Mirrorネットワーク上で同期されるSweetを効率的に再利用
/// 同時生成されるSweetの最大数を制御
/// </summary>
public class SweetObjectPool : NetworkBehaviour
{
    [System.Serializable]
    public struct SweetPrefabData
    {
        [Tooltip("生成するPrefab")]
        public GameObject prefab;

        [Tooltip("生成確率（合計は自動で正規化されます）")]
        [Range(0f, 1f)]
        public float spawnProbability;
    }

    [Header("複製する元と確率")]
    [Tooltip("SweetPrefabとそれぞれの生成確率を設定")]
    [SerializeField] private List<SweetPrefabData> _sweetPrefabs;

    [Header("複製する場所")]
    [Tooltip("Sweetを生成する親Transform")]
    [SerializeField] private Transform _sweetContent;

    [Header("Sweet生成間隔")]
    [Tooltip("Sweetを生成する時間間隔（秒）")]
    [SerializeField] private float _spawnInterval = 2f;

    [Header("Sweet最大生成数")]
    [Tooltip("同時に生成できるSweetの最大数")]
    [SerializeField] private int _maxSpawnCount = 20;

    [Header("Sweet生成範囲の頂点(X-Z平面)")]
    [Tooltip("四角形の4頂点をローカル座標で設定")]
    [SerializeField] private Vector3[] _spawnAreaVertices = new Vector3[4];

    [Header("オブジェクトプール")]
    private ObjectPool<GameObject> _pool;

    // 正規化された確率リスト
    private List<float> _normalizedProbabilities;

    // 現在生成されているSweetの数
    private int _currentSpawnCount = 0;

    // 生成タイマー
    private float _timer;

    // ゲームマネジャーへの参照
    private GameManager gameManager;

    /// <summary>
    /// サーバーで初期化される処理
    /// </summary>
    public override void OnStartServer()
    {
        base.OnStartServer();

        // _sweetContentが未設定なら自身を親にする
        if (_sweetContent == null)
        {
            _sweetContent = transform;
            Debug.LogWarning($"'_sweetContent' not set. Using '{_sweetContent.name}' as default.");
        }

        // 各Prefabの生成確率を正規化
        NormalizeProbabilities();

        // オブジェクトプールを初期化
        _pool = new ObjectPool<GameObject>(
            OnCreatePoolObject,    // 新規オブジェクト作成時
            OnGetFromPool,         // プールから取得時
            OnReleaseToPool,       // プールに返却時
            OnDestroyPooledObject, // プール破棄時
            defaultCapacity: 10,
            maxSize: 100
        );

        // 生成タイマー初期化
        _timer = _spawnInterval;

        // ゲームマネジャーを取得
        gameManager = FindObjectOfType<GameManager>();

        // 頂点が正しく設定されていない場合、デフォルトの四角形を使用
        if (_spawnAreaVertices == null || _spawnAreaVertices.Length != 4)
        {
            Debug.LogError("SpawnAreaVertices must contain exactly 4 vertices. Using default [-5,5].");
            _spawnAreaVertices = new Vector3[]
            {
                new Vector3(-5,0,-5),
                new Vector3(5,0,-5),
                new Vector3(5,0,5),
                new Vector3(-5,0,5)
            };
        }
    }

    /// <summary>
    /// サーバー側で毎フレーム呼ばれる処理
    /// </summary>
    [ServerCallback]
    void Update()
    {
        // ゲームが開始されていなければ処理しない
        if (!gameManager.gameStarted) return;

        // サーバーでない場合処理しない
        if (!isServer) return;

        // タイマーを減算
        _timer -= Time.deltaTime;

        // タイマーが0以下ならSweetを生成
        if (_timer <= 0f)
        {
            SpawnSweet();
            _timer = _spawnInterval; // タイマーをリセット
        }
    }

    /// <summary>
    /// 各Prefabの生成確率を合計1に正規化
    /// </summary>
    private void NormalizeProbabilities()
    {
        _normalizedProbabilities = new List<float>();
        float total = 0f;

        // 確率の合計を計算
        foreach (var data in _sweetPrefabs)
            total += data.spawnProbability;

        // 合計が0以下の場合は等確率に設定
        if (total <= 0f)
        {
            Debug.LogWarning("Total probability is zero or negative. Using equal probabilities.");
            float equal = 1f / _sweetPrefabs.Count;
            for (int i = 0; i < _sweetPrefabs.Count; i++)
                _normalizedProbabilities.Add(equal);
            return;
        }

        // 正規化
        foreach (var data in _sweetPrefabs)
            _normalizedProbabilities.Add(data.spawnProbability / total);
    }

    /// <summary>
    /// 確率に応じてPrefabを選択
    /// </summary>
    private GameObject SelectPrefabByProbability()
    {
        float rand = Random.value;
        float cumulative = 0f;

        for (int i = 0; i < _sweetPrefabs.Count; i++)
        {
            cumulative += _normalizedProbabilities[i];
            if (rand <= cumulative)
                return _sweetPrefabs[i].prefab;
        }

        // 万一の場合は最後のPrefabを返す
        return _sweetPrefabs[_sweetPrefabs.Count - 1].prefab;
    }

    /// <summary>
    /// プール用に新規オブジェクトを作成
    /// </summary>
    public GameObject OnCreatePoolObject()
    {
        GameObject prefabToSpawn = SelectPrefabByProbability();
        GameObject sweetObject = Instantiate(prefabToSpawn);
        sweetObject.SetActive(false); // 初期状態は非アクティブ
        return sweetObject;
    }

    /// <summary>
    /// プールから取得したオブジェクトを有効化
    /// </summary>
    public void OnGetFromPool(GameObject target)
    {
        target.SetActive(true);
    }

    /// <summary>
    /// プールに返却するオブジェクトを無効化
    /// </summary>
    public void OnReleaseToPool(GameObject target)
    {
        target.SetActive(false);
        target.transform.SetParent(null); // 親を解除
    }

    /// <summary>
    /// プールオブジェクト破棄時の処理
    /// </summary>
    public void OnDestroyPooledObject(GameObject target)
    {
        if (target != null && target.TryGetComponent<NetworkIdentity>(out NetworkIdentity ni) && ni.isServer)
            NetworkServer.Destroy(target); // サーバー上で削除
        else
            Destroy(target); // クライアントで削除
    }

    /// <summary>
    /// Sweetを生成
    /// 最大生成数を超えている場合は生成しない
    /// </summary>
    private void SpawnSweet()
    {
        // 最大生成数を超えていれば生成しない
        if (_currentSpawnCount >= _maxSpawnCount) return;

        // プールからオブジェクトを取得
        GameObject sweet = _pool.Get();
        _currentSpawnCount++; // 現在生成数をカウント

        // 親を設定
        sweet.transform.SetParent(_sweetContent);

        // ランダム位置を決定
        Vector3 randomLocalPosition = GetRandomPointInQuadXZ();
        sweet.transform.localPosition = randomLocalPosition;
        sweet.transform.localRotation = Quaternion.identity;

        // ネットワーク上で生成
        NetworkServer.Spawn(sweet);

        // クライアント側にも親を設定
        NetworkIdentity parentIdentity = _sweetContent.GetComponent<NetworkIdentity>();
        if (parentIdentity != null)
        {
            SweetParentSetter parentSetter = sweet.GetComponent<SweetParentSetter>();
            if (parentSetter != null)
                parentSetter.RpcSetParent(parentIdentity);
        }
    }

    /// <summary>
    /// Sweetをプールに返却
    /// </summary>
    public void ReleaseSweet(GameObject sweetToRelease)
    {
        if (sweetToRelease == null) return;

        _pool.Release(sweetToRelease);
        _currentSpawnCount = Mathf.Max(0, _currentSpawnCount - 1); // 安全に減算
    }

    /// <summary>
    /// 四角形範囲内のランダム位置を取得(X-Z平面)
    /// </summary>
    private Vector3 GetRandomPointInQuadXZ()
    {
        float r1 = Random.value;
        float r2 = Random.value;
        Vector3 point;

        // 乱数の組み合わせで四角形内の位置を計算
        if (r1 + r2 < 1)
            point = _spawnAreaVertices[0] + r1 * (_spawnAreaVertices[1] - _spawnAreaVertices[0]) + r2 * (_spawnAreaVertices[2] - _spawnAreaVertices[0]);
        else
        {
            float r1p = 1 - r1;
            float r2p = 1 - r2;
            point = _spawnAreaVertices[3] + r1p * (_spawnAreaVertices[2] - _spawnAreaVertices[3]) + r2p * (_spawnAreaVertices[0] - _spawnAreaVertices[3]);
        }

        // Y座標を親のY座標に合わせる
        point.y = _sweetContent.InverseTransformPoint(_sweetContent.position).y;
        return point;
    }

    /// <summary>
    /// Gizmoで生成範囲を可視化
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (_sweetContent == null || _spawnAreaVertices == null || _spawnAreaVertices.Length != 4) return;

        Gizmos.color = Color.cyan;

        // 各頂点をワールド座標に変換
        Vector3 v0 = _sweetContent.TransformPoint(_spawnAreaVertices[0]);
        Vector3 v1 = _sweetContent.TransformPoint(_spawnAreaVertices[1]);
        Vector3 v2 = _sweetContent.TransformPoint(_spawnAreaVertices[2]);
        Vector3 v3 = _sweetContent.TransformPoint(_spawnAreaVertices[3]);

        // 四角形の枠を描画
        Gizmos.DrawLine(v0, v1);
        Gizmos.DrawLine(v1, v2);
        Gizmos.DrawLine(v2, v3);
        Gizmos.DrawLine(v3, v0);

        // 頂点を黄色の球で表示
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(v0, 0.1f);
        Gizmos.DrawSphere(v1, 0.1f);
        Gizmos.DrawSphere(v2, 0.1f);
        Gizmos.DrawSphere(v3, 0.1f);
    }
}
