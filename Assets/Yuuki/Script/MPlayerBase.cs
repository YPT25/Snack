using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MPlayerBase : EnemyBase
{
    [Header("プレイヤー共通設定")]
    [Header("マウス感度")]
    [SerializeField] protected float mouseSensitivity = 3.0f;
    [Header("プレイヤー回転の補間速度")]
    [SerializeField] protected float rotationSmooth = 10f;   

    protected Vector3 m_inputDir;
    protected Rigidbody m_rb;
    protected float yaw;   // 横回転
    protected float pitch; // 縦回転
    protected Camera cam;
    private TestRespawn respawnManager;

    public override void Start()
    {
        base.Start();
        m_rb = GetComponent<Rigidbody>();
        cam = Camera.main;

        // RespawnManagerを自動取得
        respawnManager = FindObjectOfType<TestRespawn>();
        if (respawnManager == null)
        {
            Debug.LogWarning("[MPlayerBase] RespawnManagerがシーンに存在しません。");
        }
    }

    public override void Update()
    {
        base.Update();
        HandleInput();
        HandleCamera();
        if (GetHp() <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 入力処理（WASD / Shift / マウス）
    /// </summary>
    protected virtual void HandleInput()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        m_inputDir = new Vector3(h, 0, v).normalized;

        if (Input.GetMouseButtonDown(0))
        {
            OnAttackInput();
        }
    }

    protected virtual void FixedUpdate()
    {
        Move();
    }

    /// <summary>
    /// カメラ制御とプレイヤー回転
    /// </summary>
    protected virtual void HandleCamera()
    {
        if (cam == null) return;

        // マウス入力を回転角に反映
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -30f, 60f); // 上下制限（お好みで調整）

        // プレイヤーを中心にオフセットしたカメラ位置を計算
        Vector3 offset = Quaternion.Euler(pitch, yaw, 0) * new Vector3(0, 2f, -4f);
        cam.transform.position = transform.position + offset;

        // カメラをプレイヤーに向ける
        cam.transform.LookAt(transform.position + Vector3.up * 1.5f);

        // プレイヤーのY回転はカメラの水平方向に追従させる
        Quaternion targetRot = Quaternion.Euler(0, yaw, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSmooth);
    }


    /// <summary>
    /// 移動処理（カメラ基準）
    /// </summary>
    protected virtual void Move()
    {
        if (m_rb == null) return;

        // カメラ基準で移動方向を計算
        Vector3 camForward = cam.transform.forward;
        camForward.y = 0;
        camForward.Normalize();
        Vector3 camRight = cam.transform.right;
        camRight.y = 0;
        camRight.Normalize();
        Vector3 moveDir = (camForward * m_inputDir.z + camRight * m_inputDir.x).normalized;

        // 通常速度 or ダッシュ速度
        float speed = GetMoveSpeed();
        // 移動適用
        Vector3 velocity = moveDir * speed;
        m_rb.velocity = new Vector3(velocity.x, m_rb.velocity.y, velocity.z);
    }

    /// <summary>
    /// プレイヤーの死亡処理（非破壊）
    /// </summary>
    public override void Die()
    {
        // 共通の死亡処理（EnemyBase側でDestroyが呼ばれる前に）
        Debug.Log($"{name} が死亡しました。");

        // RespawnManagerを呼ぶ
        if (respawnManager != null)
        {
            respawnManager.OnPlayerDeath();
        }
        else
        {
            Debug.LogWarning("[MPlayerBase] RespawnManagerが見つからなかったため、リスポーンできません。");
        }

        // Destroyはここで最後に呼ぶ（親のDieではDestroyが入っているなら不要）
        base.Die();
    }

    /// <summary>
    /// 攻撃入力時の処理（派生クラスで上書きする）
    /// </summary>
    protected virtual void OnAttackInput()
    {
        Debug.Log($"{name} が攻撃入力");
    }
}
