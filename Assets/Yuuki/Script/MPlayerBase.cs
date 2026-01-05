using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

/// <summary>
/// プレイヤー共通クラス（Mirror対応）
/// - すべてのプレイヤーキャラはこれを継承する
/// - HP管理、移動、攻撃、カメラ制御などを担当
/// - 死亡時にRespawnManagerを呼び出し、UIから再選択可能
/// </summary>
public class MPlayerBase : EnemyBase
{
    [Header("プレイヤー共通設定")]
    [SerializeField] protected float mouseSensitivity = 3.0f;
    [SerializeField] protected float rotationSmooth = 10f;
    [SerializeField] private Sprite m_respawnIcon;

    // Pad 用設定（Editorで微調整できるようにした）
    [Header("Pad（右スティック）設定")]
    [SerializeField] private float padSensitivityMultiplier = 20f;
    [SerializeField] private float padDeadZone = 0.02f;
    [SerializeField] private bool padInvertY = true;

    protected Vector3 m_inputDir;
    protected Rigidbody m_rb;
    protected float yaw;
    protected float pitch;
    protected Camera cam;

    private bool isInitialized = false;
    [SyncVar(hook = nameof(OnDeadStateChanged))] private bool isDead = false;
    // クライアント専用（同期しない）
    private bool localDead = false;
    [SyncVar] private bool canControl = true;

    public bool iscanMove = true;
    private bool isFarstDead = false;

    // ===== FPS視点用 =====
    [Header("FPS視点設定")]
    [SerializeField] private Transform fpsCameraPoint;
    [SerializeField] private Vector3 tpsCameraOffset = new Vector3(0, 2f, -4f);

    [SerializeField] private float tpsFOV = 60f;
    [SerializeField] private float fpsFOV = 75f;
    private bool isFPS = false;
    // FPS時に自分の体を消すため
    private MeshRenderer[] myRenderers;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        Debug.Log($"[Player] OnStartLocalPlayer name={name}");
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        Debug.Log($"[Player] OnStartAuthority name={name}");
    }

    public override void Start()
    {

        Debug.Log($"[Player] Start name={name} isLocal={isLocalPlayer} isClient={isClient} isServer={isServer}");

        base.Start();
        m_rb = GetComponent<Rigidbody>();

        // FPS時に体を非表示にするため
        myRenderers = GetComponentsInChildren<MeshRenderer>();

        // ローカルプレイヤー専用初期化
        if (isLocalPlayer)
        {
            cam = Camera.main;
            StartCoroutine(InitializeAfterDelay());
        }
        if (isLocalPlayer)
        {
            Debug.Log(
                $"[LOCAL STATE] dead={isDead} canMove={iscanMove} rbNull={m_rb == null} camNull={cam == null}"
            );
        }
    }

    private IEnumerator InitializeAfterDelay()
    {
        yield return new WaitForSeconds(0.3f);
        isInitialized = true;
        Debug.Log($"[MPlayerBase] {name}: Initialize完了 (isLocalPlayer={isLocalPlayer})");
        Debug.Log($"[Server] CmdRequestRespawn called. TETETET");
        // LegacyInputHelper のパッド設定を同期（エディタでここを変えられる）
        LegacyInputHelper.padSensitivityMultiplier = padSensitivityMultiplier;
        LegacyInputHelper.padDeadZone = padDeadZone;
        LegacyInputHelper.invertPadY = padInvertY;
    }

    public override void Update()
    {
        base.Update();


        if (!isLocalPlayer || !isInitialized || isDead)
            return;

        if (localDead) return;

        HandleInput();
        HandleCamera();

        // マウスの埋め込み
        if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            CmdRequestKill();
        }
        //if (GetHp() <= 0)
        //{
        //    Die();
        //}
        if (Input.GetKeyDown(KeyCode.R)) { base.Damage(10); }

        if (!GetIsMove())
        {
            iscanMove = false;
        }
    }

    public virtual void FixedUpdate()
    {
        if (!isLocalPlayer || !isInitialized || isDead)
            return;
        if (localDead) return;

        Move();
    }

    protected virtual void HandleInput()
    {
        // ====== 移動入力（共通化）======
        Vector2 moveAxis = LegacyInputHelper.GetMoveAxis();
        m_inputDir = new Vector3(moveAxis.x, 0, moveAxis.y).normalized;

        // ====== 攻撃 ======
        if (LegacyInputHelper.GetAttackDown())
            CmdAttackInput();

        // ====== FPS/TPS切り替え（右クリック or Pad L2） ======
        bool aiming = LegacyInputHelper.GetAim();
        SetFPS(aiming);

        // マウスカーソル処理（Altキーで解除）
        if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // FPS視点への切り替え
    private void SetFPS(bool flag)
    {
        isFPS = flag;

        // FPS時はモデルを非表示に
        foreach (var r in myRenderers)
            r.enabled = !flag;

        // FOV切り替え
        if (cam != null)
        {
            cam.fieldOfView = isFPS ? fpsFOV : tpsFOV;
        }
    }

    [Command]
    private void CmdAttackInput()
    {
        OnAttackInput();
    }

    /// <summary>
    /// カメラとプレイヤーの回転処理
    /// </summary>
    protected virtual void HandleCamera()
    {
        if (cam == null) return;

        // LegacyInputHelper 側で padDeadZone / padSensitivity 等を反映している
        Vector2 lookAxis = LegacyInputHelper.GetLookAxis();

        // ここでマウスSensitivity を適用（padで戻された値は既に padMultiplier がかかっている）
        float mouseX = lookAxis.x * mouseSensitivity;
        float mouseY = lookAxis.y * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -80f, 85f);

        // ===== プレイヤー水平回転 =====
        Quaternion targetRot = Quaternion.Euler(0, yaw, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSmooth);

        // ===== FPS視点 =====
        if (isFPS)
        {
            if (fpsCameraPoint != null)
            {
                cam.transform.position = fpsCameraPoint.position;
                cam.transform.rotation = Quaternion.Euler(pitch, yaw, 0);
            }
            return;
        }

        // ===== TPS視点 =====
        Vector3 offset = Quaternion.Euler(pitch, yaw, 0) * tpsCameraOffset;
        cam.transform.position = transform.position + offset;
        cam.transform.LookAt(transform.position + Vector3.up * 1.5f);
    }

    protected virtual void Move()
    {
        if (m_rb == null || cam == null) return;
        if (iscanMove)
        {
            Vector3 camForward = cam.transform.forward;
            camForward.y = 0;
            camForward.Normalize();

            Vector3 camRight = cam.transform.right;
            camRight.y = 0;
            camRight.Normalize();

            Vector3 moveDir = (camForward * m_inputDir.z + camRight * m_inputDir.x).normalized;

            float speed = GetMoveSpeed();
            Vector3 velocity = moveDir * speed;

            m_rb.velocity = new Vector3(velocity.x, m_rb.velocity.y, velocity.z);
        }
        else
        {
            if (m_rb != null)
                m_rb.velocity = Vector3.zero; // ピタッと止める
            return;
        }
    }

    //ダメージ
    public override void Damage(float _damage)
    {
        base.Damage(_damage);

        if (!isServer) return;

        if (GetHp() <= 0)
        {
            TargetSetLocalDead(connectionToClient);
            Die();
        }
    }

    [Server]
    public void ServerResetForRespawn()
    {
        // 正式な状態
        isDead = false;
        canControl = true;

        // HP
        SetHp(GetMaxHP());

        // クライアント即時リセット
        TargetResetLocalState(connectionToClient);
    }
    //クライアント用状態リセット
    [TargetRpc]
    private void TargetResetLocalState(NetworkConnection target)
    {
        localDead = false;

        yaw = 0f;
        pitch = 0f;
        // ★追加：死んだ時にenabled=falseしたのを戻す
        enabled = true;

        // ★追加：初期化が止まってた場合に備える（安全策）
        if (!isInitialized) StartCoroutine(InitializeAfterDelay());


        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    //クライアント専用の死亡通知処理
    [TargetRpc]
    private void TargetSetLocalDead(NetworkConnection target)
    {
        localDead = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // ===== 死亡処理 =====
    [Server]
    public override void Die()
    {
        if (isDead) return;
        isDead = true;

        EnemyType[] types;

        // 初回（DummyPlayer）
        bool isFirst = (GetEnemyType() == EnemyType.TYPE_NULL);

        if (isFirst)
        {
            types = RespawnSystem.GetAllPlayerTypes();
            Debug.Log("[Respawn] First respawn → ALL player types");
        }
        else
        {
            types = RespawnSystem.GetAliveEnemyTypes().ToArray();
            Debug.Log($"[Respawn] Normal respawn → alive={types.Length}");
        }

        TargetShowRespawnUI(connectionToClient, types);
    }


    [TargetRpc]
    private void TargetShowRespawnUI(
        NetworkConnection target,
        EnemyType[] allowedTypes)
    {
        Debug.Log($"[Respawn] TargetShowRespawnUI called. types={allowedTypes.Length}");

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        RespawnManager.Instance.Show(this, allowedTypes);
    }

    [ClientRpc]
    protected void RpcSetDeadState(bool value)
    {
        if (value && m_rb != null)
            m_rb.velocity = Vector3.zero;
    }


    [Command]
    public void CmdRequestRespawn(EnemyType type)
    {
        Debug.Log($"[Server] CmdRequestRespawn called. type={type}");
        Debug.Log($"[Respawn] CmdRequestRespawn type={type} " +
          $"hasConn={connectionToClient != null} " +
          $"hasIdentity={connectionToClient?.identity != null}");

        RespawnSystem.ServerRespawn(connectionToClient, type);
    }

    protected virtual void OnAttackInput()
    {
        Debug.Log($"{name} Attack Input");
    }

    private void CursorController()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    //クライアント用の死亡時停止処理
    private void OnDeadStateChanged(bool oldValue, bool newValue)
    {
        if (!isLocalPlayer) return;

        if (newValue) // 死亡
        {
            // 入力・カメラ完全停止
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // 回転を止める
            enabled = false; // ← 最強
        }
    }

    //デバック用
    [Command]
    private void CmdRequestKill()
    {
        // サーバーで確実にHPを0にする
        Damage(GetHp());
    }

    public Sprite GetRespawnIcon() => m_respawnIcon;
    //視点の状態を渡す
    public bool GetIsFPS() => isFPS;
}