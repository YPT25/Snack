using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Mirror;

#if UNITY_EDITOR // Unity�G�f�B�^�ł̂�Handles API���g�p���邽�߂ɕK�v
using UnityEditor;
#endif

/// <summary>
/// ���َq�I�u�W�F�N�g�𐶐��E�Ǘ����邽�߂̃I�u�W�F�N�g�v�[���B
/// Mirror�l�b�g���[�N��œ�������邨�َq�������I�ɍė��p����B
/// </summary>
public class SweetObjectPool : NetworkBehaviour
{
    [Header("�������錳")]
    [SerializeField] private List<GameObject> _sweetPrefabs; // GameObject�̃��X�g�ɕύX
    [Header("��������ꏊ")]
    [SerializeField] private Transform _sweetContent;
    [Header("���َq�𐶐�����Ԋu")]
    [SerializeField] private float _spawnInterval = 2f;

    [Header("�����͈͐ݒ�")]
    [SerializeField] private float _spawnRadius = 5f; // ��������~�̔��a

    [Header("�I�u�W�F�N�g�v�[��")]
    // �v���n�u���ƂɃv�[�������K�v�����邽�߁ADictionary���g�p
    private Dictionary<GameObject, ObjectPool<GameObject>> _pools = new Dictionary<GameObject, ObjectPool<GameObject>>();

    // ���َq�̐����^�C�~���O���v�邽�߂̃^�C�}�[
    private float _timer;

    // Start�̓T�[�o�[�ł̂ݎ��s�����悤�ɂ���
    public override void OnStartServer()
    {
        base.OnStartServer(); // �e�N���X��OnStartServer���Ăяo��

        // �e�v���n�u�ɑ΂��Čʂ̃I�u�W�F�N�g�v�[��������������
        foreach (GameObject prefab in _sweetPrefabs)
        {
            if (prefab == null)
            {
                // �G���[���b�Z�[�W���C��: "SweetPrefabs"
                Debug.LogWarning("SweetPrefabs list contains a null entry. Please check your inspector settings.");
                continue;
            }

            _pools.Add(prefab, new ObjectPool<GameObject>(
                () => OnCreatePoolObject(prefab), // �e�v���n�u�ɑΉ����鐶��������n��
                OnGetFromPool,               // �I�u�W�F�N�g�v�[������Q�[���I�u�W�F�N�g���擾����֐� (�T�[�o�[�̂�)
                OnReleaseToPool,             // �Q�[���I�u�W�F�N�g���I�u�W�F�N�g�v�[���ɕԋp���鏈���֐� (�T�[�o�[�̂�)
                OnDestroyPooledObject        // �Q�[���I�u�W�F�N�g���폜���鏈���̊֐� (�T�[�o�[�̂�)
            ));
        }

        _timer = _spawnInterval; // ���񐶐��̂��߂Ƀ^�C�}�[��������
    }

    // Update�̓T�[�o�[�ł̂ݎ��s�����悤�ɂ���
    [ServerCallback] // ���̃��\�b�h���T�[�o�[��ł̂݌Ăяo����邱�Ƃ�ۏ�
    void Update()
    {
        // �T�[�o�[��ł݂̂��َq�̐������W�b�N�����s
        if (!isServer) return; // �O�̂��߁A�T�[�o�[�łȂ���Ώ������Ȃ�

        _timer -= Time.deltaTime; // �^�C�}�[������������

        if (_timer <= 0)
        {
            SpawnSweet(); // ���َq�𐶐����� (�֐������C��)
            _timer = _spawnInterval; // �^�C�}�[�����Z�b�g
        }
    }

    /// <summary>
    /// �I�u�W�F�N�g�v�[���p�̃Q�[���I�u�W�F�N�g���������̊֐��B
    /// ���̊֐��̓T�[�o�[���ŃI�u�W�F�N�g���s�������ۂɌĂяo�����B
    /// �����łǂ̃v���n�u���琶�����邩���w��ł���悤�ɕύX�B
    /// </summary>
    /// <param name="prefab">�������̃v���n�u</param>
    /// <returns>�������ꂽGameObject</returns>
    public GameObject OnCreatePoolObject(GameObject prefab)
    {
        // �����ł͂܂��e��ݒ肵�܂���B�X�|�[����ARPC�ŃN���C�A���g�ɂ��e�𓯊������܂��B
        // Instantiate�̓T�[�o�[���Ŏ��s����܂��B
        GameObject sweetObject = Instantiate(prefab); // �����̃v���n�u���g�p
        // �����������َq�I�u�W�F�N�g�͍ŏ��͔�A�N�e�B�u�ȏ�Ԃ�Pool�Ɋi�[�����悤�ɂ���
        sweetObject.SetActive(false);
        return sweetObject;
    }

    /// <summary>
    /// �I�u�W�F�N�g�v�[������Q�[���I�u�W�F�N�g���擾���鏈���̊֐��B
    /// �A�N�e�B�u���Ə����ݒ肪�s����B
    /// </summary>
    /// <param name="target">�v�[������擾����GameObject</param>
    public void OnGetFromPool(GameObject target)
    {
        target.gameObject.SetActive(true); // �I�u�W�F�N�g���A�N�e�B�u�ɂ���
        // �����ł͂܂��e��ݒ肵�܂���BSpawnSweet()���Őݒ肵�A�N���C�A���g�ɓ������܂��B
    }

    /// <summary>
    /// �Q�[���I�u�W�F�N�g���I�u�W�F�N�g�v�[���ɕԋp���鏈���̊֐��B
    /// ��A�N�e�B�u���ƃN���[���A�b�v���s����B
    /// </summary>
    /// <param name="target">�v�[���ɕԋp����GameObject</param>
    public void OnReleaseToPool(GameObject target)
    {
        // �I�u�W�F�N�g���A�N�e�B�u�ɂ���
        target.gameObject.SetActive(false);
        // �v�[���ɖ߂��ۂɁA�I�u�W�F�N�g�̐e���������Ă������ƂŁA
        // ���Ɏ擾���ꂽ�Ƃ��ɃN���[���ȏ�Ԃ���X�^�[�g�ł���悤�ɂ���B
        target.transform.SetParent(null);
    }

    /// <summary>
    /// �Q�[���I�u�W�F�N�g�����S�ɍ폜���鏈���̊֐��B
    /// �I�u�W�F�N�g�v�[���������ς��ɂȂ����ꍇ��A�����I�ȍ폜�v�����������ꍇ�ɌĂяo�����B
    /// </summary>
    /// <param name="target">�폜����GameObject</param>
    public void OnDestroyPooledObject(GameObject target)
    {
        if (target != null && target.TryGetComponent<NetworkIdentity>(out NetworkIdentity ni) && ni.isServer)
        {
            NetworkServer.Destroy(target); // NetworkServer.Destroy ���g�p���ăl�b�g���[�N����I�u�W�F�N�g���폜
        }
        else if (target != null) // NetworkIdentity���Ȃ��ꍇ�ł�Destroy���Ă�
        {
            Destroy(target.gameObject);
        }
    }

    /// <summary>
    /// ���َq�𐶐����A�I�u�W�F�N�g�v�[������擾���ăl�b�g���[�N�ɃX�|�[������֐��B
    /// </summary>
    private void SpawnSweet() // �֐������C��
    {
        if (_sweetPrefabs.Count == 0)
        {
            // �G���[���b�Z�[�W���C��: "SweetPrefabs"
            Debug.LogWarning("SweetPrefabs list is empty. Cannot spawn sweet.");
            return;
        }

        // ���َq�v���n�u�̃��X�g���烉���_����1�I��
        GameObject selectedPrefab = _sweetPrefabs[Random.Range(0, _sweetPrefabs.Count)];
        if (selectedPrefab == null)
        {
            // �G���[���b�Z�[�W���C��: "sweet prefab"
            Debug.LogWarning("Selected sweet prefab is null. Skipping spawn.");
            return;
        }

        // �I�����ꂽ�v���n�u�ɑΉ�����I�u�W�F�N�g�v�[�����炨�َq�I�u�W�F�N�g���擾
        GameObject sweet = _pools[selectedPrefab].Get(); // �ϐ������C��

        // �~�`�͈͓��̃����_���Ȉʒu���v�Z
        // _sweetContent�̈ʒu�𒆐S�ɁA_spawnRadius�̔��a�Ő���
        Vector2 randomCirclePoint = Random.insideUnitCircle * _spawnRadius;
        Vector3 spawnPosition = _sweetContent.position + new Vector3(randomCirclePoint.x, randomCirclePoint.y, 0f);


        sweet.transform.SetParent(_sweetContent); // _sweetContent��e�ɐݒ�
        sweet.transform.position = spawnPosition; // �v�Z�����ʒu�ɃX�|�[��
        sweet.transform.localRotation = Quaternion.identity; // ��]�����Z�b�g
        sweet.transform.localScale = Vector3.one; // �X�P�[�������Z�b�g (�K�v�ɉ�����)

        // �����ł��َq�̏����ݒ���s���ꍇ�́A���̃R���|�[�l���g�ɃA�N�Z�X���Đݒ�
        // ��: sweet.GetComponent<SweetData>().Initialize();

        // �I�u�W�F�N�g���l�b�g���[�N��ɃX�|�[������B
        NetworkServer.Spawn(sweet);

        // �N���C�A���g���Őe��ݒ肷�邽�߂�RPC���Ăяo���B
        // _sweetContent��NetworkIdentity�������Ă���K�v������B
        NetworkIdentity parentIdentity = _sweetContent.GetComponent<NetworkIdentity>();
        if (parentIdentity != null)
        {
            SweetParentSetter parentSetter = sweet.GetComponent<SweetParentSetter>();
            if (parentSetter != null)
            {
                // �N���C�A���g��SweetParentSetter�ɁA�e�ƂȂ�NetworkIdentity��n����RPC���Ăяo���B
                parentSetter.RpcSetParent(parentIdentity);
            }
            else
            {
                // �G���[���b�Z�[�W���C��: "SweetParentSetter"
                Debug.LogWarning($"Sweet prefab '{selectedPrefab.name}' is missing SweetParentSetter component. Cannot set parent on clients.");
            }
        }
        else
        {
            // �G���[���b�Z�[�W���C��: "_sweetContent"
            Debug.LogWarning($"'_sweetContent' ({_sweetContent.name}) is missing NetworkIdentity component. Cannot synchronize parent via RPC.");
        }
    }

    /// <summary>
    /// ���َq���j�󂳂ꂽ��A�s�v�ɂȂ����肵���ۂɃv�[���ɖ߂����߂̊֐��B
    /// ���َq���g�̃X�N���v�g�Ȃǂ���Ăяo����邱�Ƃ�z��B
    /// </summary>
    /// <param name="sweetToRelease">�v�[���ɕԋp���邨�َqGameObject</param>
    public void ReleaseSweet(GameObject sweetToRelease) // �֐����ƈ��������C��
    {
        if (sweetToRelease == null) return;

        // �ԋp����I�u�W�F�N�g�̌��̃v���n�u��������K�v������
        GameObject originalPrefab = null;
        foreach (GameObject prefab in _sweetPrefabs)
        {
            if (prefab != null && sweetToRelease.name.Contains(prefab.name)) // ���O�̕�����v�Ŕ��� (�����ɂ�GUID�ȂǂŔ��肷�ׂ������A�ȈՓI��)
            {
                originalPrefab = prefab;
                break;
            }
        }

        if (originalPrefab != null && _pools.ContainsKey(originalPrefab))
        {
            _pools[originalPrefab].Release(sweetToRelease); // �Ή�����v�[���ɕԋp
        }
        else
        {
            Debug.LogWarning($"Could not find original prefab for {sweetToRelease.name} to release it to a pool. Destroying instead.");
            // �v�[����������Ȃ��ꍇ�́A�ŏI��i�Ƃ��ăI�u�W�F�N�g��j��
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
    // �G�f�B�^��p�̕`��E�����@�\ (Gizmos��Handles)
    // =========================================================================

    // Scene�r���[�ŏ�ɉ~��`��
    void OnDrawGizmos()
    {
        // _sweetContent���ݒ肳��Ă��Ȃ��ꍇ�͕`�悵�Ȃ�
        if (_sweetContent == null) return;

        // ��ɕ`�悳���~�̐F��ݒ�
        Gizmos.color = Color.yellow;
        // _sweetContent�̈ʒu�𒆐S�ɁA_spawnRadius�̔��a�Ń��C���[�t���[���̋��i�~�j��`��
        // �����ł�Z���𖳎�����XY���ʂɕ`�悳���悤�ɂ��܂�
        Gizmos.DrawWireSphere(_sweetContent.position, _spawnRadius);
    }

    // �R���|�[�l�N�g���I�����ꂽ�Ƃ��ɂ̂݁A���a�����n���h���Ƌ������ꂽ�~��`��
    void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR // Handles API�̓G�f�B�^��p�̂��߁A#if UNITY_EDITOR�ň͂�

        // _sweetContent���ݒ肳��Ă��Ȃ��ꍇ�͕`�悵�Ȃ�
        if (_sweetContent == null) return;

        // �I�����ɕ`�悳���~�̐F��ݒ�
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_sweetContent.position, _spawnRadius);

        // Handles.RadiusHandle ���g�p���āAScene�r���[�Ŕ��a���h���b�O�Œ����ł���悤�ɂ���
        // _sweetContent.position ���n���h���̒��S�_
        // _spawnRadius �����݂̔��a
        // Quaternion.identity �̓n���h���̌����i�����ł͉�]�Ȃ��j
        // Handles.RadiusHandle �͒�����̐V�������a��Ԃ�
        _spawnRadius = Handles.RadiusHandle(Quaternion.identity, _sweetContent.position, _spawnRadius);

        // ���a�����̒l�ɂȂ�Ȃ��悤�ɃN�����v
        _spawnRadius = Mathf.Max(0.1f, _spawnRadius); // �Œᔼ�a��0.1f�ɐݒ�

#endif
    }
}