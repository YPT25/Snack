using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : IPlayerState_Tanabe
{
    private Player_Tanabe m_player;

    // 攻撃時間
    private float m_attackTime = 0.0f;
    // 終了時間
    private float m_exitTimer = 1.0f;
    // 攻撃解除判定フラグ
    private bool m_isExitAttack = false;
    // 移動速度
    private Vector3 m_targetVelocity = Vector3.zero;
    // 攻撃強制解除タイミング
    private readonly float EXITTIME = 1.0f - (1.0f / 4.0f);

    public AttackState(Player_Tanabe _player)
    {
        m_player = _player;
    }

    public void Enter()
    {
        if(!m_player.GetIsThrow())
        {
            m_player.Attack();
            // セットパーツがなければ通す
            if(m_player.GetPart() == null)
            {
                Vector3 velocity = m_player.GetRigidbody().velocity;
                if (velocity.y >= 0.0f)
                {
                    velocity.y *= -1.0f;
                }
                else
                {
                    velocity.y *= 2.0f;
                }
                m_player.GetRigidbody().velocity = velocity;
            }
        }


        Debug.Log("Attack:開始");
    }

    public void Update()
    {
        if(!m_player.GetIsAttack())
        {
            m_isExitAttack = true;
            m_player.ChangeState(new IdleState(m_player));
            return;
        }

        if (m_player.GetIsThrow())
        {
            m_isExitAttack = true;
            m_player.ChangeState(new IdleState(m_player));
            m_player.SetIsThrow(false);
            return;
        }
        // 移動入力処理
        InputMovement();

        m_exitTimer -= Time.deltaTime;

        if (m_player.GetPartType() != global::SetPart_Tanabe.PartType.SHARPBULLET && !m_isExitAttack && Input.GetButtonUp("Attack") ||
            m_player.GetPartType() != global::SetPart_Tanabe.PartType.SHARPBULLET && !m_isExitAttack && m_exitTimer <= EXITTIME && m_player.GetIsGrounded())
        {
            m_isExitAttack = true;
            m_player.ExitAttack();
        }

        if(m_player.GetPartType() == global::SetPart_Tanabe.PartType.SHARPBULLET)
        {
            if (m_exitTimer <= 0.8f && Input.GetButtonDown("Attack") ||
                m_exitTimer <= 0.8f && m_player.GetPrevShotButton() == 0.0f && Input.GetAxisRaw("Shot") != 0.0f)
            {
                m_player.ChangeState(new AttackState(m_player));
            }
        }
        else
        {
            if (m_exitTimer <= 0.8f - (1.0f / 4.0f) && Input.GetButtonDown("Attack") ||
                m_exitTimer <= 0.8f - (1.0f / 4.0f) && m_player.GetPrevShotButton() == 0.0f && Input.GetAxisRaw("Shot") != 0.0f)
            {
                m_player.ChangeState(new AttackChargeState(m_player));
            }
        }

        if (m_exitTimer <= 0.0f && m_player.GetIsGrounded() ||
            m_exitTimer <= 0.0f && m_isExitAttack)
        {
            if(m_player.GetPartType() == global::SetPart_Tanabe.PartType.SHARPBULLET)
            {
                m_player.ExitAttack();
            }
            m_player.ChangeState(new IdleState(m_player));
        }

        m_player.SetPrevShotButton(Input.GetAxisRaw("Shot"));
    }

    public void FixedUpdate()
    {
        if (m_player.GetIsHitBomb()) { return; }
        // 移動速度の設定
        Vector3 velocity = m_player.GetRigidbody().velocity;
        Vector3 velocityChange = m_targetVelocity - new Vector3(velocity.x, 0, velocity.z);
        m_player.GetRigidbody().AddForce(velocityChange / Time.fixedDeltaTime, ForceMode.Acceleration);
    }

    public void Exit()
    {
        if(!m_isExitAttack && m_player.GetPartType() != global::SetPart_Tanabe.PartType.SHARPBULLET)
        {
            m_player.ExitAttack();
        }
        Debug.Log("Attack:終了");
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

        // Transformの取得
        Transform transform = m_player.GetRigidbody().transform;
        // 移動ベクトルの算出
        Vector3 move = (transform.right * x + transform.forward * z).normalized;

        // 移動速度の算出
        m_targetVelocity = move * m_player.GetMoveSpeed() * 0.3f;
    }
}
