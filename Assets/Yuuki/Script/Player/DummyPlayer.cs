using Mirror;
using UnityEngine;

public class DummyPlayer : MPlayerBase
{
    public override void Start()
    {
        base.Start();

        if (isServer)
        {
            SetHp(0);
        }
    }

    public override void Update() { }
}