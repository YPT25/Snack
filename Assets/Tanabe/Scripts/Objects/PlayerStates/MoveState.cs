using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveState : IPlayerState_Tanabe
{
    private Player_Tanabe m_player;

    private Vector3 m_targetVelocity = Vector3.zero;

    public MoveState(Player_Tanabe _player)
    {
        m_player = _player;
    }

    public void Enter()
    {
        m_player.SetIsMoving(true);
        m_player.SetIsDefaultState(true);
        Debug.Log("Move:�J�n");
    }

    public void Update()
    {
        if (!m_player.GetIsMove())
        {
            m_player.GetRigidbody().velocity = Vector3.zero;
            m_player.ChangeState(new IdleState(m_player));
            return;
        }

        float x = Input.GetAxis("Horizontal Pad");
        float z = Input.GetAxis("Vertical Pad");

        if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) != 0.0f) x = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(Input.GetAxisRaw("Vertical")) != 0.0f) z = Input.GetAxisRaw("Vertical");

        if (Mathf.Abs(x) <= 0.05f) x = 0f;
        else if (x > 0f) x = 1f;
        else x = -1f;
        if (Mathf.Abs(z) <= 0.05f) z = 0f;
        else if (z > 0f) z = 1f;
        else z = -1f;

        float dashSpeed = 1.0f;
        // Shift�L�[�������Ă���ԃ_�b�V����Ԃɂ���
        if(Input.GetButton("Dash")/*Input.GetKey(KeyCode.LeftShift)*/)
        {
            m_player.SetStamina(m_player.GetStamina() - Time.deltaTime);
            // �X�^�~�i��0f��葽����΃_�b�V����Ԃɂ���
            if(m_player.GetStamina() > 0.0f)
            {
                dashSpeed = 2.0f;
            }
        }
        // �_�b�V����ԂłȂ��Ȃ�X�^�~�i�������񕜂���
        else
        {
            m_player.SetStamina(m_player.GetStamina() + Time.deltaTime * 0.5f);
        }

        // Transform�̎擾
        Transform transform = m_player.GetRigidbody().transform;
        // �ړ��x�N�g���̎Z�o
        Vector3 move = (transform.right * x + transform.forward * z).normalized;

        // ���͂Ȃ����Idle�֖߂�
        if (move.magnitude == 0)
        {
            m_player.ChangeState(new IdleState(m_player));
            move = Vector3.zero;
            if (!m_player.GetIsHitBomb())
            {
                m_player.GetRigidbody().velocity = new Vector3(0f, m_player.GetRigidbody().velocity.y, 0f);
            }
        }
        // ���N���b�N�����m������U���X�e�[�g�ɑJ�ڂ���
        if (Input.GetButtonDown("Attack")                                               && m_player.GetWeaponID() == Player_Tanabe.WeaponID.HAMMER && m_player.GetIsAttack() ||
            m_player.GetPrevShotButton() == 0.0f && Input.GetAxisRaw("Shot") != 0.0f    && m_player.GetWeaponID() == Player_Tanabe.WeaponID.HAMMER && m_player.GetIsAttack())
        {
            if (m_player.GetIsThrow())
            {
                m_player.ChangeState(new AttackState(m_player));
            }
            else
            {
                m_player.ChangeState(new AttackChargeState(m_player));
            }
            move = Vector3.zero;
            if (!m_player.GetIsHitBomb())
            {
                m_player.GetRigidbody().velocity = new Vector3(0f, m_player.GetRigidbody().velocity.y, 0f);
            }
        }
        // �E�N���b�N�����m������_���X�e�[�g�ɑJ�ڂ���
        if (Input.GetButtonDown("Aiming")           && m_player.GetWeaponID() == Player_Tanabe.WeaponID.GUN && m_player.GetIsAttack() ||
            Input.GetAxisRaw("Aiming Pad") != 0.0f  && m_player.GetWeaponID() == Player_Tanabe.WeaponID.GUN && m_player.GetIsAttack())
        {
            m_player.ChangeState(new AimingState(m_player));
            move = Vector3.zero;
            if (!m_player.GetIsHitBomb())
            {
                m_player.GetRigidbody().velocity = new Vector3(0f, m_player.GetRigidbody().velocity.y, 0f);
            }
        }

        m_player.SetPrevShotButton(Input.GetAxisRaw("Shot"));

        // �ړ����x�̎Z�o
        m_targetVelocity = move * m_player.GetMoveSpeed() * dashSpeed;
    }

    public void FixedUpdate()
    {
        if(m_player.GetIsHitBomb()) { return; }
        // �ړ����x�̐ݒ�
        Vector3 velocity = m_player.GetRigidbody().velocity;
        Vector3 velocityChange = m_targetVelocity - new Vector3(velocity.x, 0, velocity.z);
        m_player.GetRigidbody().AddForce(velocityChange / Time.fixedDeltaTime, ForceMode.Acceleration);
    }

    public void Exit()
    {
        m_player.SetIsMoving(false);
        m_player.SetIsDefaultState(false);
        Debug.Log("Move:�I��");
    }
}
