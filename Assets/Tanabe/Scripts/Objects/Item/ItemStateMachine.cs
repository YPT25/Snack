using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ItemStateMachine : NetworkBehaviour
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

    public enum ItemStateType
    {
        DROP,
        SUCK,
        HANDS,
        TRAP,
        PREPARINGTHROW,
        THROW,
        PARTEQUIPPED,
    }

    // ���݂̃X�e�[�g
    private IItemState_Tanabe currentState;
    [SyncVar, Header("�A�C�e���̎��"), SerializeField] private ItemType m_itemType;
    [Header("�ړ����x ���f�t�H���g�l:30.0"), SerializeField, Range(0f, 100f)] public float moveSpeed;
    [Header("��]���x ���f�t�H���g�l:30.0"), SerializeField, Range(0f, 100f)] public float rotateSpeed;
    [Header("�|�C���g"), SerializeField, Range(0f, 100f)] private float m_point;

    private Rigidbody m_rb = null;
    private Collider m_collider = null;
    [SyncVar] private Transform playerTransform = null;
    [SyncVar] private Player_Tanabe m_playerData;
    [SyncVar] private ItemStateType m_stateType;

    [Header("�o�t�̎��"), SerializeField] private BuffManager_Tanabe.Buff.BuffType m_buffType;

    [Header("�G�t�F�N�g�̃I�u�W�F�N�g"), SerializeField] private GameObject m_effectObject;

    // �J�n�֐�
    void Start()
    {
        m_rb = GetComponent<Rigidbody>();
        m_collider = GetComponent<Collider>();
        Debug.Log("Item_Start");
        // �����̃X�e�[�g�̐ݒ�
        ChangeState(this, ItemStateType.DROP);

        if(m_itemType == ItemType.THROW)
        {
            // �v���n�u��GameObject�^�Ŏ擾
            //GameObject obj = (GameObject)Resources.Load("Explosion_2_Bomb_Yellow");

            m_effectObject = Instantiate(m_effectObject);
            m_effectObject.transform.parent = this.transform;
            m_effectObject.SetActive(false);
            m_effectObject.transform.localPosition = new Vector3(0f, 1f, 0f);
        }
        else if(m_itemType == ItemType.TRAP_BOMB)
        {
            // �v���n�u��GameObject�^�Ŏ擾
            //GameObject obj = (GameObject)Resources.Load("Explosion_2_Bomb_Purple");

            m_effectObject = Instantiate(m_effectObject);
            m_effectObject.transform.parent = this.transform;
            m_effectObject.SetActive(false);
            //m_effectObject.transform.localPosition = new Vector3(0f, 2f, 0f);
            m_effectObject.transform.localPosition = new Vector3(0f, 0f, 0f);
        }
    }

    // �X�V�֐�
    [ServerCallback]
    void Update()
    {
        currentState?.Update();
    }

    [ClientRpc]
    public void RpcChangeState(ItemStateMachine _item, ItemStateType _newStateType)
    {
        //this.ChangeState(_item, _newStateType);
        if (_item != null)
        {
            _item.ChangeState(_item, _newStateType);
        }
    }

    [Command]
    public void CmdChangeState(ItemStateMachine _item, ItemStateType _newStateType)
    {
        //this.ChangeState(_item, _newStateType);
        if (_item != null)
        {
            // �T�[�o�[���ł���ԕύX
            _item.ChangeState(_item, _newStateType);

            // �N���C�A���g�S���ɔ��f
            RpcChangeState(_item, _newStateType);
        }
    }

    [ServerCallback]
    public void ServerChangeState(ItemStateMachine _item, ItemStateType _newStateType)
    {
        if (_item != null)
        {
            // �T�[�o�[���ł���ԕύX
            _item.ChangeState(_item, _newStateType);

            // �N���C�A���g�S���ɔ��f
            RpcChangeState(_item, _newStateType);
        }
    }

    // ���݂̃X�e�[�g�̕ύX
    public void ChangeState(ItemStateMachine _item, ItemStateType _newStateType)
    {
        m_stateType = _newStateType;
        switch(_newStateType)
        {
            case ItemStateType.DROP:
                this.ChangeState(new DropState(_item));
                break;
            case ItemStateType.SUCK:
                this.ChangeState(new SuckState(_item));
                break;
            case ItemStateType.HANDS:
                this.ChangeState(new HandsState(_item));
                break;
            case ItemStateType.TRAP:
                this.ChangeState(new TrapState(_item));
                break;
            case ItemStateType.PREPARINGTHROW:
                this.ChangeState(new PreparingThrowState(_item));
                break;
            case ItemStateType.THROW:
                this.ChangeState(new ThrowState(_item));
                break;
            case ItemStateType.PARTEQUIPPED:
                this.ChangeState(new PartEquippedState(_item));
                break;
            default:
                break;
        }
    }

    private void ChangeState(IItemState_Tanabe newState)
    {
        Debug.Log("Item_ChageState");
        // ���݂̃X�e�[�g�̏I������
        currentState?.Exit();
        // �V���ȃX�e�[�g�̐ݒ�
        currentState = newState;
        // �V���ȃX�e�[�g�̊J�n����
        currentState.Enter();
    }

    // isTrigger�Փ˔���
    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        // ���݂̃X�e�[�g��isTrigger�Փ˂��N�������Ƃ�ʒm����
        currentState?.OnTriggerEnter(other.gameObject);
    }

    [ServerCallback]
    private void OnTriggerStay(Collider other)
    {
        // ���݂̃X�e�[�g��isTrigger�Փ˂��N�������Ƃ�ʒm����
        currentState?.OnTriggerEnter(other.gameObject);
    }

    [ServerCallback]
    private void OnTriggerExit(Collider other)
    {
        // ���݂̃X�e�[�g��isTrigger�Փ˂��O�ꂽ���Ƃ�ʒm����
        currentState?.OnTriggerExit(other);
    }

    [ServerCallback]
    private void OnCollisionStay(Collision collision)
    {
        if (m_itemType != ItemType.TRAP && m_itemType != ItemType.TRAP_BOMB) { return; }
        // ���݂̃X�e�[�g��isTrigger�Փ˂��N�������Ƃ�ʒm����
        currentState?.OnTriggerEnter(collision.gameObject);
    }

    [ServerCallback]
    private void OnCollisionExit(Collision collision)
    {
        if (m_itemType != ItemType.TRAP && m_itemType != ItemType.SETPART) { return; }
        // ���݂̃X�e�[�g�ɏՓ˂����ꂽ���Ƃ�ʒm����
        currentState?.OnCollisionExit(collision.collider);
    }

    [ServerCallback]
    public void ServerAddPoint()
    {
        // �|�C���g�ɕϊ�����
        this.GetPlayerData().AddPoint(this.GetPoint());
        this.RpcAddPoint();
        // ���̃I�u�W�F�N�g��j������
        this.DestroysGameObject();
    }

    [ClientRpc]
    private void RpcAddPoint()
    {
        // �|�C���g�ɕϊ�����
        this.GetPlayerData().AddPoint(this.GetPoint());
        // ���̃I�u�W�F�N�g��j������
        this.DestroysGameObject();
    }

    [ClientRpc]
    public void RpcExplode()
    {
        this.SetIsKinematic(true);
        this.GetEffectObject().SetActive(true);
        this.GetEffectObject().transform.parent = null;
        this.transform.localScale = Vector3.zero;
        this.GetColiider().enabled = false;
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

    // ���̃A�C�e���̌��݂̃X�e�[�g�̎擾
    public ItemStateType GetItemStateType()
    {
        return m_stateType;
    }

    public BuffManager_Tanabe.Buff.BuffType GetBuffType()
    {
        return m_buffType;
    }

    public Rigidbody GetRigidbody()
    {
        return m_rb;
    }

    public bool GetIsKinematic()
    {
        return m_rb.isKinematic;
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

    // �v���C���[�f�[�^�̐ݒ�
    [ClientRpc]
    public void RpcSetPlayerData(Player_Tanabe _playerData)
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
