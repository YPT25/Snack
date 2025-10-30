using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Mirror;

/// <summary>
/// Sweet�I�u�W�F�N�g�𐶐��E�Ǘ�����I�u�W�F�N�g�v�[��
/// �ePrefab���Ƃɐ����m����ݒ�ł��A���v�m���������Ő��K��
/// Mirror�l�b�g���[�N��œ��������Sweet�������I�ɍė��p
/// </summary>
public class SweetObjectPool : NetworkBehaviour
{
    [System.Serializable]
    public struct SweetPrefabData
    {
        public GameObject prefab;          // ��������Prefab
        [Range(0f, 1f)]
        public float spawnProbability;     // �����m���i���v�͎����Ő��K������܂��j
    }

    [Header("�������錳�Ɗm��")]
    [Tooltip("SweetPrefab�Ƃ��ꂼ��̐����m����ݒ�")]
    [SerializeField] private List<SweetPrefabData> _sweetPrefabs;

    [Header("��������ꏊ")]
    [Tooltip("Sweet�𐶐�����eTransform")]
    [SerializeField] private Transform _sweetContent;

    [Header("Sweet�����Ԋu")]
    [SerializeField] private float _spawnInterval = 2f;

    [Header("�I�u�W�F�N�g�v�[��")]
    private ObjectPool<GameObject> _pool;

    private float _timer;

    [Header("Sweet�����͈͂̒��_(X-Z����)")]
    [Tooltip("�l�p�`��4���_�����[�J�����W�Őݒ�")]
    [SerializeField] private Vector3[] _spawnAreaVertices = new Vector3[4];

    // ���K�����ꂽ�m�����X�g
    private List<float> _normalizedProbabilities;

    public override void OnStartServer()
    {
        base.OnStartServer();

        if (_sweetContent == null)
        {
            _sweetContent = transform;
            Debug.LogWarning($"'_sweetContent' not set. Using '{_sweetContent.name}' as default.");
        }

        // �m���𐳋K��
        NormalizeProbabilities();

        // �v�[��������
        _pool = new ObjectPool<GameObject>(
            OnCreatePoolObject,
            OnGetFromPool,
            OnReleaseToPool,
            OnDestroyPooledObject,
            defaultCapacity: 10,
            maxSize: 100
        );

        _timer = _spawnInterval;

        // ���_�`�F�b�N
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

    [ServerCallback]
    void Update()
    {
        if (!isServer) return;

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            SpawnSweet();
            _timer = _spawnInterval;
        }
    }

    /// <summary>
    /// �ePrefab�̊m�������v1�ɐ��K��
    /// </summary>
    private void NormalizeProbabilities()
    {
        _normalizedProbabilities = new List<float>();
        float total = 0f;
        foreach (var data in _sweetPrefabs)
            total += data.spawnProbability;

        if (total <= 0f)
        {
            Debug.LogWarning("Total probability is zero or negative. Using equal probabilities.");
            float equal = 1f / _sweetPrefabs.Count;
            for (int i = 0; i < _sweetPrefabs.Count; i++)
                _normalizedProbabilities.Add(equal);
            return;
        }

        foreach (var data in _sweetPrefabs)
            _normalizedProbabilities.Add(data.spawnProbability / total);
    }

    /// <summary>
    /// �m���ɉ�����Prefab��I��
    /// </summary>
    /// <returns>�I�΂ꂽPrefab</returns>
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

        // ����̂��ߍŌ��Ԃ�
        return _sweetPrefabs[_sweetPrefabs.Count - 1].prefab;
    }

    public GameObject OnCreatePoolObject()
    {
        GameObject prefabToSpawn = SelectPrefabByProbability();
        GameObject sweetObject = Instantiate(prefabToSpawn);
        sweetObject.SetActive(false);
        return sweetObject;
    }

    public void OnGetFromPool(GameObject target)
    {
        target.SetActive(true);
    }

    public void OnReleaseToPool(GameObject target)
    {
        target.SetActive(false);
        target.transform.SetParent(null);
    }

    public void OnDestroyPooledObject(GameObject target)
    {
        if (target != null && target.TryGetComponent<NetworkIdentity>(out NetworkIdentity ni) && ni.isServer)
            NetworkServer.Destroy(target);
        else
            Destroy(target);
    }

    private void SpawnSweet()
    {
        GameObject sweet = _pool.Get();
        sweet.transform.SetParent(_sweetContent);

        Vector3 randomLocalPosition = GetRandomPointInQuadXZ();
        sweet.transform.localPosition = randomLocalPosition;
        sweet.transform.localRotation = Quaternion.identity;
        sweet.transform.localScale = Vector3.one;

        NetworkServer.Spawn(sweet);

        // �N���C�A���g���ɐe��ݒ肷��RPC
        NetworkIdentity parentIdentity = _sweetContent.GetComponent<NetworkIdentity>();
        if (parentIdentity != null)
        {
            SweetParentSetter parentSetter = sweet.GetComponent<SweetParentSetter>();
            if (parentSetter != null)
                parentSetter.RpcSetParent(parentIdentity);
        }
    }

    public void ReleaseSweet(GameObject sweetToRelease)
    {
        if (sweetToRelease == null) return;
        _pool.Release(sweetToRelease);
    }

    private Vector3 GetRandomPointInQuadXZ()
    {
        float r1 = Random.value;
        float r2 = Random.value;
        Vector3 point;

        if (r1 + r2 < 1)
            point = _spawnAreaVertices[0] + r1 * (_spawnAreaVertices[1] - _spawnAreaVertices[0]) + r2 * (_spawnAreaVertices[2] - _spawnAreaVertices[0]);
        else
        {
            float r1p = 1 - r1;
            float r2p = 1 - r2;
            point = _spawnAreaVertices[3] + r1p * (_spawnAreaVertices[2] - _spawnAreaVertices[3]) + r2p * (_spawnAreaVertices[0] - _spawnAreaVertices[3]);
        }

        point.y = _sweetContent.InverseTransformPoint(_sweetContent.position).y;
        return point;
    }

    void OnDrawGizmosSelected()
    {
        if (_sweetContent == null || _spawnAreaVertices == null || _spawnAreaVertices.Length != 4) return;

        Gizmos.color = Color.cyan;
        Vector3 v0 = _sweetContent.TransformPoint(_spawnAreaVertices[0]);
        Vector3 v1 = _sweetContent.TransformPoint(_spawnAreaVertices[1]);
        Vector3 v2 = _sweetContent.TransformPoint(_spawnAreaVertices[2]);
        Vector3 v3 = _sweetContent.TransformPoint(_spawnAreaVertices[3]);

        Gizmos.DrawLine(v0, v1);
        Gizmos.DrawLine(v1, v2);
        Gizmos.DrawLine(v2, v3);
        Gizmos.DrawLine(v3, v0);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(v0, 0.1f);
        Gizmos.DrawSphere(v1, 0.1f);
        Gizmos.DrawSphere(v2, 0.1f);
        Gizmos.DrawSphere(v3, 0.1f);
    }
}
