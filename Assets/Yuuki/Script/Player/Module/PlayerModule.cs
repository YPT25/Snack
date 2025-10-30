using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �e�v���C���[�̓���i�J�Z�b�g�j�̋��ʊ��N���X�B
/// �l�b�g���[�N�ʐM�͍s�킸�A���[�J�������̂ݒS������B
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