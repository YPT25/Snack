using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Player_Tanabe : CharacterBase
{
    // ＜列挙型＞ーーーーーーーーーーーーーーーーーーーーーーー

    public enum WeaponID
    {
        NONE,
        HAMMER,
        GUN,
    }


    // ＜パラメータ＞ーーーーーーーーーーーーーーーーーーーーー

    [Header("カメラ")]
    public Transform m_cameraTransform;
    [SyncVar, Header("武器ID"), SerializeField] private WeaponID m_weaponID;
    [Header("武器オブジェクト"), SerializeField] private GameObject m_weaponObject;
    [Header("武器"), SerializeField] private Hammer_Tanabe m_hammer;
    [Header("武器"), SerializeField] private Gun_Tanabe m_gun;

    // アイテムマネージャ
    private PossessionManager_Tanabe m_possessionManager;
    // セットパーツ
    private SetPart_Tanabe m_setPart = null;
    // 右手に所持しているアイテム
    private ItemStateMachine m_rightHandsItem = null;

    // 現在のステート
    IPlayerState_Tanabe m_currentState;

    // Rigidbody
    private Rigidbody m_rb;
    // 着地判定
    private bool isGrounded;
    // 移動判定フラグ
    private bool m_isMoving = false;
    // デフォルト状態かの判定フラグ
    private bool m_isDefaultState = true;

    private float m_prevShotButton = 0.0f;

    // ジャンプリクエスト
    private bool m_jumpRequest = false;

    // エイム状態か
    [SyncVar] private bool m_isAiming = false;

    // Throw状態かの判定フラグ
    [SyncVar] private bool m_isThrow = false;

    // 爆発が当たっているか
    [SyncVar] private bool m_isHitBomb = false;

    // 重力
    [SyncVar] private float m_prevGravity = 0.0f;

    // ＜関数＞ーーーーーーーーーーーーーーーーーーーーーーーー

    // 開始関数
    public override void OnStartClient()
    {
        // キャラクタータイプの設定
        base.SetCharacterType(CharacterType.HERO_TYPE);
        base.OnStartClient();
        // Rigidbodyをアタッチする
        m_rb = GetComponent<Rigidbody>();

        if (!this.isLocalPlayer) { return; }
        m_cameraTransform = GameObject.FindWithTag("MainCamera").transform;

        // アイテムマネージャをアタッチする
        m_possessionManager = GetComponent<PossessionManager_Tanabe>();

        // 初期のステートの設定
        ChangeState(new IdleState(this));

        if (m_weaponID == WeaponID.HAMMER)
        {
            //一時停止 this.gameObject.GetComponentInChildren<DebugAttacker>().SetParentCharacter(this);
        }
    }

    // 更新関数

    public override void Update()
    {
        if (!this.isLocalPlayer) { return; }

        if (Input.GetKeyDown(KeyCode.P))
        {
            this.transform.position = new Vector3(0f, 2f, 0f);
            m_rb.velocity = Vector3.zero;
            base.SetHp(base.GetMaxHP());
        }

        base.Update();
        // 現在のステートの更新処理
        m_currentState?.Update();

        // 移動していない状態ならスタミナを回復する
        if (!m_isMoving)
        {
            base.SetStamina(GetStamina() + Time.deltaTime);
        }

        // 着地判定処理
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);

        //// デバッグ用にRayをSceneビューで可視化
        //Debug.DrawRay(transform.position, Vector3.down * 1.1f, isGrounded ? Color.green : Color.red);

        // ジャンプ処理
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            m_jumpRequest = true;
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
        // 現在のステートの更新処理
        m_currentState?.FixedUpdate();


        // ジャンプの指示が出たときのみ通す
        if (m_jumpRequest)
        {
            m_rb.AddForce(Vector3.up * 5f, ForceMode.Impulse);
            m_jumpRequest = false;
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

    // 現在のステートの変更
    public void ChangeState(IPlayerState_Tanabe newState)
    {
        // 現在のステートの終了処理
        m_currentState?.Exit();
        // 新たなステートの設定
        m_currentState = newState;
        // 新たなステートの開始処理
        m_currentState.Enter();
    }

    // 攻撃準備処理
    public void AttackCharge()
    {
        m_hammer.AttackCharge();
    }

    // 攻撃処理
    public void Attack()
    {
        m_hammer.Attack();
    }

    // 攻撃解除処理
    public void ExitAttack()
    {
        m_hammer.ExitAttack();
    }

    // お菓子のポイントを受け取る
    public void AddPoint(float point)
    {
        // マネージャ等にポイントを渡す
    }

    // ＜ゲッター関数＞ーーーーーーーーーーーーーーーーーーーー

    // 武器IDの取得
    public WeaponID GetWeaponID()
    {
        return m_weaponID;
    }

    // CameraTransform.forwardの取得
    public Vector3 GetCameraForward()
    {
        return m_cameraTransform.forward;
    }

    // 銃口の取得
    [Client]
    public Transform GetGunHead()
    {
        return m_gun.GetGunHead();
    }

    // エイム状態か
    public bool GetIsAiming()
    {
        return m_isAiming;
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

    // アイテムマネージャの取得
    public PossessionManager_Tanabe GetPossesionManager()
    {
        return m_possessionManager;
    }

    // セットパーツの取得
    public SetPart_Tanabe GetPart()
    {
        return m_setPart;
    }

    // セットパーツのタイプの取得
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

    // 右手に持っているアイテムの取得
    public ItemStateMachine GetRightHandsItem()
    {
        return m_rightHandsItem;
    }

    // 着地判定の取得
    public bool GetIsGrounded()
    {
        return isGrounded;
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

    // デフォルト状態かの判定フラグの取得
    public bool GetIsDefaultState()
    {
        return m_isDefaultState;
    }

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


    // ＜セッター関数＞ーーーーーーーーーーーーーーーーーーーー

    // セットパーツの設定
    public void SetPart(SetPart_Tanabe _setPart)
    {
        global::SetPart_Tanabe.PartType prevPartType = global::SetPart_Tanabe.PartType.NONE_TYPE;
        if (m_setPart != null)
        {
            prevPartType = m_setPart.GetPartType();
        }

        // 使用武器がハンマーなら通す
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

            // 新たなセットパーツを装備する
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
            // セットパーツを外す
            else
            {
                m_hammer.SetPartType(global::SetPart_Tanabe.PartType.NONE_TYPE);
            }
        }
        // セットパーツを変更・解除する
        m_setPart = _setPart;
    }

    // 右手に持つアイテムの設定
    public void SetRightHandsItem(ItemStateMachine _item)
    {
        m_rightHandsItem = _item;
    }

    // 移動判定フラグの設定
    public void SetIsMoving(bool _flag)
    {
        m_isMoving = _flag;
    }

    // デフォルト状態かの判定フラグの設定
    public void SetIsDefaultState(bool _flag)
    {
        m_isDefaultState = _flag;
    }

    public void SetPrevShotButton(float _shot)
    {
        m_prevShotButton = _shot;
    }

    // エイム状態かの設定
    public void SetIsAiming(bool _flag)
    {
        m_isAiming = _flag;
    }

    // Throw状態かの判定フラグの設定
    public void SetIsThrow(bool _flag)
    {
        m_isThrow = _flag;
        if (m_weaponID == WeaponID.HAMMER)
        {
            m_hammer.SetIsThrow(_flag);
        }
    }

    // 爆弾が当たっているかの設定
    public void SetIsHitBomb(bool _flag)
    {
        m_isHitBomb = _flag;
    }
}
