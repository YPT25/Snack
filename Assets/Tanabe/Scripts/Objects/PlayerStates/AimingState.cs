using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimingState : IPlayerState_Tanabe
{
    private Player_Tanabe m_player;

    // �ړ����x
    private Vector3 m_targetVelocity = Vector3.zero;

    public AimingState(Player_Tanabe _player)
    {
        m_player = _player;
    }

    public void Enter()
    {
        m_player.SetIsAiming(true);


        Debug.Log("Aiming:�J�n");
    }

    public void Update()
    {
        if (!m_player.GetIsAttack())
        {
            m_player.ChangeState(new IdleState(m_player));
            return;
        }

        // �ړ����͏���
        InputMovement();

        if (!Input.GetButton("Aiming") && Input.GetAxisRaw("Aiming Pad") == 0.0f)
        {
            m_player.ChangeState(new IdleState(m_player));
        }
    }

    public void FixedUpdate()
    {
        if (m_player.GetIsHitBomb()) { return; }
        if (!m_player.GetIsMove())
        {
            m_player.GetRigidbody().velocity = Vector3.zero;
            return;
        }

        // �ړ����x�̐ݒ�
        Vector3 velocity = m_player.GetRigidbody().velocity;
        Vector3 velocityChange = m_targetVelocity - new Vector3(velocity.x, 0, velocity.z);
        m_player.GetRigidbody().AddForce(velocityChange / Time.fixedDeltaTime, ForceMode.Acceleration);
    }

    public void Exit()
    {
        m_player.SetIsAiming(false);

        Debug.Log("Aiming:�I��");
    }

    private void InputMovement()
    {
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

        // Transform�̎擾
        Transform transform = m_player.GetRigidbody().transform;
        // �ړ��x�N�g���̎Z�o
        Vector3 move = (transform.right * x + transform.forward * z).normalized;

        // �ړ����x�̎Z�o
        m_targetVelocity = move * m_player.GetMoveSpeed() * 0.3f;
    }
}
