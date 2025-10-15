using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : IPlayerState_Tanabe
{
    private Player_Tanabe m_player;

    public IdleState(Player_Tanabe _player)
    {
        m_player = _player;
    }

    public void Enter()
    {
        m_player.SetIsDefaultState(true);
        Debug.Log("Idle:�J�n");
    }

    public void Update()
    {
        // �ړ����͂����m������ړ��X�e�[�g�ɑJ�ڂ���
        if (Mathf.Abs(Input.GetAxis("Horizontal Pad")) > 0.05f || Mathf.Abs(Input.GetAxis("Vertical Pad")) > 0.05f ||
            Mathf.Abs(Input.GetAxisRaw("Horizontal")) != 0.0f || Mathf.Abs(Input.GetAxisRaw("Vertical")) != 0.0f)
        {
            if(m_player.GetIsMove())
            {
                m_player.ChangeState(new MoveState(m_player));
            }
        }
        // ���N���b�N�����m������U���X�e�[�g�ɑJ�ڂ���
        if (Input.GetButtonDown("Attack")                                               && m_player.GetWeaponID() == Player_Tanabe.WeaponID.HAMMER && m_player.GetIsAttack() ||
            m_player.GetPrevShotButton() == 0.0f && Input.GetAxisRaw("Shot") != 0.0f    && m_player.GetWeaponID() == Player_Tanabe.WeaponID.HAMMER && m_player.GetIsAttack())
        {
            if(m_player.GetIsThrow())
            {
                m_player.ChangeState(new AttackState(m_player));
            }
            else
            {
                m_player.ChangeState(new AttackChargeState(m_player));
            }
        }
        // �E�N���b�N�����m������_���X�e�[�g�ɑJ�ڂ���
        if (Input.GetButtonDown("Aiming")           && m_player.GetWeaponID() == Player_Tanabe.WeaponID.GUN && m_player.GetIsAttack() ||
            Input.GetAxisRaw("Aiming Pad") != 0.0f  && m_player.GetWeaponID() == Player_Tanabe.WeaponID.GUN && m_player.GetIsAttack())
        {
            m_player.ChangeState(new AimingState(m_player));
        }

        m_player.SetPrevShotButton(Input.GetAxisRaw("Shot"));
    }

    public void FixedUpdate()
    {

    }

    public void Exit()
    {
        m_player.SetIsDefaultState(false);
        Debug.Log("Idle:�I��");
    }
}
