using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TestHero : CharacterBase
{
    private CharacterType m_HeroCharacterType = CharacterType.HERO_TYPE;

    [Header("ランダム移動設定")]
    public float changeDirInterval = 2f;

    private Vector3 moveDir;
    private float timer;

    void Start()
    {
        if (isServer)
            SetCharacterType(m_HeroCharacterType);

        timer = changeDirInterval;
    }

    public override void Update()
    {
        if (!isServer) return;            // AIはサーバーのみ
        if (!GetIsMove()) return;         // 止められているなら動かない

        RandomMove();
    }

    void RandomMove()
    {
        timer -= Time.deltaTime;

        // 一定間隔で方向変更
        if (timer <= 0f)
        {
            moveDir = new Vector3(
                Random.Range(-1f, 1f),
                0f,
                Random.Range(-1f, 1f)
            ).normalized;

            timer = changeDirInterval;
        }

        // CharacterBase にある移動速度を使用
        transform.position += moveDir * GetMoveSpeed() * Time.deltaTime;
    }
}