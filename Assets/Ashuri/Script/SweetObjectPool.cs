using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Mirror;

#if UNITY_EDITOR // UnityエディタでのみHandles APIを使用するために必要
using UnityEditor;
#endif

/// <summary>
/// お菓子オブジェクトを生成・管理するためのオブジェクトプール。
/// Mirrorネットワーク上で同期されるお菓子を効率的に再利用する。
/// </summary>
public class SweetObjectPool : NetworkBehaviour
{
    [Header("複製する元")]
    [SerializeField] private List<GameObject> _sweetPrefabs; // GameObjectのリストに変更
    [Header("複製する場所")]
    [SerializeField] private Transform _sweetContent;
    [Header("お菓子を生成する間隔")]
    [SerializeField] private float _spawnInterval = 2f;

    [Header("生成範囲設定")]
    [SerializeField] private float _spawnRadius = 5f; // 生成する円の半径

    [Header("オブジェクトプール")]
    // プレハブごとにプールを持つ必要があるため、Dictionaryを使用
    private Dictionary<GameObject, ObjectPool<GameObject>> _pools = new Dictionary<GameObject, ObjectPool<GameObject>>();

    // お菓子の生成タイミングを計るためのタイマー
    private float _timer;

    // Startはサーバーでのみ実行されるようにする
    public override void OnStartServer()
    {
        base.OnStartServer(); // 親クラスのOnStartServerを呼び出す

        // 各プレハブに対して個別のオブジェクトプールを初期化する
        foreach (GameObject prefab in _sweetPrefabs)
        {
            if (prefab == null)
            {
                // エラーメッセージを修正: "SweetPrefabs"
                Debug.LogWarning("SweetPrefabs list contains a null entry. Please check your inspector settings.");
                continue;
            }

            _pools.Add(prefab, new ObjectPool<GameObject>(
                () => OnCreatePoolObject(prefab), // 各プレハブに対応する生成処理を渡す
                OnGetFromPool,               // オブジェクトプールからゲームオブジェクトを取得する関数 (サーバーのみ)
                OnReleaseToPool,             // ゲームオブジェクトをオブジェクトプールに返却する処理関数 (サーバーのみ)
                OnDestroyPooledObject        // ゲームオブジェクトを削除する処理の関数 (サーバーのみ)
            ));
        }

        _timer = _spawnInterval; // 初回生成のためにタイマーを初期化
    }

    // Updateはサーバーでのみ実行されるようにする
    [ServerCallback] // このメソッドがサーバー上でのみ呼び出されることを保証
    void Update()
    {
        // サーバー上でのみお菓子の生成ロジックを実行
        if (!isServer) return; // 念のため、サーバーでなければ処理しない

        _timer -= Time.deltaTime; // タイマーを減少させる

        if (_timer <= 0)
        {
            SpawnSweet(); // お菓子を生成する (関数名を修正)
            _timer = _spawnInterval; // タイマーをリセット
        }
    }

    /// <summary>
    /// オブジェクトプール用のゲームオブジェクト生成処理の関数。
    /// この関数はサーバー側でオブジェクトが不足した際に呼び出される。
    /// 引数でどのプレハブから生成するかを指定できるように変更。
    /// </summary>
    /// <param name="prefab">生成元のプレハブ</param>
    /// <returns>生成されたGameObject</returns>
    public GameObject OnCreatePoolObject(GameObject prefab)
    {
        // ここではまだ親を設定しません。スポーン後、RPCでクライアントにも親を同期させます。
        // Instantiateはサーバー側で実行されます。
        GameObject sweetObject = Instantiate(prefab); // 引数のプレハブを使用
        // 生成したお菓子オブジェクトは最初は非アクティブな状態でPoolに格納されるようにする
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
        target.transform.SetParent(null);
    }

    /// <summary>
    /// ゲームオブジェクトを完全に削除する処理の関数。
    /// オブジェクトプールがいっぱいになった場合や、明示的な削除要求があった場合に呼び出される。
    /// </summary>
    /// <param name="target">削除するGameObject</param>
    public void OnDestroyPooledObject(GameObject target)
    {
        if (target != null && target.TryGetComponent<NetworkIdentity>(out NetworkIdentity ni) && ni.isServer)
        {
            NetworkServer.Destroy(target); // NetworkServer.Destroy を使用してネットワークからオブジェクトを削除
        }
        else if (target != null) // NetworkIdentityがない場合でもDestroyを呼ぶ
        {
            Destroy(target.gameObject);
        }
    }

    /// <summary>
    /// お菓子を生成し、オブジェクトプールから取得してネットワークにスポーンする関数。
    /// </summary>
    private void SpawnSweet() // 関数名を修正
    {
        if (_sweetPrefabs.Count == 0)
        {
            // エラーメッセージを修正: "SweetPrefabs"
            Debug.LogWarning("SweetPrefabs list is empty. Cannot spawn sweet.");
            return;
        }

        // お菓子プレハブのリストからランダムに1つ選択
        GameObject selectedPrefab = _sweetPrefabs[Random.Range(0, _sweetPrefabs.Count)];
        if (selectedPrefab == null)
        {
            // エラーメッセージを修正: "sweet prefab"
            Debug.LogWarning("Selected sweet prefab is null. Skipping spawn.");
            return;
        }

        // 選択されたプレハブに対応するオブジェクトプールからお菓子オブジェクトを取得
        GameObject sweet = _pools[selectedPrefab].Get(); // 変数名を修正

        // 円形範囲内のランダムな位置を計算
        // _sweetContentの位置を中心に、_spawnRadiusの半径で生成
        Vector2 randomCirclePoint = Random.insideUnitCircle * _spawnRadius;
        Vector3 spawnPosition = _sweetContent.position + new Vector3(randomCirclePoint.x, randomCirclePoint.y, 0f);


        sweet.transform.SetParent(_sweetContent); // _sweetContentを親に設定
        sweet.transform.position = spawnPosition; // 計算した位置にスポーン
        sweet.transform.localRotation = Quaternion.identity; // 回転もリセット
        sweet.transform.localScale = Vector3.one; // スケールもリセット (必要に応じて)

        // ここでお菓子の初期設定を行う場合は、そのコンポーネントにアクセスして設定
        // 例: sweet.GetComponent<SweetData>().Initialize();

        // オブジェクトをネットワーク上にスポーンする。
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
                // エラーメッセージを修正: "SweetParentSetter"
                Debug.LogWarning($"Sweet prefab '{selectedPrefab.name}' is missing SweetParentSetter component. Cannot set parent on clients.");
            }
        }
        else
        {
            // エラーメッセージを修正: "_sweetContent"
            Debug.LogWarning($"'_sweetContent' ({_sweetContent.name}) is missing NetworkIdentity component. Cannot synchronize parent via RPC.");
        }
    }

    /// <summary>
    /// お菓子が破壊されたり、不要になったりした際にプールに戻すための関数。
    /// お菓子自身のスクリプトなどから呼び出されることを想定。
    /// </summary>
    /// <param name="sweetToRelease">プールに返却するお菓子GameObject</param>
    public void ReleaseSweet(GameObject sweetToRelease) // 関数名と引数名を修正
    {
        if (sweetToRelease == null) return;

        // 返却するオブジェクトの元のプレハブを見つける必要がある
        GameObject originalPrefab = null;
        foreach (GameObject prefab in _sweetPrefabs)
        {
            if (prefab != null && sweetToRelease.name.Contains(prefab.name)) // 名前の部分一致で判定 (厳密にはGUIDなどで判定すべきだが、簡易的に)
            {
                originalPrefab = prefab;
                break;
            }
        }

        if (originalPrefab != null && _pools.ContainsKey(originalPrefab))
        {
            _pools[originalPrefab].Release(sweetToRelease); // 対応するプールに返却
        }
        else
        {
            Debug.LogWarning($"Could not find original prefab for {sweetToRelease.name} to release it to a pool. Destroying instead.");
            // プールが見つからない場合は、最終手段としてオブジェクトを破棄
            if (sweetToRelease.TryGetComponent<NetworkIdentity>(out NetworkIdentity ni) && ni.isServer)
            {
                NetworkServer.Destroy(sweetToRelease);
            }
            else
            {
                Destroy(sweetToRelease);
            }
        }
    }

    // =========================================================================
    // エディタ専用の描画・調整機能 (GizmosとHandles)
    // =========================================================================

    // Sceneビューで常に円を描画
    void OnDrawGizmos()
    {
        // _sweetContentが設定されていない場合は描画しない
        if (_sweetContent == null) return;

        // 常に描画される円の色を設定
        Gizmos.color = Color.yellow;
        // _sweetContentの位置を中心に、_spawnRadiusの半径でワイヤーフレームの球（円）を描画
        // ここではZ軸を無視してXY平面に描画されるようにします
        Gizmos.DrawWireSphere(_sweetContent.position, _spawnRadius);
    }

    // コンポーネクトが選択されたときにのみ、半径調整ハンドルと強調された円を描画
    void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR // Handles APIはエディタ専用のため、#if UNITY_EDITORで囲む

        // _sweetContentが設定されていない場合は描画しない
        if (_sweetContent == null) return;

        // 選択時に描画される円の色を設定
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_sweetContent.position, _spawnRadius);

        // Handles.RadiusHandle を使用して、Sceneビューで半径をドラッグで調整できるようにする
        // _sweetContent.position がハンドルの中心点
        // _spawnRadius が現在の半径
        // Quaternion.identity はハンドルの向き（ここでは回転なし）
        // Handles.RadiusHandle は調整後の新しい半径を返す
        _spawnRadius = Handles.RadiusHandle(Quaternion.identity, _sweetContent.position, _spawnRadius);

        // 半径が負の値にならないようにクランプ
        _spawnRadius = Mathf.Max(0.1f, _spawnRadius); // 最低半径を0.1fに設定

#endif
    }
}