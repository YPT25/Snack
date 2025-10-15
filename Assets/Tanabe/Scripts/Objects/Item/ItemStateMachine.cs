using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemStateMachine : MonoBehaviour
{
    public enum ItemType
    {
        NONE_TYPE,
        POINT,
        USE,
        THROW,
        TRAP,
        TRAP_BOMB,
        BUFF,
        SETPART,
    }

    // ���݂̃X�e�[�g
    private IItemState_Tanabe currentState;
    [Header("�A�C�e���̎��"), SerializeField] private ItemType m_itemType;
    [Header("�ړ����x ���f�t�H���g�l:30.0"), SerializeField, Range(0f, 100f)] public float moveSpeed;
    [Header("��]���x ���f�t�H���g�l:30.0"), SerializeField, Range(0f, 100f)] public float rotateSpeed;
    [Header("�|�C���g"), SerializeField, Range(0f, 100f)] private float m_point;

    private Rigidbody m_rb = null;
    private Collider m_collider = null;
    private Transform playerTransform = null;
    private Player_Tanabe m_playerData;

    [Header("�o�t�̎��"), SerializeField] private BuffManager_Tanabe.Buff.BuffType m_buffType;

    private GameObject m_effectObject;

    // �J�n�֐�
    void Start()
    {
        m_rb = GetComponent<Rigidbody>();
        m_collider = GetComponent<Collider>();

        // �����̃X�e�[�g�̐ݒ�
        ChangeState(new DropState(this));

        if(m_itemType == ItemType.THROW)
        {
            // �v���n�u��GameObject�^�Ŏ擾
            GameObject obj = (GameObject)Resources.Load("Explosion_2_Bomb_Yellow");

            m_effectObject = Instantiate(obj);
            m_effectObject.transform.parent = this.transform;
            m_effectObject.SetActive(false);
            m_effectObject.transform.localPosition = new Vector3(0f, 1f, 0f);
        }
        else if(m_itemType == ItemType.TRAP_BOMB)
        {
            // �v���n�u��GameObject�^�Ŏ擾
            GameObject obj = (GameObject)Resources.Load("Explosion_2_Bomb_Purple");

            m_effectObject = Instantiate(obj);
            m_effectObject.transform.parent = this.transform;
            m_effectObject.SetActive(false);
            //m_effectObject.transform.localPosition = new Vector3(0f, 2f, 0f);
            m_effectObject.transform.localPosition = new Vector3(0f, 0f, 0f);
        }
    }

    // �X�V�֐�
    void Update()
    {
        currentState?.Update();
    }

    // ���݂̃X�e�[�g�̕ύX
    public void ChangeState(IItemState_Tanabe newState)
    {
        // ���݂̃X�e�[�g�̏I������
        currentState?.Exit();
        // �V���ȃX�e�[�g�̐ݒ�
        currentState = newState;
        // �V���ȃX�e�[�g�̊J�n����
        currentState.Enter();
    }

    // isTrigger�Փ˔���
    private void OnTriggerEnter(Collider other)
    {
        // ���݂̃X�e�[�g��isTrigger�Փ˂��N�������Ƃ�ʒm����
        currentState?.OnTriggerEnter(other);
    }

    private void OnTriggerStay(Collider other)
    {
        // ���݂̃X�e�[�g��isTrigger�Փ˂��N�������Ƃ�ʒm����
        currentState?.OnTriggerEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        // ���݂̃X�e�[�g��isTrigger�Փ˂��O�ꂽ���Ƃ�ʒm����
        currentState?.OnTriggerExit(other);
    }

    private void OnCollisionStay(Collision collision)
    {
        if(m_itemType != ItemType.TRAP && m_itemType != ItemType.TRAP_BOMB) { return; }
        // ���݂̃X�e�[�g�ɏՓ˂��N�������Ƃ�ʒm����
        currentState?.OnTriggerEnter(collision.collider);
    }

    private void OnCollisionExit(Collision collision)
    {
        if(m_itemType != ItemType.TRAP && m_itemType != ItemType.SETPART) { return; }
        // ���݂̃X�e�[�g�ɏՓ˂����ꂽ���Ƃ�ʒm����
        currentState?.OnCollisionExit(collision.collider);
    }

    // ���̃I�u�W�F�N�g��j������
    public void DestroysGameObject(GameObject _gameObject = null)
    {
        if(_gameObject == null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Destroy(_gameObject);
        }
    }

    private void OnDestroy()
    {
        if(m_itemType != ItemType.TRAP) { return; }
        // ���݂̃X�e�[�g�̏I������
        currentState?.Exit();
    }

    // ���̃A�C�e���̎�ނ̎擾
    public ItemType GetItemType()
    {
        return m_itemType;
    }

    public BuffManager_Tanabe.Buff.BuffType GetBuffType()
    {
        return m_buffType;
    }

    public Rigidbody GetRigidbody()
    {
        return m_rb;
    }

    public Collider GetColiider()
    {
        return m_collider;
    }

    // �|�C���g�̎擾
    public float GetPoint()
    {
        return m_point;
    }

    // �v���C���[�̃g�����X�t�H�[���̎擾
    public Transform GetPlayerTransform()
    {
        return playerTransform;
    }

    // �v���C���[�f�[�^�̎擾
    public Player_Tanabe GetPlayerData()
    {
        return m_playerData;
    }

    public GameObject GetEffectObject()
    {
        return m_effectObject;
    }


    // �v���C���[�̃g�����X�t�H�[���̐ݒ�
    public void SetPlayerTransform(Transform _transform)
    {
        if(playerTransform != null) { return; }
        playerTransform = _transform;
    }

    // �v���C���[�f�[�^�̐ݒ�
    public void SetPlayerData(Player_Tanabe _playerData)
    {
        m_playerData = _playerData;
        if(_playerData != null)
        {
            playerTransform = m_playerData.transform;
        }
        else
        {
            playerTransform = null;
        }
    }

    // �d�̗͂L���ݒ�
    public void SetUseGravity(bool flag)
    {
        if(m_rb == null) { return; }
        m_rb.useGravity = flag;
    }

    // �L�l�}�e�B�b�N�̐ݒ�
    public void SetIsKinematic(bool flag)
    {
        if (m_rb == null) { return; }
        m_rb.isKinematic = flag;
    }

}
