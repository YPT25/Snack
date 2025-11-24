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
public class Check_MPlayerBase_Tanabe : EnemyBase
{
    [Header("プレイヤー共通設定")]
    [SerializeField] protected float mouseSensitivity = 3.0f;
    [SerializeField] protected float rotationSmooth = 10f;
    [SerializeField] private Sprite m_respawnIcon;

    protected Vector3 m_inputDir;
    protected Rigidbody m_rb;
    protected float yaw;
    protected float pitch;
    protected Camera cam;

    private bool isInitialized = false;
    private bool isDead = false;

    // ===== ADS 用 =====
    [Header("ADS 設定")]
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float adsFOV = 35f;

    [SerializeField] private Vector3 normalCamOffset = new Vector3(0, 2f, -4f);
    [SerializeField] private Vector3 adsCamOffset = new Vector3(0.6f, 1.8f, -2.5f);

    [SerializeField] private float adsSensitivityMultiplier = 0.5f;
    private bool isADS = false;

    public override void Start()
    {
        base.Start();
        m_rb = GetComponent<Rigidbody>();

        // ローカルプレイヤー専用初期化
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
        Debug.Log($"[MPlayerBase] {name}: Initialize完了 (isLocalPlayer={isLocalPlayer})");
    }

    public override void Update()
    {
        base.Update();

        if (!isLocalPlayer || !isInitialized || isDead)
            return;

        HandleInput();
        HandleCamera();

        if (GetHp() <= 0 && isServer)
            Die();
    }

    public override void FixedUpdate()
    {
        if (!isLocalPlayer || !isInitialized || isDead)
            return;

        Move();
    }

    protected virtual void HandleInput()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        m_inputDir = new Vector3(h, 0, v).normalized;

        // 左クリック攻撃
        if (Input.GetMouseButtonDown(0))
            CmdAttackInput();

        // === ADS 入力 ===
        if (Input.GetMouseButtonDown(1))
            isADS = true;

        if (Input.GetMouseButtonUp(1))
            isADS = false;
    }

    [Command]
    private void CmdAttackInput()
    {
        OnAttackInput();
    }

    /// <summary>
    /// カメラとプレイヤーの回転処理（ADSあり）
    /// </summary>
    protected virtual void HandleCamera()
    {
        if (cam == null) return;

        // ADS中は感度を下げる
        float sens = isADS ? mouseSensitivity * adsSensitivityMultiplier : mouseSensitivity;

        float mouseX = Input.GetAxis("Camera X") * sens;
        float mouseY = Input.GetAxis("Camera Y") * sens;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -30f, 60f);

        // ===== カメラオフセット =====
        Vector3 offset = isADS ? adsCamOffset : normalCamOffset;

        // カメラ位置を決定
        Vector3 camOffset = Quaternion.Euler(pitch, yaw, 0) * offset;
        cam.transform.position = transform.position + camOffset;

        // カメラがプレイヤーの胸あたりを見るように調整
        cam.transform.LookAt(transform.position + Vector3.up);

        // FOV（ズーム）
        float targetFOV = isADS ? adsFOV : normalFOV;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * 8f);

        // プレイヤー自身の回転
        Quaternion targetRot = Quaternion.Euler(0, yaw, 0);
        Vector3 _baseRotation = transform.rotation.eulerAngles;
        if(Mathf.Abs(_baseRotation.x) <= 0.5f) { _baseRotation.x = 0f; }
        transform.rotation = Quaternion.Slerp(Quaternion.Euler(/*_baseRotation.x*/0f, _baseRotation.y, 0f), targetRot, Time.deltaTime * rotationSmooth);
        //transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSmooth);
    }

    protected virtual void Move()
    {
        if (m_rb == null || cam == null) return;

        Vector3 camForward = cam.transform.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 camRight = cam.transform.right;
        camRight.y = 0;
        camRight.Normalize();

        Vector3 moveDir = (camForward * m_inputDir.z + camRight * m_inputDir.x).normalized;
        //Vector3 moveDir = (transform.forward * m_inputDir.z + transform.right * m_inputDir.x).normalized;

        float speed = GetMoveSpeed();
        Vector3 velocity = moveDir * speed;

        //m_rb.velocity = new Vector3(velocity.x, m_rb.velocity.y, velocity.z);


        Vector3 _prevVelocity = m_rb.velocity;
        Vector3 velocityChange = velocity - new Vector3(_prevVelocity.x, 0f, _prevVelocity.z);

        // 着地判定処理
        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.3f);
        if(isGrounded)
        {
            velocityChange.y = 0f;
        }
        m_rb.AddForce(velocityChange / Time.fixedDeltaTime, ForceMode.Acceleration);

    }

    // ===== 死亡処理 =====
    [Server]
    public override void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"{name} が死亡。リスポーンUI表示へ");

        RpcSetDeadState(true);

        TargetShowRespawnUI(connectionToClient);
    }

    [TargetRpc]
    private void TargetShowRespawnUI(NetworkConnection target)
    {
        if (RespawnManager.Instance != null)
            RespawnManager.Instance.ShowRespawnUI();
        else
            Debug.LogWarning("RespawnManager が見つかりません");
    }

    [ClientRpc]
    protected void RpcSetDeadState(bool value)
    {
        if (value && m_rb != null)
            m_rb.velocity = Vector3.zero;
    }

    protected virtual void OnAttackInput()
    {
        Debug.Log($"{name} Attack Input");
    }

    public Sprite GetRespawnIcon() => m_respawnIcon;
}