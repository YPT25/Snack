using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 各プレイヤーの動作（カセット）の共通基底クラス。
/// ネットワーク通信は行わず、ローカル挙動のみ担当する。
/// </summary>
public abstract class PlayerModule : MonoBehaviour
{
    protected MPlayerBase owner;
    protected Rigidbody rb;
    protected Camera cam;

    public virtual void OnAttach(MPlayerBase player)
    {
        owner = player;
        rb = GetComponent<Rigidbody>();
        cam = Camera.main;
    }

    public virtual void OnDetach() { }

    public abstract void HandleInput();
    public abstract void HandleMovement();

    public virtual void OnDeath()
    {
        owner.OnPlayerDeath();
    }
}