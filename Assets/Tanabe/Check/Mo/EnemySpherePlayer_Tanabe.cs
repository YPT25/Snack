using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using TMPro;

public class EnemySpherePlayer_Tanabe : CharacterBase
{
    // ＜パラメータ＞ーーーーーーーーーーーーーーーーーーーーー

    /// <summary>
    /// サーバーで割り当てられるプレイヤー番号。
    /// [SyncVar] により全クライアントに自動で同期されます。
    /// </summary>
    [SyncVar]
    public int playerNumber;

    [SyncVar]
    public string playerName = "Player";

    [Header("カメラ")]
    private Transform m_cameraTransform;
    [Header("カメラ制御スクリプト"), SerializeField] private TPSCameraController_Tanabe m_cameraController;
    [Header("デバッグモードか"), SerializeField] private bool m_isDebugMode = false;

    private float m_removeEquippedTimer = 1.0f;

    [SerializeField] private Collider m_attackCollider;

    // 共有用カメラベクトル
    [SyncVar] private Vector3 m_notLocalCameraForward;
    private Vector3 m_targetVelocity = Vector3.zero;

    // 現在のステート
    IPlayerState_Tanabe m_currentState;

    // Rigidbody
    private Rigidbody m_rb;
    // 着地判定
    private bool isGrounded;
    // 移動判定フラグ
    private bool m_isMoving = false;
    // スタミナ無限フラグ
    private bool m_isStaminan = false;
    // デフォルト状態かの判定フラグ
    private bool m_isDefaultState = true;
    // ハンマーのチャージ中か
    private bool m_isAttackCharge = false;

    private float m_prevShotButton = 0.0f;

    // ジャンプリクエスト
    private bool m_jumpRequest = false;

    private bool m_isDash = false;
    private float m_dashTime = 1f;
    private float m_dashInterval = 2f;

    // エイム状態か
    [SyncVar] private bool m_isAiming = false;

    // Throw状態かの判定フラグ
    [SyncVar] private bool m_isThrow = false;

    // 爆発が当たっているか
    [SyncVar] private bool m_isHitBomb = false;

    private bool m_isAttract = false;

    // 重力
    [SyncVar] private float m_prevGravity = 0.0f;

    // デバッグ用パラメーターテキスト
    private DebugParameterText_Tanabe m_debugParameterText;

    private PlayerManager_Tanabe m_playerManager;
    private GameOption_Tanabe m_gameOption;

    private DamagePerformance m_damagePerformance;


    // ＜関数＞ーーーーーーーーーーーーーーーーーーーーーーーー

    // 開始関数
    public override void OnStartServer()
    {
        base.OnStartServer();

        // Rigidbodyをアタッチする
        m_rb = GetComponent<Rigidbody>();

        m_damagePerformance = GetComponent<DamagePerformance>();
    }

    public override void OnStartClient()
    {
        // キャラクタータイプの設定
        base.SetCharacterType(CharacterType.ENEMY_TYPE);
        base.OnStartClient();
        // サーバーに自分の名前を登録する
        CmdReportNameToServer(PlayerNameHolder.PlayerName);
        // Rigidbodyをアタッチする
        m_rb = GetComponent<Rigidbody>();

        m_damagePerformance = GetComponent<DamagePerformance>();

        // ローカルプレイヤーではない物のみ通す
        if (!this.isLocalPlayer)
        {
            return;
        }

        // カメラコントローラがあればプレイヤーマネージャに登録しておく
        if (m_cameraController != null)
        {
            m_cameraController.SetTarget(this.transform);
        }

        // デバッグ時のみ
        m_debugParameterText = GameObject.Find("DebugParameterText")?.GetComponent<DebugParameterText_Tanabe>();
        if(m_debugParameterText != null) { m_debugParameterText.SetCharacter(this); }

        m_cameraTransform = GameObject.FindWithTag("MainCamera").transform;

        // 攻撃判定主の登録
        this.gameObject.GetComponentInChildren<DebugAttackTest_Tanabe>()?.CmdSetParentCharacter(this);

    }

    [Command]
    private void CmdReportNameToServer(string name)
    {
        // ここでサーバーが全員に通知する
        RpcSendNameToDisplay(name);
    }

    [ClientRpc]
    private void RpcSendNameToDisplay(string name)
    {
        // モニターにある PlayerListDisplay_Ashuri 
        PlayerListDisplay_Ashuri display = GetComponent<PlayerListDisplay_Ashuri>();
        if (display != null)
        {
            display.RespownName(name);
        }
    }

    // 更新関数

    public override void Update()
    {
        // ローカルプレイヤー以外は処理しない
        if (!this.isLocalPlayer) { return; }
        base.Update();

        float z = Input.GetAxis("Vertical Pad");

        if (Mathf.Abs(Input.GetAxisRaw("Vertical")) != 0.0f) z = Input.GetAxisRaw("Vertical");

        // コントローラー入力のみ判定の調整を行う
        if (Mathf.Abs(z) <= 0.05f) z = 0f;
        else if (z > 0f) z = 1f;
        else z = -1f;

        // Transformの取得
        Transform transform = GetRigidbody().transform;
        // 移動ベクトルの算出
        Vector3 move = (transform.forward * z).normalized;

        // 入力なければIdleへ戻る
        if (move.magnitude == 0)
        {
            move = Vector3.zero;
        }

        // 左クリックを感知したら攻撃ステートに遷移する
        if (Input.GetButtonDown("Attack") && !m_isDash)
        {
            m_isDash = true;
            m_dashTime = 0.5f;
            m_attackCollider.enabled = true;
        }

        float dashSpeed = 1f;
        if(m_isDash)
        {
            if(m_dashTime > 0f)
            {
                move = (transform.forward * 1).normalized;
                m_dashTime -= Time.deltaTime;
                dashSpeed = 5f;
                if(m_dashTime <= 0f)
                {
                    m_attackCollider.enabled = false;
                }
            }
            else
            {
                m_dashInterval -= Time.deltaTime;
                if(m_dashInterval <= 0f)
                {
                    m_dashInterval = 2f;
                    m_isDash = false;
                    move = Vector3.zero;
                }
            }
        }

        // 移動速度の算出
        m_targetVelocity = move * GetMoveSpeed() * dashSpeed;

        // 着地判定処理
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);

        // 移動フラグがfalseのときはこれ以上処理しない
        if (!base.GetIsMove())
        {
            // 現在の速度を0にする
            m_rb.velocity = Vector3.zero;
            return;
        }
        // 現在の重力を保存する
        m_prevGravity = m_rb.velocity.y;
    }

    // 更新関数
    public override void FixedUpdate()
    {
        // ローカルプレイヤー以外は処理しない
        if (!this.isLocalPlayer) { return; }

        base.FixedUpdate();
        // 現在のステートの更新処理
        m_currentState?.FixedUpdate();

        this.Move(m_targetVelocity);

        // ジャンプの指示が出たときのみ通す
        if (m_jumpRequest)
        {
            m_rb.AddForce(Vector3.up * 5f, ForceMode.Impulse);
            m_jumpRequest = false;
        }

        if (isGrounded)
        {
            Vector3 velocity = m_rb.velocity;
            velocity.x *= 0.97f;
            velocity.z *= 0.97f;
            m_rb.velocity = velocity;
        }

        // プレイヤーのY回転をカメラのY回転に合わせる
        Vector3 camForward = m_cameraTransform.forward;
        camForward.y = 0;
        if (camForward.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(camForward.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10.0f * Time.deltaTime);
        }

    }

    public void Move(Vector3 _targetVelocity)
    {
        if (GetIsHitBomb()) { return; }

        // 移動速度の設定
        Vector3 velocity = GetRigidbody().velocity;
        Vector3 velocityChange = _targetVelocity - new Vector3(velocity.x, 0, velocity.z);
        if (GetIsAttract())
        {
            GetRigidbody().AddForce(velocityChange * 0.5f / Time.fixedDeltaTime, ForceMode.Acceleration);
        }
        else
        {
            GetRigidbody().AddForce(velocityChange / Time.fixedDeltaTime, ForceMode.Acceleration);
        }
    }

    // オブジェクトを削除する
    [Command]
    public void CmdDestroysObject(GameObject _gameObject)
    {
        Destroy(_gameObject);
    }

    public override void Damage(float _damage)
    {
        base.Damage(_damage);
        m_damagePerformance?.Damage();
    }


    // ＜ゲッター関数＞ーーーーーーーーーーーーーーーーーーーー

    // CameraTransform.forwardの取得
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

    // Rigidbodyの取得
    public Rigidbody GetRigidbody()
    {
        return m_rb;
    }

    // 重力の取得
    public float GetGravity()
    {
        return m_prevGravity;
    }

    // 着地判定の取得
    public bool GetIsGrounded()
    {
        return isGrounded;
    }

    // スタミナ無限フラグの取得の取得
    public bool GetIsStaminan()
    {
        return m_isStaminan;
    }

    // 移動速度の取得
    public override float GetMoveSpeed()
    {
        // 残りHPが3分の1未満なら移動速度を遅くする
        if (GetHp() / GetMaxHP() < 0.3f)
        {
            return base.GetMoveSpeed() * 0.7f;
        }
        return base.GetMoveSpeed();
    }

    // デフォルトの移動速度の取得
    public float GetDefaultMoveSpeed()
    {
        return base.GetMoveSpeed();
    }

    // RTボタンの入力判定の取得
    public float GetPrevShotButton()
    {
        return m_prevShotButton;
    }

    // Throw状態かの判定フラグの取得
    public bool GetIsThrow()
    {
        return m_isThrow;
    }

    // 爆弾が当たっているか
    public bool GetIsHitBomb()
    {
        return m_isHitBomb;
    }

    public bool GetIsAttract()
    {
        return m_isAttract;
    }

    // ポーズ中か
    public bool IsPause()
    {
        if(m_gameOption == null) { return false; }
        return m_gameOption.IsPause();
    }


    // ＜セッター関数＞ーーーーーーーーーーーーーーーーーーーー

    // 移動判定フラグの設定
    public void SetIsMoving(bool _flag)
    {
        m_isMoving = _flag;
    }

    // スタミナ無限フラグの設定
    public void SetIsStaminan(bool _flag)
    {
        m_isStaminan = _flag;
    }

    // デフォルト状態かの判定フラグの設定
    public void SetIsDefaultState(bool _flag)
    {
        m_isDefaultState = _flag;
    }

    // ハンマーのチャージ中か
    public void SetIsAttackCharge(bool _flag)
    {
        m_isAttackCharge = _flag;
    }

    // RTボタンの入力処理
    public void SetPrevShotButton(float _shot)
    {
        m_prevShotButton = _shot;
    }

    // エイム状態かの設定
    public void SetIsAiming(bool _flag)
    {
        m_isAiming = _flag;
    }

    // 爆弾が当たっているかの設定
    public void SetIsHitBomb(bool _flag)
    {
        m_isHitBomb = _flag;
    }

    public void SetIsAttract(bool _flag)
    {
        m_isAttract = _flag;
    }

    // プレイヤーの名前の設定
    public void SetPlayerName(string name)
    {
        playerName = name;
    }
}
