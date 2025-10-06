using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Mirror;

/// <summary>
/// Sweetオブジェクトを生成・管理するためのオブジェクトプール。
/// Mirrorネットワーク上で同期されるSweetを効率的に再利用する。
/// </summary>
public class SweetObjectPool : NetworkBehaviour
{
    [Header("複製する元")]
    [SerializeField] private GameObject _sweetPrefab;
    [Header("複製する場所 (このスクリプトがアタッチされているオブジェクト)")]
    [Tooltip("Sweetを生成する際の親となるTransform。通常、このスクリプトがアタッチされているオブジェクトを設定します。")]
    [SerializeField] private Transform _sweetContent;
    [Header("Sweetを生成する間隔")]
    [SerializeField] private float _spawnInterval = 2f;

    [Header("オブジェクトプール")]
    private ObjectPool<GameObject> _pool;

    // Sweetの生成タイミングを計るためのタイマー
    private float _timer;

    [Header("Sweet生成範囲の頂点 (X-Z平面)")]
    [Tooltip("Sweetが生成されるX-Z平面上の4つの頂点座標を設定します。Y座標は無視されます。" +
             "これらの座標は'_sweetContent'からの相対的なローカル座標として扱われます。")]
    [SerializeField] private Vector3[] _spawnAreaVertices = new Vector3[4];

    // Startはサーバーでのみ実行されるようにする
    public override void OnStartServer()
    {
        base.OnStartServer(); // 親クラスのOnStartServerを呼び出す

        // _sweetContentが未設定の場合、このスクリプトがアタッチされているTransformを使用
        if (_sweetContent == null)
        {
            _sweetContent = this.transform;
            Debug.LogWarning($"'_sweetContent' was not set. Using '{_sweetContent.name}' (this GameObject's transform) as default.");
        }

        // UnityのObjectPoolの初期化
        _pool = new ObjectPool<GameObject>(
            OnCreatePoolObject,          // ゲームオブジェクトの生成処理の関数 (サーバーのみ)
            OnGetFromPool,               // オブジェクトプールからゲームオブジェクトを取得する関数 (サーバーのみ)
            OnReleaseToPool,             // ゲームオブジェクトをオブジェクトプールに返却する処理関数 (サーバーのみ)
            OnDestroyPooledObject,       // ゲームオブジェクトを削除する処理の関数 (サーバーのみ)
            defaultCapacity: 10,         // 初期容量 (任意)
            maxSize: 100                 // 最大サイズ (任意)
        );

        _timer = _spawnInterval; // 初回生成のためにタイマーを初期化

        // 頂点が設定されているかチェック
        if (_spawnAreaVertices == null || _spawnAreaVertices.Length != 4)
        {
            Debug.LogError("SpawnAreaVertices must contain exactly 4 vertices. Please set them in the Inspector. " +
                           "Using default square range [-5,5] in X and Z.");
            // デフォルトの範囲を設定しておく (フォールバック)
            _spawnAreaVertices = new Vector3[]
            {
                new Vector3(-5, 0, -5), // bottom-left
                new Vector3(5, 0, -5),  // bottom-right
                new Vector3(5, 0, 5),   // top-right
                new Vector3(-5, 0, 5)   // top-left
            };
        }
    }

    // Updateはサーバーでのみ実行されるようにする
    [ServerCallback] // このメソッドがサーバー上でのみ呼び出されることを保証
    void Update()
    {
        // サーバー上でのみSweetの生成ロジックを実行
        if (!isServer) return; // 念のため、サーバーでなければ処理しない

        _timer -= Time.deltaTime; // タイマーを減少させる

        if (_timer <= 0)
        {
            SpawnSweet(); // Sweetを生成する
            _timer = _spawnInterval; // タイマーをリセット
        }
    }

    /// <summary>
    /// オブジェクトプール用のゲームオブジェクト生成処理の関数。
    /// この関数はサーバー側でオブジェクトが不足した際に呼び出される。
    /// </summary>
    /// <returns>生成されたGameObject</returns>
    public GameObject OnCreatePoolObject()
    {
        // ここではまだ親を設定しません。スポーン後、RPCでクライアントにも親を同期させます。
        // Instantiateはサーバー側で実行されます。
        GameObject sweetObject = Instantiate(_sweetPrefab);
        // 生成したSweetオブジェクトは最初は非アクティブな状態でPoolに格納されるようにする
        sweetObject.SetActive(false);
        return sweetObject;
    }

    /// <summary>
    /// オブジェクトプールからゲームオブジェクトを取得する処理の関数。
    /// アクティブ化と初期設定が行われる。
    /// </summary>
    /// <param name="target">プールから取得したGameObject</param>
    public void OnGetFromPool(GameObject target)
    {
        target.gameObject.SetActive(true); // オブジェクトをアクティブにする
        // ここではまだ親を設定しません。SpawnSweet()内で設定し、クライアントに同期します。
    }

    /// <summary>
    /// ゲームオブジェクトをオブジェクトプールに返却する処理の関数。
    /// 非アクティブ化とクリーンアップが行われる。
    /// </summary>
    /// <param name="target">プールに返却するGameObject</param>
    public void OnReleaseToPool(GameObject target)
    {
        // オブジェクトを非アクティブにする
        target.gameObject.SetActive(false);
        // プールに戻す際に、オブジェクトの親を解除しておくことで、
        // 次に取得されたときにクリーンな状態からスタートできるようにする。
        target.transform.SetParent(null); // 親を解除
    }

    /// <summary>
    /// ゲームオブジェクトを完全に削除する処理の関数。
    /// オブジェクトプールがいっぱいになった場合や、明示的な削除要求があった場合に呼び出される。
    /// </summary>
    /// <param name="target">削除するGameObject</param>
    public void OnDestroyPooledObject(GameObject target)
    {
        // MirrorでNetworkServer.SpawnされたオブジェクトをDestroyする際は、
        // NetworkServer.Destroy を使用してネットワークからオブジェクトを削除します。
        // これは、Poolingをせずに完全にオブジェクトを削除する場合に重要です。
        // ただし、通常PoolingではDestroyPooledObjectが呼ばれるのは稀なケースです。
        if (target != null && target.TryGetComponent<NetworkIdentity>(out NetworkIdentity ni) && ni.isServer)
        {
            NetworkServer.Destroy(target);
        }
        else
        {
            Destroy(target.gameObject);
        }
    }

    /// <summary>
    /// Sweetを生成し、オブジェクトプールから取得してネットワークにスポーンする関数。
    /// </summary>
    private void SpawnSweet()
    {
        // オブジェクトプールからSweetオブジェクトを取得
        GameObject sweet = _pool.Get();

        // サーバー側で、一時的に親を設定し、座標を調整する。
        // このSetParentは、クライアントには直接同期されないため、RPCで別途通知します。
        sweet.transform.SetParent(_sweetContent);

        // 4つの頂点で定義されるX-Z平面の四角形内のランダムなローカル座標を取得します。
        // この関数の内部で、Y座標は _sweetContent のローカルY座標に固定されます。
        Vector3 randomLocalPosition = GetRandomPointInQuadXZ();

        // 取得したランダムなローカル座標をSweetのlocalPositionに設定
        sweet.transform.localPosition = randomLocalPosition;
        sweet.transform.localRotation = Quaternion.identity; // 回転もリセット
        sweet.transform.localScale = Vector3.one; // スケールもリセット (必要に応じて)

        // 必要であれば、ここでSweetのコンポーネントにアクセスして初期設定を行います
        // 例: sweet.GetComponent<SweetData>().Initialize(Color.red);

        // オブジェクトをネットワーク上にスポーンします。
        // これにより、クライアント側でもこのSweetオブジェクトが生成され、
        // そのTransformが同期され始めます。
        NetworkServer.Spawn(sweet);

        // クライアント側で親を設定するためにRPCを呼び出す。
        // _sweetContentがNetworkIdentityを持っている必要がある。
        NetworkIdentity parentIdentity = _sweetContent.GetComponent<NetworkIdentity>();
        if (parentIdentity != null)
        {
            SweetParentSetter parentSetter = sweet.GetComponent<SweetParentSetter>();
            if (parentSetter != null)
            {
                // クライアントのSweetParentSetterに、親となるNetworkIdentityを渡してRPCを呼び出す。
                parentSetter.RpcSetParent(parentIdentity);
            }
            else
            {
                Debug.LogWarning($"Sweet prefab '{_sweetPrefab.name}' is missing SweetParentSetter component. Cannot set parent on clients.");
            }
        }
        else
        {
            // _sweetContentにNetworkIdentityがない場合でも、基本的な生成は可能です。
            // ただし、クライアント側でSweetが親の下に階層化されないため、
            // ワールド座標で生成されてしまいます。
            Debug.LogWarning($"'_sweetContent' ({_sweetContent.name}) is missing NetworkIdentity component. " +
                             "Sweet objects will spawn in world space on clients if not manually parented.");
        }
    }

    /// <summary>
    /// Sweetが破壊されたり、不要になったりした際にプールに戻すための関数。
    /// Sweet自身のスクリプトなどから呼び出されることを想定。
    /// </summary>
    /// <param name="sweetToRelease">プールに返却するSweet GameObject</param>
    public void ReleaseSweet(GameObject sweetToRelease)
    {
        if (sweetToRelease == null) return;

        // オブジェクトプールに戻す場合、NetworkServer.UnSpawnは通常行いません。
        // NetworkServer.UnSpawn を実行すると、クライアントからもオブジェクトが消えてしまいます。
        // プールの目的は再利用なので、完全にネットワークから削除するのではなく、
        // 非アクティブにしてプールに返却し、次回の利用に備えます。
        _pool.Release(sweetToRelease); // オブジェクトをプールに返却
    }

    /// <summary>
    /// 4つの頂点で定義されるX-Z平面の四角形（または凸四角形）内のランダムなローカル座標を返す。
    /// 返されるY座標は、'_sweetContent'のローカルY座標に固定されます。
    /// </summary>
    /// <returns>四角形内のランダムなローカル座標</returns>
    private Vector3 GetRandomPointInQuadXZ()
    {
        // _spawnAreaVertices が _sweetContent のローカル座標として入力されることを想定しています。
        // (もしワールド座標として入力される場合は、事前に _sweetContent.InverseTransformPoint() で
        //  ローカル座標に変換する必要がありますが、今回はインスペクターからの入力なのでローカルを想定します)

        // 四角形を2つの三角形に分割し、どちらかの三角形内でランダムな点を生成します。
        // この方法は、四角形が凸型である場合に最も均一な分布を提供します。
        // 頂点0, 1, 2, 3 が反時計回りまたは時計回りに順序良く並んでいることを前提とします。

        // 重心座標系 (Barycentric Coordinates) を利用して、
        // 三角形内のランダムな点を効率的に生成します。
        float r1 = Random.value;
        float r2 = Random.value;

        Vector3 randomPoint;

        if (r1 + r2 < 1)
        {
            // 頂点0, 頂点1, 頂点2 で構成される三角形の範囲
            randomPoint = _spawnAreaVertices[0] + r1 * (_spawnAreaVertices[1] - _spawnAreaVertices[0]) + r2 * (_spawnAreaVertices[2] - _spawnAreaVertices[0]);
        }
        else
        {
            // 頂点0, 頂点2, 頂点3 で構成される三角形の範囲
            // または、頂点3, 頂点2, 頂点0 の順で考える (1-r1, 1-r2 を使用)
            float r1Prime = 1 - r1;
            float r2Prime = 1 - r2;
            randomPoint = _spawnAreaVertices[3] + r1Prime * (_spawnAreaVertices[2] - _spawnAreaVertices[3]) + r2Prime * (_spawnAreaVertices[0] - _spawnAreaVertices[3]);
        }

        // Y座標は'_sweetContent'のローカルY座標に固定します。
        // _sweetContentの子としてSweetが生成されるため、親のローカルY座標を適用することで
        // 親と同じ高さ（Y座標）にSweetが配置されます。
        // 通常、_sweetContentのローカルY座標は0なので、SweetのY座標も0になります。
        randomPoint.y = _sweetContent.InverseTransformPoint(_sweetContent.position).y;

        return randomPoint;
    }

    // デバッグ表示用のGizmos (エディタ専用)
    void OnDrawGizmosSelected()
    {
        // _sweetContentが設定されていないか、頂点数が不正な場合は描画しない
        if (_sweetContent == null || _spawnAreaVertices == null || _spawnAreaVertices.Length != 4)
        {
            return;
        }

        // ギズモの色を設定
        Gizmos.color = Color.cyan;

        // _sweetContentを基準としたワールド座標に変換してGizmosを描画
        // _spawnAreaVerticesは_sweetContentのローカル座標として扱われるため、
        // Gizmosでワールド座標として表示するにはTransformPointを使用します。
        Vector3 v0_world = _sweetContent.TransformPoint(_spawnAreaVertices[0]);
        Vector3 v1_world = _sweetContent.TransformPoint(_spawnAreaVertices[1]);
        Vector3 v2_world = _sweetContent.TransformPoint(_spawnAreaVertices[2]);
        Vector3 v3_world = _sweetContent.TransformPoint(_spawnAreaVertices[3]);

        // 4頂点を結ぶ線を描画
        Gizmos.DrawLine(v0_world, v1_world);
        Gizmos.DrawLine(v1_world, v2_world);
        Gizmos.DrawLine(v2_world, v3_world);
        Gizmos.DrawLine(v3_world, v0_world);

        // 各頂点を強調表示する球を描画
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(v0_world, 0.1f);
        Gizmos.DrawSphere(v1_world, 0.1f);
        Gizmos.DrawSphere(v2_world, 0.1f);
        Gizmos.DrawSphere(v3_world, 0.1f);
    }
}