using Mirror;
using UnityEngine;

public class DummyPlayer : MPlayerBase
{
    public override void Start()
    {
        base.Start();

        if (isServer)
        {
            // 起動直後に即死亡 → リスポーンUIへ
            SetHp(0);
            Die();
        }
    }

    public override void Update() { }
}