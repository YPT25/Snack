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
    public bool  iscanMove = true;

    // ===== FPS視点用 =====
    [Header("FPS視点設定")]
    // プレイヤー頭に置くポイント
    [SerializeField] private Transform fpsCameraPoint;  
    [SerializeField] private Vector3 tpsCameraOffset = new Vector3(0, 2f, -4f);

    [SerializeField] private float tpsFOV = 60f;
    [SerializeField] private float fpsFOV = 75f;
    private bool isFPS = false;
    // FPS時に自分の体を消すため
    private MeshRenderer[] myRenderers;  

    public override void Start()
    {
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
            SetHp(0);
        }
            if (GetHp() <= 0 && isServer)
        {
            Die();
        }
    }

    protected virtual void FixedUpdate()
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

        if (!Input.GetKeyDown(KeyCode.LeftAlt))
        {
            Cursor.visible = false;
        }

        // 左クリック攻撃
        if (Input.GetMouseButtonDown(0))
            CmdAttackInput();

        // FPS切替
        if (Input.GetMouseButtonDown(1))
        {
            SetFPS(true);
        }
        if (Input.GetMouseButtonUp(1))
        {
            SetFPS(false);
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

        float mouseX = Input.GetAxis("Camera X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Camera Y") * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -80f, 85f);

        // === プレイヤーの水平回転 ===
        Quaternion targetRot = Quaternion.Euler(0, yaw, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSmooth);

        // === FPS視点 ===
        if (isFPS)
        {
            cam.transform.position = fpsCameraPoint.position;
            cam.transform.rotation = Quaternion.Euler(pitch, yaw, 0);
            return;
        }

        // === TPS視点 ===
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
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

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

    private void CursorController()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
    public Sprite GetRespawnIcon() => m_respawnIcon;
    //視点の状態を渡す
    public bool GetIsFPS() => isFPS;
}