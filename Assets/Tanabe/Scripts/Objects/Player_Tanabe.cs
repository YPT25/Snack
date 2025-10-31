using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class Player_Tanabe : CharacterBase
{
    // ���񋓌^���[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[

    public enum WeaponID
    {
        NONE,
        HAMMER,
        GUN,
    }


    // ���p�����[�^���[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[

    /// <summary>
    /// �T�[�o�[�Ŋ��蓖�Ă���v���C���[�ԍ��B
    /// [SyncVar] �ɂ��S�N���C�A���g�Ɏ����œ�������܂��B
    /// </summary>
    [SyncVar]
    public int playerNumber;

    [Header("�J����")]
    private Transform m_cameraTransform;
    [SyncVar, Header("����ID"), SerializeField] private WeaponID m_weaponID;
    [Header("����I�u�W�F�N�g"), SerializeField] private GameObject m_weaponObject;
    [Header("����"), SerializeField] private Hammer_Tanabe m_hammer;
    [Header("����"), SerializeField] private Gun_Tanabe m_gun;

    // �A�C�e���}�l�[�W��
    private PossessionManager_Tanabe m_possessionManager;
    // �Z�b�g�p�[�c
    private SetPart_Tanabe m_setPart = null;
    private float m_removeEquippedTimer = 1.0f;
    // �����������̃A�C�e��
    private ItemStateMachine m_equipStandbyItem = null;
    // �E��ɏ������Ă���A�C�e��
    private ItemStateMachine m_rightHandsItem = null;

    // ���L�p�J�����x�N�g��
    [SyncVar] private Vector3 m_notLocalCameraForward;

    // ���݂̃X�e�[�g
    IPlayerState_Tanabe m_currentState;

    // Rigidbody
    private Rigidbody m_rb;
    // ���n����
    private bool isGrounded;
    // �ړ�����t���O
    private bool m_isMoving = false;
    // �f�t�H���g��Ԃ��̔���t���O
    private bool m_isDefaultState = true;
    // �n���}�[�̃`���[�W����
    private bool m_isAttackCharge = false;

    private float m_prevShotButton = 0.0f;

    // �W�����v���N�G�X�g
    private bool m_jumpRequest = false;

    // �G�C����Ԃ�
    [SyncVar] private bool m_isAiming = false;

    // Throw��Ԃ��̔���t���O
    [SyncVar] private bool m_isThrow = false;

    // �������������Ă��邩
    [SyncVar] private bool m_isHitBomb = false;

    // �d��
    [SyncVar] private float m_prevGravity = 0.0f;

    // �f�o�b�O�p�p�����[�^�[�e�L�X�g
    private DebugParameterText_Tanabe m_debugParameterText;

    // ���֐����[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[

    // �J�n�֐�
    public override void OnStartClient()
    {
        // �L�����N�^�[�^�C�v�̐ݒ�
        base.SetCharacterType(CharacterType.HERO_TYPE);
        base.OnStartClient();
        // Rigidbody���A�^�b�`����
        m_rb = GetComponent<Rigidbody>();

        // �A�C�e���}�l�[�W�����A�^�b�`����
        m_possessionManager = GetComponent<PossessionManager_Tanabe>();

        if (!this.isLocalPlayer) { return; }

        // �f�o�b�O���̂�
        m_debugParameterText = GameObject.Find("DebugParameterText")?.GetComponent<DebugParameterText_Tanabe>();
        if(m_debugParameterText != null) { m_debugParameterText.SetCharacter(this); }

        m_cameraTransform = GameObject.FindWithTag("MainCamera").transform;

        // �����̃X�e�[�g�̐ݒ�
        ChangeState(new IdleState(this));

        if (m_weaponID == WeaponID.HAMMER)
        {
            //�ꎞ��~ this.gameObject.GetComponentInChildren<DebugAttacker>().SetParentCharacter(this);
        }
    }

    // �X�V�֐�

    public override void Update()
    {
        if (!this.isLocalPlayer) { return; }
        m_notLocalCameraForward = m_cameraTransform.forward;
        if (Input.GetKeyDown(KeyCode.P))
        {
            this.transform.position = new Vector3(0f, 2f, 0f);
            m_rb.velocity = Vector3.zero;
            base.SetHp(base.GetMaxHP());
        }

        base.Update();
        // ���݂̃X�e�[�g�̍X�V����
        m_currentState?.Update();

        // �ړ����Ă��Ȃ���ԂȂ�X�^�~�i���񕜂���
        if (!m_isMoving)
        {
            base.SetStamina(GetStamina() + Time.deltaTime);
        }

        // ���n���菈��
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);

        //// �f�o�b�O�p��Ray��Scene�r���[�ŉ���
        //Debug.DrawRay(transform.position, Vector3.down * 1.1f, isGrounded ? Color.green : Color.red);

        // �W�����v����
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            m_jumpRequest = true;
        }

        if(m_setPart != null)
        {
            if (Input.GetKeyUp(KeyCode.V) || Input.GetKeyUp("joystick button 4"))
            {
                m_removeEquippedTimer = 1.0f;
            }
            else if (Input.GetKey(KeyCode.V) || Input.GetKey("joystick button 4"))
            {
                m_removeEquippedTimer -= Time.deltaTime;
                if (m_removeEquippedTimer <= 0.0f)
                {
                    m_removeEquippedTimer = 1.0f;
                    ItemStateMachine item = m_setPart.GetComponent<ItemStateMachine>();
                    this.SetPart(null);
                    CmdChangeState_Item(item, ItemStateMachine.ItemStateType.DROP);

                    Vector3 moveVector = new Vector3((float)UnityEngine.Random.Range(-10, 11) * 0.1f, 3.0f, (float)UnityEngine.Random.Range(-10, 11) * 0.1f);
                    //item.GetRigidbody().AddForce(moveVector.normalized * 5.0f, ForceMode.Impulse);
                    this.CmdAddForce_Item(item, moveVector.normalized * 5.0f, ForceMode.Impulse);
                }
            }
        }
        else if (GetIsDefaultState() && m_equipStandbyItem != null && m_equipStandbyItem.GetPlayerData() == this)
        {
            if (Input.GetButtonDown("Attack")                                           && this.GetPart() == null ||
                this.GetPrevShotButton() == 0.0f && Input.GetAxisRaw("Shot") != 0.0f    && this.GetPart() == null)
            {
                this.SetPrevShotButton(Input.GetAxisRaw("Shot"));
                this.CmdChangeState_Item(m_equipStandbyItem, ItemStateMachine.ItemStateType.PARTEQUIPPED);
                //m_equipStandbyItem.ChangeState(m_equipStandbyItem, ItemStateMachine.ItemStateType.PARTEQUIPPED);
                this.CmdSetEquipStandbyItem(m_equipStandbyItem);
            }
        }
        if (!base.GetIsMove())
        {
            m_rb.velocity = Vector3.zero;
            return;
        }

        m_prevGravity = m_rb.velocity.y;
    }

    public override void FixedUpdate()
    {
        if (!this.isLocalPlayer) { return; }

        base.FixedUpdate();
        // ���݂̃X�e�[�g�̍X�V����
        m_currentState?.FixedUpdate();


        // �W�����v�̎w�����o���Ƃ��̂ݒʂ�
        if (m_jumpRequest)
        {
            m_rb.AddForce(Vector3.up * 5f, ForceMode.Impulse);
            m_jumpRequest = false;
        }

        // �v���C���[��Y��]���J������Y��]�ɍ��킹��
        Vector3 camForward = m_cameraTransform.forward;
        camForward.y = 0;
        if (camForward.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(camForward.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10.0f * Time.deltaTime);
        }

    }

    // ���݂̃X�e�[�g�̕ύX
    public void ChangeState(IPlayerState_Tanabe newState)
    {
        // ���݂̃X�e�[�g�̏I������
        m_currentState?.Exit();
        // �V���ȃX�e�[�g�̐ݒ�
        m_currentState = newState;
        // �V���ȃX�e�[�g�̊J�n����
        m_currentState.Enter();
    }

    // �U����������
    public void AttackCharge()
    {
        m_hammer.AttackCharge();
    }

    // �U������
    public void Attack()
    {
        m_hammer.Attack();
    }

    // �U����������
    public void ExitAttack()
    {
        m_hammer.ExitAttack();
    }

    // ���َq�̃|�C���g���󂯎��
    public void AddPoint(float point)
    {
        // �}�l�[�W�����Ƀ|�C���g��n��
        SweetScore sweetScore = FindObjectOfType<SweetScore>();
        sweetScore.AddScore(10);
    }

    // �A�C�e���̏�ԑJ��
    [Command]
    public void CmdChangeState_Item(ItemStateMachine _item, ItemStateMachine.ItemStateType _newStateType)
    {
        // Throw��ԂɑJ�ڂ���
        _item.RpcChangeState(_item, _newStateType);
    }

    [Command]
    private void CmdAddForce_Item(ItemStateMachine _item, Vector3 _moveForce, ForceMode _forceMode)
    {
        _item.GetRigidbody().AddForce(_moveForce, _forceMode);
        this.RpcAddForce_Item(_item, _moveForce, _forceMode);
    }

    [ClientRpc]
    private void RpcAddForce_Item(ItemStateMachine _item, Vector3 _moveForce, ForceMode _forceMode)
    {
        _item.GetRigidbody().AddForce(_moveForce, _forceMode);
    }

    [Command]
    public void CmdDestroysObject(GameObject _gameObject)
    {
        Destroy(_gameObject);
    }


    // ���Q�b�^�[�֐����[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[

    // ����ID�̎擾
    public WeaponID GetWeaponID()
    {
        return m_weaponID;
    }

    // CameraTransform.forward�̎擾
    public Vector3 GetCameraForward()
    {
        if(this.isLocalPlayer)
        {
            return m_cameraTransform.forward;
        }
        else
        {
            return m_notLocalCameraForward;
        }
    }

    // �e���̎擾
    [Client]
    public Transform GetGunHead()
    {
        return m_gun.GetGunHead();
    }

    // �G�C����Ԃ�
    public bool GetIsAiming()
    {
        return m_isAiming;
    }

    // Rigidbody�̎擾
    public Rigidbody GetRigidbody()
    {
        return m_rb;
    }

    // �d�͂̎擾
    public float GetGravity()
    {
        return m_prevGravity;
    }

    // �A�C�e���}�l�[�W���̎擾
    public PossessionManager_Tanabe GetPossesionManager()
    {
        return m_possessionManager;
    }

    // �Z�b�g�p�[�c�̎擾
    public SetPart_Tanabe GetPart()
    {
        return m_setPart;
    }

    // �Z�b�g�p�[�c�̃^�C�v�̎擾
    public SetPart_Tanabe.PartType GetPartType()
    {
        if (m_setPart != null)
        {
            return m_setPart.GetPartType();
        }
        else
        {
            return global::SetPart_Tanabe.PartType.NONE_TYPE;
        }
    }

    // �����ҋ@�A�C�e���̎擾
    public ItemStateMachine GetEquipStandbyItem()
    {
        return m_equipStandbyItem;
    }

    // �E��Ɏ����Ă���A�C�e���̎擾
    public ItemStateMachine GetRightHandsItem()
    {
        return m_rightHandsItem;
    }

    // ���n����̎擾
    public bool GetIsGrounded()
    {
        return isGrounded;
    }

    // �ړ����x�̎擾
    public override float GetMoveSpeed()
    {
        // �c��HP��3����1�����Ȃ�ړ����x��x������
        if (GetHp() / GetMaxHP() < 0.3f)
        {
            return base.GetMoveSpeed() * 0.7f;
        }
        return base.GetMoveSpeed();
    }

    // �f�t�H���g�̈ړ����x�̎擾
    public float GetDefaultMoveSpeed()
    {
        return base.GetMoveSpeed();
    }

    // �f�t�H���g��Ԃ��̔���t���O�̎擾
    public bool GetIsDefaultState()
    {
        if(m_weaponID == WeaponID.HAMMER && m_isAttackCharge)
        {
            return true;
        }
        return m_isDefaultState;
    }

    public float GetPrevShotButton()
    {
        return m_prevShotButton;
    }

    // Throw��Ԃ��̔���t���O�̎擾
    public bool GetIsThrow()
    {
        return m_isThrow;
    }

    // ���e���������Ă��邩
    public bool GetIsHitBomb()
    {
        return m_isHitBomb;
    }


    // ���Z�b�^�[�֐����[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[�[

    // �Z�b�g�p�[�c�̐ݒ�
    public void SetPart(SetPart_Tanabe _setPart)
    {
        global::SetPart_Tanabe.PartType prevPartType = global::SetPart_Tanabe.PartType.NONE_TYPE;
        if (m_setPart != null)
        {
            prevPartType = m_setPart.GetPartType();
        }

        // �g�p���킪�n���}�[�Ȃ�ʂ�
        if (m_weaponID == WeaponID.HAMMER)
        {
            if (prevPartType == global::SetPart_Tanabe.PartType.LONGBARREL)
            {
                SetPower(GetPower() - 40f);
            }
            else if (prevPartType == global::SetPart_Tanabe.PartType.SHARPBULLET)
            {
                SetPower(GetPower() - 10f);
            }

            // �V���ȃZ�b�g�p�[�c�𑕔�����
            if (_setPart != null)
            {
                m_hammer.SetPartType(_setPart.GetPartType());

                if (_setPart.GetPartType() == global::SetPart_Tanabe.PartType.LONGBARREL)
                {
                    SetPower(GetPower() + 40f);
                }
                else if (_setPart.GetPartType() == global::SetPart_Tanabe.PartType.SHARPBULLET)
                {
                    SetPower(GetPower() + 10f);
                }
            }
            // �Z�b�g�p�[�c���O��
            else
            {
                m_hammer.SetPartType(global::SetPart_Tanabe.PartType.NONE_TYPE);
            }
        }
        // �Z�b�g�p�[�c��ύX�E��������
        m_setPart = _setPart;
    }

    // �����ҋ@�A�C�e���̐ݒ�
    public void SetEquipStandbyItem(ItemStateMachine _item)
    {
        m_equipStandbyItem = _item;
    }

    [Command]
    public void CmdSetEquipStandbyItem(ItemStateMachine _item)
    {
        m_equipStandbyItem = _item;
        RpcSetEquipStandbyItem(_item);
    }

    [ClientRpc]
    public void RpcSetEquipStandbyItem(ItemStateMachine _item)
    {
        m_equipStandbyItem = _item;
    }

    // �E��Ɏ��A�C�e���̐ݒ�
    public void SetRightHandsItem(ItemStateMachine _item)
    {
        m_rightHandsItem = _item;
    }

    // �ړ�����t���O�̐ݒ�
    public void SetIsMoving(bool _flag)
    {
        m_isMoving = _flag;
    }

    // �f�t�H���g��Ԃ��̔���t���O�̐ݒ�
    public void SetIsDefaultState(bool _flag)
    {
        m_isDefaultState = _flag;
    }

    // �n���}�[�̃`���[�W����
    public void SetIsAttackCharge(bool _flag)
    {
        m_isAttackCharge = _flag;
    }

    public void SetPrevShotButton(float _shot)
    {
        m_prevShotButton = _shot;
    }

    // �G�C����Ԃ��̐ݒ�
    public void SetIsAiming(bool _flag)
    {
        m_isAiming = _flag;
    }

    // Throw��Ԃ��̔���t���O�̐ݒ�
    public void SetIsThrow(bool _flag)
    {
        m_isThrow = _flag;
        if (m_weaponID == WeaponID.HAMMER)
        {
            m_hammer.SetIsThrow(_flag);
        }
    }

    // ���e���������Ă��邩�̐ݒ�
    public void SetIsHitBomb(bool _flag)
    {
        m_isHitBomb = _flag;
    }
}
