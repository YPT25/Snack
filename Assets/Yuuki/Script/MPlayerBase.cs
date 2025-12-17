using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// プレイヤー共通クラス（Mirror対応）
/// - すべてのプレイヤーキャラはこれを継承する
/// - HP管理、移動、攻撃、カメラ制御などを担当
/// - 死亡時にRespawnManagerを呼び出し、UIから再選択可能
/// </summary>
public class MPlayerBase : EnemyBase
{
    // =========================
    // 共通設定
    // =========================
    [Header("プレイヤー共通設定")]
    [SerializeField] protected float mouseSensitivity = 3.0f;
    [SerializeField] protected float rotationSmooth = 10f;
    [SerializeField] private Sprite m_respawnIcon;

    // =========================
    // Pad 設定（ローカル専用）
    // =========================
    [Header("Pad 設定")]
    [SerializeField] private float padSensitivityMultiplier = 20f;
    [SerializeField] private float padDeadZone = 0.02f;
    [SerializeField] private bool padInvertY = true;

    // =========================
    // 同期用（Server Authority）
    // =========================
    [SyncVar] protected Vector3 m_syncMoveDir;
    [SyncVar] protected float m_syncYaw;
    [SyncVar] protected bool m_isMoving;

    // =========================
    // 内部状態
    // =========================
    protected Rigidbody m_rb;
    protected Camera cam;

    protected float yaw;
    protected float pitch;

    protected bool isInitialized = false;
    protected bool isDead = false;
    public bool iscanMove = true;

    // =========================
    // FPS / TPS
    // =========================
    [Header("視点設定")]
    [SerializeField] private Transform fpsCameraPoint;
    [SerializeField] private Vector3 tpsCameraOffset = new Vector3(0, 2f, -4f);
    [SerializeField] private float tpsFOV = 60f;
    [SerializeField] private float fpsFOV = 75f;

    private bool isFPS = false;
    private MeshRenderer[] myRenderers;

    // =========================
    // 初期化
    // =========================
    public override void Start()
    {
        base.Start();

        m_rb = GetComponent<Rigidbody>();
        myRenderers = GetComponentsInChildren<MeshRenderer>();

        // 他人の Player は物理を止める
        if (!isLocalPlayer && m_rb != null)
            m_rb.isKinematic = true;

        if (isLocalPlayer)
        {
            cam = Camera.main;
            StartCoroutine(InitializeAfterDelay());
        }
    }

    private IEnumerator InitializeAfterDelay()
    {
        yield return new WaitForSeconds(0.3f);
        isInitialized = true;

        // Pad 設定を LegacyInputHelper に反映
        LegacyInputHelper.padSensitivityMultiplier = padSensitivityMultiplier;
        LegacyInputHelper.padDeadZone = padDeadZone;
        LegacyInputHelper.invertPadY = padInvertY;
    }

    // =========================
    // Client：入力・カメラ
    // =========================
    private void Update()
    {
        if (!isLocalPlayer || !isInitialized || isDead)
            return;

        HandleInput();
        HandleCamera();
        HandleCursor();
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer || !isInitialized || isDead)
            return;

        Move_Local();
    }

    // =========================
    // 入力
    // =========================
    protected virtual void HandleInput()
    {
        Vector2 moveAxis = LegacyInputHelper.GetMoveAxis();
        Vector3 dir = new Vector3(moveAxis.x, 0, moveAxis.y).normalized;

        bool aiming = LegacyInputHelper.GetAim();
        SetFPS(aiming);

        CmdSetMove(dir, yaw);

        if (LegacyInputHelper.GetAttackDown())
            CmdAttackInput();
    }

    [Command]
    private void CmdSetMove(Vector3 dir, float yawValue)
    {
        m_syncMoveDir = dir;
        m_syncYaw = yawValue;
        m_isMoving = dir.sqrMagnitude > 0.01f;
    }

    // =========================
    // Client：移動再生
    // =========================
    protected virtual void Move_Local()
    {
        if (m_rb == null) return;

        // ===== 移動入力なし / 移動不可 =====
        if (!m_isMoving || !iscanMove)
        {
            // ★ Y は絶対に触らない
            Vector3 v = m_rb.velocity;
            m_rb.velocity = new Vector3(0f, v.y, 0f);
            return;
        }

        Vector3 camForward = cam.transform.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 camRight = cam.transform.right;
        camRight.y = 0;
        camRight.Normalize();

        Vector3 moveDir =
            camForward * m_syncMoveDir.z +
            camRight * m_syncMoveDir.x;

        float speed = GetMoveSpeed();

        // ★ 重力を完全に尊重
        Vector3 currentVel = m_rb.velocity;
        Vector3 targetVel = new Vector3(
            moveDir.x * speed,
            currentVel.y,
            moveDir.z * speed
        );

        m_rb.velocity = targetVel;
    }

    // =========================
    // カメラ・回転
    // =========================
    protected virtual void HandleCamera()
    {
        if (cam == null) return;

        Vector2 lookAxis = LegacyInputHelper.GetLookAxis();

        yaw += lookAxis.x * mouseSensitivity;
        pitch -= lookAxis.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, -80f, 85f);

        transform.rotation = Quaternion.Euler(0, yaw, 0);

        if (isFPS && fpsCameraPoint != null)
        {
            cam.transform.position = fpsCameraPoint.position;
            cam.transform.rotation = Quaternion.Euler(pitch, yaw, 0);
            return;
        }

        Vector3 offset = Quaternion.Euler(pitch, yaw, 0) * tpsCameraOffset;
        cam.transform.position = transform.position + offset;
        cam.transform.LookAt(transform.position + Vector3.up * 1.5f);
    }

    private void SetFPS(bool flag)
    {
        isFPS = flag;

        foreach (var r in myRenderers)
            r.enabled = !flag;

        if (cam != null)
            cam.fieldOfView = isFPS ? fpsFOV : tpsFOV;
    }

    // =========================
    // 攻撃
    // =========================
    [Command]
    private void CmdAttackInput()
    {
        OnAttackInput();
    }

    protected virtual void OnAttackInput()
    {
        // 派生クラスで実装
    }

    // =========================
    // Server：死亡管理
    // =========================
    [ServerCallback]
    private void LateUpdate()
    {
        if (isDead) return;

        if (GetHp() <= 0)
            Die();
    }

    [Server]
    public override void Die()
    {
        if (isDead) return;
        isDead = true;

        RpcSetDeadState();
        TargetShowRespawnUI(connectionToClient);
    }

    [ClientRpc]
    private void RpcSetDeadState()
    {
        if (m_rb != null)
            m_rb.velocity = Vector3.zero;
    }

    [TargetRpc]
    private void TargetShowRespawnUI(NetworkConnection target)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        RespawnManager.Instance.ShowRespawnUI();
    }

    // =========================
    // Cursor
    // =========================
    private void HandleCursor()
    {
        if (Input.GetKey(KeyCode.LeftAlt))
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

    [Command]
    public void CmdRequestRespawn(int index)
    {
        RespawnManager.Instance.ServerRespawn(index, connectionToClient);
    }

    // =========================
    // Getter
    // =========================
    public Sprite GetRespawnIcon() => m_respawnIcon;
    public bool GetIsFPS() => isFPS;
}