using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Mirror;

/// <summary>
/// Sweet�I�u�W�F�N�g�𐶐��E�Ǘ����邽�߂̃I�u�W�F�N�g�v�[���B
/// Mirror�l�b�g���[�N��œ��������Sweet�������I�ɍė��p����B
/// </summary>
public class SweetObjectPool : NetworkBehaviour
{
    [Header("�������錳")]
    [SerializeField] private GameObject _sweetPrefab;
    [Header("��������ꏊ (���̃X�N���v�g���A�^�b�`����Ă���I�u�W�F�N�g)")]
    [Tooltip("Sweet�𐶐�����ۂ̐e�ƂȂ�Transform�B�ʏ�A���̃X�N���v�g���A�^�b�`����Ă���I�u�W�F�N�g��ݒ肵�܂��B")]
    [SerializeField] private Transform _sweetContent;
    [Header("Sweet�𐶐�����Ԋu")]
    [SerializeField] private float _spawnInterval = 2f;

    [Header("�I�u�W�F�N�g�v�[��")]
    private ObjectPool<GameObject> _pool;

    // Sweet�̐����^�C�~���O���v�邽�߂̃^�C�}�[
    private float _timer;

    [Header("Sweet�����͈͂̒��_ (X-Z����)")]
    [Tooltip("Sweet�����������X-Z���ʏ��4�̒��_���W��ݒ肵�܂��BY���W�͖�������܂��B" +
             "�����̍��W��'_sweetContent'����̑��ΓI�ȃ��[�J�����W�Ƃ��Ĉ����܂��B")]
    [SerializeField] private Vector3[] _spawnAreaVertices = new Vector3[4];

    // Start�̓T�[�o�[�ł̂ݎ��s�����悤�ɂ���
    public override void OnStartServer()
    {
        base.OnStartServer(); // �e�N���X��OnStartServer���Ăяo��

        // _sweetContent�����ݒ�̏ꍇ�A���̃X�N���v�g���A�^�b�`����Ă���Transform���g�p
        if (_sweetContent == null)
        {
            _sweetContent = this.transform;
            Debug.LogWarning($"'_sweetContent' was not set. Using '{_sweetContent.name}' (this GameObject's transform) as default.");
        }

        // Unity��ObjectPool�̏�����
        _pool = new ObjectPool<GameObject>(
            OnCreatePoolObject,          // �Q�[���I�u�W�F�N�g�̐��������̊֐� (�T�[�o�[�̂�)
            OnGetFromPool,               // �I�u�W�F�N�g�v�[������Q�[���I�u�W�F�N�g���擾����֐� (�T�[�o�[�̂�)
            OnReleaseToPool,             // �Q�[���I�u�W�F�N�g���I�u�W�F�N�g�v�[���ɕԋp���鏈���֐� (�T�[�o�[�̂�)
            OnDestroyPooledObject,       // �Q�[���I�u�W�F�N�g���폜���鏈���̊֐� (�T�[�o�[�̂�)
            defaultCapacity: 10,         // �����e�� (�C��)
            maxSize: 100                 // �ő�T�C�Y (�C��)
        );

        _timer = _spawnInterval; // ���񐶐��̂��߂Ƀ^�C�}�[��������

        // ���_���ݒ肳��Ă��邩�`�F�b�N
        if (_spawnAreaVertices == null || _spawnAreaVertices.Length != 4)
        {
            Debug.LogError("SpawnAreaVertices must contain exactly 4 vertices. Please set them in the Inspector. " +
                           "Using default square range [-5,5] in X and Z.");
            // �f�t�H���g�͈̔͂�ݒ肵�Ă��� (�t�H�[���o�b�N)
            _spawnAreaVertices = new Vector3[]
            {
                new Vector3(-5, 0, -5), // bottom-left
                new Vector3(5, 0, -5),  // bottom-right
                new Vector3(5, 0, 5),   // top-right
                new Vector3(-5, 0, 5)   // top-left
            };
        }
    }

    // Update�̓T�[�o�[�ł̂ݎ��s�����悤�ɂ���
    [ServerCallback] // ���̃��\�b�h���T�[�o�[��ł̂݌Ăяo����邱�Ƃ�ۏ�
    void Update()
    {
        // �T�[�o�[��ł̂�Sweet�̐������W�b�N�����s
        if (!isServer) return; // �O�̂��߁A�T�[�o�[�łȂ���Ώ������Ȃ�

        _timer -= Time.deltaTime; // �^�C�}�[������������

        if (_timer <= 0)
        {
            SpawnSweet(); // Sweet�𐶐�����
            _timer = _spawnInterval; // �^�C�}�[�����Z�b�g
        }
    }

    /// <summary>
    /// �I�u�W�F�N�g�v�[���p�̃Q�[���I�u�W�F�N�g���������̊֐��B
    /// ���̊֐��̓T�[�o�[���ŃI�u�W�F�N�g���s�������ۂɌĂяo�����B
    /// </summary>
    /// <returns>�������ꂽGameObject</returns>
    public GameObject OnCreatePoolObject()
    {
        // �����ł͂܂��e��ݒ肵�܂���B�X�|�[����ARPC�ŃN���C�A���g�ɂ��e�𓯊������܂��B
        // Instantiate�̓T�[�o�[���Ŏ��s����܂��B
        GameObject sweetObject = Instantiate(_sweetPrefab);
        // ��������Sweet�I�u�W�F�N�g�͍ŏ��͔�A�N�e�B�u�ȏ�Ԃ�Pool�Ɋi�[�����悤�ɂ���
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
        target.transform.SetParent(null); // �e������
    }

    /// <summary>
    /// �Q�[���I�u�W�F�N�g�����S�ɍ폜���鏈���̊֐��B
    /// �I�u�W�F�N�g�v�[���������ς��ɂȂ����ꍇ��A�����I�ȍ폜�v�����������ꍇ�ɌĂяo�����B
    /// </summary>
    /// <param name="target">�폜����GameObject</param>
    public void OnDestroyPooledObject(GameObject target)
    {
        // Mirror��NetworkServer.Spawn���ꂽ�I�u�W�F�N�g��Destroy����ۂ́A
        // NetworkServer.Destroy ���g�p���ăl�b�g���[�N����I�u�W�F�N�g���폜���܂��B
        // ����́APooling�������Ɋ��S�ɃI�u�W�F�N�g���폜����ꍇ�ɏd�v�ł��B
        // �������A�ʏ�Pooling�ł�DestroyPooledObject���Ă΂��̂͋H�ȃP�[�X�ł��B
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
    /// Sweet�𐶐����A�I�u�W�F�N�g�v�[������擾���ăl�b�g���[�N�ɃX�|�[������֐��B
    /// </summary>
    private void SpawnSweet()
    {
        // �I�u�W�F�N�g�v�[������Sweet�I�u�W�F�N�g���擾
        GameObject sweet = _pool.Get();

        // �T�[�o�[���ŁA�ꎞ�I�ɐe��ݒ肵�A���W�𒲐�����B
        // ����SetParent�́A�N���C�A���g�ɂ͒��ړ�������Ȃ����߁ARPC�ŕʓr�ʒm���܂��B
        sweet.transform.SetParent(_sweetContent);

        // 4�̒��_�Œ�`�����X-Z���ʂ̎l�p�`���̃����_���ȃ��[�J�����W���擾���܂��B
        // ���̊֐��̓����ŁAY���W�� _sweetContent �̃��[�J��Y���W�ɌŒ肳��܂��B
        Vector3 randomLocalPosition = GetRandomPointInQuadXZ();

        // �擾���������_���ȃ��[�J�����W��Sweet��localPosition�ɐݒ�
        sweet.transform.localPosition = randomLocalPosition;
        sweet.transform.localRotation = Quaternion.identity; // ��]�����Z�b�g
        sweet.transform.localScale = Vector3.one; // �X�P�[�������Z�b�g (�K�v�ɉ�����)

        // �K�v�ł���΁A������Sweet�̃R���|�[�l���g�ɃA�N�Z�X���ď����ݒ���s���܂�
        // ��: sweet.GetComponent<SweetData>().Initialize(Color.red);

        // �I�u�W�F�N�g���l�b�g���[�N��ɃX�|�[�����܂��B
        // ����ɂ��A�N���C�A���g���ł�����Sweet�I�u�W�F�N�g����������A
        // ����Transform����������n�߂܂��B
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
                Debug.LogWarning($"Sweet prefab '{_sweetPrefab.name}' is missing SweetParentSetter component. Cannot set parent on clients.");
            }
        }
        else
        {
            // _sweetContent��NetworkIdentity���Ȃ��ꍇ�ł��A��{�I�Ȑ����͉\�ł��B
            // �������A�N���C�A���g����Sweet���e�̉��ɊK�w������Ȃ����߁A
            // ���[���h���W�Ő�������Ă��܂��܂��B
            Debug.LogWarning($"'_sweetContent' ({_sweetContent.name}) is missing NetworkIdentity component. " +
                             "Sweet objects will spawn in world space on clients if not manually parented.");
        }
    }

    /// <summary>
    /// Sweet���j�󂳂ꂽ��A�s�v�ɂȂ����肵���ۂɃv�[���ɖ߂����߂̊֐��B
    /// Sweet���g�̃X�N���v�g�Ȃǂ���Ăяo����邱�Ƃ�z��B
    /// </summary>
    /// <param name="sweetToRelease">�v�[���ɕԋp����Sweet GameObject</param>
    public void ReleaseSweet(GameObject sweetToRelease)
    {
        if (sweetToRelease == null) return;

        // �I�u�W�F�N�g�v�[���ɖ߂��ꍇ�ANetworkServer.UnSpawn�͒ʏ�s���܂���B
        // NetworkServer.UnSpawn �����s����ƁA�N���C�A���g������I�u�W�F�N�g�������Ă��܂��܂��B
        // �v�[���̖ړI�͍ė��p�Ȃ̂ŁA���S�Ƀl�b�g���[�N����폜����̂ł͂Ȃ��A
        // ��A�N�e�B�u�ɂ��ăv�[���ɕԋp���A����̗��p�ɔ����܂��B
        _pool.Release(sweetToRelease); // �I�u�W�F�N�g���v�[���ɕԋp
    }

    /// <summary>
    /// 4�̒��_�Œ�`�����X-Z���ʂ̎l�p�`�i�܂��͓ʎl�p�`�j���̃����_���ȃ��[�J�����W��Ԃ��B
    /// �Ԃ����Y���W�́A'_sweetContent'�̃��[�J��Y���W�ɌŒ肳��܂��B
    /// </summary>
    /// <returns>�l�p�`���̃����_���ȃ��[�J�����W</returns>
    private Vector3 GetRandomPointInQuadXZ()
    {
        // _spawnAreaVertices �� _sweetContent �̃��[�J�����W�Ƃ��ē��͂���邱�Ƃ�z�肵�Ă��܂��B
        // (�������[���h���W�Ƃ��ē��͂����ꍇ�́A���O�� _sweetContent.InverseTransformPoint() ��
        //  ���[�J�����W�ɕϊ�����K�v������܂����A����̓C���X�y�N�^�[����̓��͂Ȃ̂Ń��[�J����z�肵�܂�)

        // �l�p�`��2�̎O�p�`�ɕ������A�ǂ��炩�̎O�p�`���Ń����_���ȓ_�𐶐����܂��B
        // ���̕��@�́A�l�p�`���ʌ^�ł���ꍇ�ɍł��ψ�ȕ��z��񋟂��܂��B
        // ���_0, 1, 2, 3 �������v���܂��͎��v���ɏ����ǂ�����ł��邱�Ƃ�O��Ƃ��܂��B

        // �d�S���W�n (Barycentric Coordinates) �𗘗p���āA
        // �O�p�`���̃����_���ȓ_�������I�ɐ������܂��B
        float r1 = Random.value;
        float r2 = Random.value;

        Vector3 randomPoint;

        if (r1 + r2 < 1)
        {
            // ���_0, ���_1, ���_2 �ō\�������O�p�`�͈̔�
            randomPoint = _spawnAreaVertices[0] + r1 * (_spawnAreaVertices[1] - _spawnAreaVertices[0]) + r2 * (_spawnAreaVertices[2] - _spawnAreaVertices[0]);
        }
        else
        {
            // ���_0, ���_2, ���_3 �ō\�������O�p�`�͈̔�
            // �܂��́A���_3, ���_2, ���_0 �̏��ōl���� (1-r1, 1-r2 ���g�p)
            float r1Prime = 1 - r1;
            float r2Prime = 1 - r2;
            randomPoint = _spawnAreaVertices[3] + r1Prime * (_spawnAreaVertices[2] - _spawnAreaVertices[3]) + r2Prime * (_spawnAreaVertices[0] - _spawnAreaVertices[3]);
        }

        // Y���W��'_sweetContent'�̃��[�J��Y���W�ɌŒ肵�܂��B
        // _sweetContent�̎q�Ƃ���Sweet����������邽�߁A�e�̃��[�J��Y���W��K�p���邱�Ƃ�
        // �e�Ɠ��������iY���W�j��Sweet���z�u����܂��B
        // �ʏ�A_sweetContent�̃��[�J��Y���W��0�Ȃ̂ŁASweet��Y���W��0�ɂȂ�܂��B
        randomPoint.y = _sweetContent.InverseTransformPoint(_sweetContent.position).y;

        return randomPoint;
    }

    // �f�o�b�O�\���p��Gizmos (�G�f�B�^��p)
    void OnDrawGizmosSelected()
    {
        // _sweetContent���ݒ肳��Ă��Ȃ����A���_�����s���ȏꍇ�͕`�悵�Ȃ�
        if (_sweetContent == null || _spawnAreaVertices == null || _spawnAreaVertices.Length != 4)
        {
            return;
        }

        // �M�Y���̐F��ݒ�
        Gizmos.color = Color.cyan;

        // _sweetContent����Ƃ������[���h���W�ɕϊ�����Gizmos��`��
        // _spawnAreaVertices��_sweetContent�̃��[�J�����W�Ƃ��Ĉ����邽�߁A
        // Gizmos�Ń��[���h���W�Ƃ��ĕ\������ɂ�TransformPoint���g�p���܂��B
        Vector3 v0_world = _sweetContent.TransformPoint(_spawnAreaVertices[0]);
        Vector3 v1_world = _sweetContent.TransformPoint(_spawnAreaVertices[1]);
        Vector3 v2_world = _sweetContent.TransformPoint(_spawnAreaVertices[2]);
        Vector3 v3_world = _sweetContent.TransformPoint(_spawnAreaVertices[3]);

        // 4���_�����Ԑ���`��
        Gizmos.DrawLine(v0_world, v1_world);
        Gizmos.DrawLine(v1_world, v2_world);
        Gizmos.DrawLine(v2_world, v3_world);
        Gizmos.DrawLine(v3_world, v0_world);

        // �e���_�������\�����鋅��`��
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(v0_world, 0.1f);
        Gizmos.DrawSphere(v1_world, 0.1f);
        Gizmos.DrawSphere(v2_world, 0.1f);
        Gizmos.DrawSphere(v3_world, 0.1f);
    }
}