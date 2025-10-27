using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Hammer_Tanabe : NetworkBehaviour
{
    private Player_Tanabe m_player;

    [SerializeField] private HammerObject_Tanabe m_weaponTest;
    [Header("攻撃判定"), SerializeField] private Collider m_attackCollider;
    [Header("衝撃波オブジェクト"), SerializeField] private GameObject m_shockWavePrefab;
    [Header("爆発オブジェクト"), SerializeField] private GameObject m_bombExplosionPrefab;

    private Vector3 m_defaultPosition;

    private Quaternion m_defaultRotation;
    private Quaternion m_chargeRotation;
    private Quaternion m_attackRotation;

    private Quaternion m_defaultRotation2;
    private Quaternion m_chargeRotation2;
    private Quaternion m_attackRotation2;

    private bool m_isCharge = false;
    private bool m_isAttack = false;
    private bool m_isExitAttack = false;
    private float m_chargePower = 0.0f;
    private float m_chargeTimer = 0.0f;
    private float m_attackTimer = 0.0f;
    private bool m_isShockWave = false;

    private bool m_isSharpAttack = false;

    // Start is called before the first frame update
    void Start()
    {
        m_player = GetComponentInParent<Player_Tanabe>();

        m_defaultPosition = this.transform.localPosition;

        m_defaultRotation = this.transform.localRotation;
        m_chargeRotation = Quaternion.Euler(-30f, 0f, 0f);
        m_attackRotation = Quaternion.Euler(/*90f*/107f, 0f, 0f);

        m_defaultRotation2 = Quaternion.Euler(0f, 0f, -80f);
        m_chargeRotation2 = Quaternion.Euler(0f, 30f, -80f);
        m_attackRotation2 = Quaternion.Euler(0f, -155f, -80f);

        m_attackCollider.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(!this.isLocalPlayer) { return; }
        if (!m_player.GetIsAttack())
        {
            if (m_player.GetPartType() == global::SetPart_Tanabe.PartType.NONE_TYPE)
            {
                transform.localRotation = m_defaultRotation;
            }
            else
            {
                transform.localRotation = m_defaultRotation2;
            }
            m_attackTimer = 0.0f;
            m_isAttack = false;
            m_isExitAttack = false;
            return;
        }

        // 溜め攻撃のチャージ中のみ通す
        if (m_isCharge)
        {
            if (m_chargeTimer < 1.0f)
            {
                m_chargeTimer += Time.deltaTime;
                if (m_player.GetPartType() == global::SetPart_Tanabe.PartType.SHARPBULLET)
                {
                    m_chargeTimer += Time.deltaTime * 5.0f;
                }
            }
            this.ChargeMotion(this.EaseOutSine(m_chargeTimer));
        }
        if (!m_isAttack) { return; }
        Vector3 eulerAngles = transform.eulerAngles;

        // 攻撃始めの動き
        if (m_attackTimer <= 1.0f)
        {
            m_attackTimer += Time.deltaTime * 4.0f;
            if (m_player.GetPartType() == global::SetPart_Tanabe.PartType.SHARPBULLET)
            {
                m_attackTimer += Time.deltaTime * 2.0f;
            }

            this.ChargeMotion(this.EaseOutSine(m_chargeTimer));
            this.AttackMotion();

            // 攻撃判定を有効にするタイミング
            if (m_attackTimer >= 0.3f && m_attackTimer - Time.deltaTime * 4.0f <= 0.3f)
            {
                CmdSetAttackColliderEnabled(true);
            }
        }
        // 攻撃状態からデフォルトに戻す動き
        else if (m_isExitAttack)
        {
            if (m_attackCollider.enabled)
            {
                CmdSetAttackColliderEnabled(false);
                //m_attackCollider.enabled = false;
            }

            m_attackTimer += Time.deltaTime * 2.0f;

            this.ExitAttackMotion();
        }

        // 攻撃モーション終了後の処理
        if (m_attackTimer >= 2.0f)
        {
            if (m_player.GetPartType() == global::SetPart_Tanabe.PartType.NONE_TYPE)
            {
                transform.localRotation = m_defaultRotation;
            }
            else
            {
                transform.localRotation = m_defaultRotation2;
            }
            m_chargeTimer = 0.0f;
            m_attackTimer = 0.0f;
            m_isAttack = false;
            m_isExitAttack = false;
        }
    }

    // 攻撃準備モーション
    private void ChargeMotion(float _time)
    {
        switch (m_player.GetPartType())
        {
            case global::SetPart_Tanabe.PartType.NONE_TYPE:
                {
                    transform.localRotation = Quaternion.Lerp(m_defaultRotation, m_chargeRotation, m_chargeTimer);
                    break;
                }
            case global::SetPart_Tanabe.PartType.LONGBARREL:
                {
                    transform.localRotation = Quaternion.Lerp(m_defaultRotation2, m_chargeRotation2, m_chargeTimer);
                    break;
                }
            case global::SetPart_Tanabe.PartType.SHARPBULLET:
                {
                    //m_attackCollider.enabled = true;
                    //transform.localRotation = Quaternion.Lerp(m_defaultRotation2, m_attackRotation2, m_chargeTimer);
                    break;
                }
            default:
                break;
        }
    }

    // 攻撃モーション
    private void AttackMotion()
    {
        switch (m_player.GetPartType())
        {
            case global::SetPart_Tanabe.PartType.NONE_TYPE:
                {
                    transform.localRotation = Quaternion.Lerp(m_defaultRotation, m_attackRotation, m_attackTimer);
                    break;
                }
            case global::SetPart_Tanabe.PartType.LONGBARREL:
                {
                    transform.localRotation = Quaternion.Lerp(m_defaultRotation2, m_attackRotation2, m_attackTimer);
                    break;
                }
            case global::SetPart_Tanabe.PartType.SHARPBULLET:
                {
                    if (m_isSharpAttack)
                    {
                        transform.localRotation = Quaternion.Lerp(m_defaultRotation2, m_attackRotation2, m_attackTimer);
                    }
                    else
                    {
                        transform.localRotation = Quaternion.Lerp(m_attackRotation2, m_defaultRotation2, m_attackTimer);
                    }

                    if (m_attackTimer >= 1.0f)
                    {
                        CmdSetAttackColliderEnabled(false);
                        //m_attackCollider.enabled = false;

                        if (!m_isSharpAttack)
                        {
                            m_attackTimer = 0.0f;
                            m_isAttack = false;
                            m_isExitAttack = false;
                            m_player.ChangeState(new IdleState(m_player));
                        }
                    }
                    break;
                }
            default:
                break;
        }
    }

    // 攻撃解除モーション
    private void ExitAttackMotion()
    {
        switch (m_player.GetPartType())
        {
            case global::SetPart_Tanabe.PartType.NONE_TYPE:
                {
                    transform.localRotation = Quaternion.Lerp(m_attackRotation, m_defaultRotation, m_attackTimer - 1.0f);
                    break;
                }
            case global::SetPart_Tanabe.PartType.LONGBARREL:
                {
                    transform.localRotation = Quaternion.Lerp(m_attackRotation2, m_defaultRotation2, m_attackTimer - 1.0f);
                    break;
                }
            case global::SetPart_Tanabe.PartType.SHARPBULLET:
                {
                    transform.localRotation = Quaternion.Lerp(m_attackRotation2, m_defaultRotation2, m_attackTimer - 1.0f);
                    break;
                }
            default:
                break;
        }
    }

    // 攻撃準備
    public void AttackCharge()
    {
        m_isCharge = true;
        m_chargeTimer = 0.0f;
    }

    // 攻撃処理
    public void Attack()
    {
        m_isCharge = false;
        m_isAttack = true;
        m_isExitAttack = false;
        m_attackTimer = 0.0f;

        if (m_player.GetPartType() == global::SetPart_Tanabe.PartType.SHARPBULLET)
        {
            m_isSharpAttack = !m_isSharpAttack;
            CmdSetAttackColliderEnabled(true);
            //m_attackCollider.enabled = true;
            return;
        }

        if (m_chargeTimer >= 1.0f)
        {
            m_chargePower = 20.0f;
        }
        else if (m_chargeTimer >= 0.5f)
        {
            m_chargePower = 10.0f;
        }
        m_player.SetPower(m_player.GetPower() + m_chargePower);
    }

    // 攻撃解除処理
    public void ExitAttack()
    {
        m_isSharpAttack = false;

        m_player.SetPower(m_player.GetPower() - m_chargePower);

        m_chargePower = 0.0f;

        if (m_player.GetPartType() == global::SetPart_Tanabe.PartType.NONE_TYPE && !m_isExitAttack/* && m_player.GetIsGrounded()*/)
        {
            m_isShockWave = true;
            // プレハブをGameObject型で取得
            //GameObject obj = (GameObject)Resources.Load("ShockWave");


            this.CmdShockWave(m_chargeTimer, m_player.GetGravity(), m_weaponTest.GetPosition_HammerHead(), m_player.transform.forward);

            //RaycastHit hit;

            //// 着地判定処理
            //bool isGrounded = Physics.Raycast(m_weaponTest.GetPosition_HammerHead(), Vector3.down, out hit, 2.1f, 9);

            //if (isGrounded)
            //{

                //GameObject shockWave = Instantiate(m_shockWavePrefab);

                //Vector3 wavePosition = m_weaponTest.GetPosition_HammerHead();
                //wavePosition.y = hit.point.y + 0.1f;
                //shockWave.transform.position = wavePosition;
                ////if (m_player.GetGravity() < -0.2f)
                ////{
                ////    shockWave.GetComponent<ShockWave>().Fall(m_player.GetGravity() * (-2.0f) + m_chargeTimer * 2.0f);
                ////}

                //shockWave.GetComponent<ShockWave_Tanabe>().Fall(1f + m_player.GetGravity() * (-2.0f) + m_chargeTimer * 20.0f);

                ////shockWave.transform.localPosition = m_weaponTest.GetPosition_HammerHead() - new Vector3(0f, hit.transform.position.y, 0f);
                //Debug.Log("衝撃波の生成");

                //if (m_player.GetGravity() < -13f)
                //{
                //    ExplodeTest();
                //}
            //}

        }
        m_isExitAttack = true;
    }

    [Command]
    private void CmdShockWave(float _chargeTimer, float _playersGravity, Vector3 _hammerHeadPosition, Vector3 _playersForward)
    {
        RaycastHit hit;

        // 着地判定処理
        bool isGrounded = Physics.Raycast(_hammerHeadPosition, Vector3.down, out hit, 2.1f, 9);

        if (!isGrounded) { return; }

        GameObject shockWave = Instantiate(m_shockWavePrefab);

        Vector3 wavePosition = _hammerHeadPosition;
        wavePosition.y = hit.point.y + 0.1f;
        shockWave.transform.position = wavePosition;

        shockWave.GetComponent<ShockWave_Tanabe>().Fall(1f + _playersGravity * (-2.0f) + _chargeTimer * 20.0f);

        NetworkServer.Spawn(shockWave);

        Debug.Log("衝撃波の生成");

        if (_playersGravity < -13f)
        {
            ExplodeTest(_hammerHeadPosition, _playersForward);
        }
    }

    [Command]
    private void CmdSetAttackColliderEnabled(bool _flag)
    {
        m_attackCollider.enabled = _flag;
        this.RpcSetAttackColliderEnabled(_flag);
    }

    [ClientRpc]
    private void RpcSetAttackColliderEnabled(bool _flag)
    {
        m_attackCollider.enabled = _flag;
    }

    public void SetPartType(SetPart_Tanabe.PartType _setPartType)
    {
        if (m_isCharge || m_isAttack || m_isExitAttack)
        {
            m_player.ChangeState(new IdleState(m_player));
        }
        m_player.SetPower(m_player.GetPower() - m_chargePower);
        m_chargePower = 0.0f;
        m_chargeTimer = 0.0f;
        m_attackTimer = 0.0f;
        m_isCharge = false;
        m_isAttack = false;
        m_isExitAttack = false;

        switch (_setPartType)
        {
            case global::SetPart_Tanabe.PartType.NONE_TYPE:
                {
                    m_weaponTest.gameObject.transform.localScale = new Vector3(1.47f, 1.47f, 1.47f);
                    transform.localRotation = m_defaultRotation;
                    break;
                }
            case global::SetPart_Tanabe.PartType.LONGBARREL:
                {
                    m_weaponTest.gameObject.transform.localScale = new Vector3(2.2f, 2.2f, 2.2f);
                    transform.localRotation = m_defaultRotation2;
                    break;
                }
            case global::SetPart_Tanabe.PartType.SHARPBULLET:
                {
                    m_weaponTest.gameObject.transform.localScale = new Vector3(1.47f, 1.47f, 1.47f);
                    transform.localRotation = m_defaultRotation2;
                    break;
                }
            default:
                break;
        }
    }

    public void SetIsThrow(bool _flag)
    {
        if (_flag)
        {
            this.transform.localPosition = m_defaultPosition + new Vector3(0f, 0f, 0.6f);
        }
        else
        {
            this.transform.localPosition = m_defaultPosition;
        }
    }

    private void ExplodeTest(Vector3 _hammerHeadPosition, Vector3 _playersForward)
    {
        //GameObject explode = Instantiate(m_bombExplosionPrefab);
        //explode.transform.position = m_weaponTest.GetPosition_HammerHead() + m_player.transform.forward * 2.0f;
        //explode.GetComponent<BombExplosion_Tanabe>().HammerExplode();

        GameObject explode = Instantiate(m_bombExplosionPrefab);
        explode.transform.position = _hammerHeadPosition + _playersForward * 2.0f;
        NetworkServer.Spawn(explode);
        explode.GetComponent<BombExplosion_Tanabe>().HammerExplode();

    }

    private float EaseOutSine(float _time)
    {
        return (1f - (float)Math.Pow(1f - _time, 4f));
    }
}
