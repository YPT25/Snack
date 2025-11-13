using Mirror;
using UnityEngine;

public class DummyPlayer : MPlayerBase
{
    public override void Start()
    {
        base.Start();

        if (isServer)
        {
            // HPを0にして即死亡（UI表示をトリガー）
            SetHp(0);
            Die();
        }
    }

    public override void Update() { } // 操作無効
    protected override void OnAttackInput() { } // 攻撃無効
}