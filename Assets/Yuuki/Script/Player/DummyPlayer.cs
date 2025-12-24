using Mirror;
using System.Collections;
using UnityEngine;

public class DummyPlayer : MPlayerBase
{
    [Server]
    private IEnumerator Start()
    {
        yield return new WaitUntil(() =>
            RespawnSystem.GetAliveEnemyTypes().Count > 0
        );

        SetHp(0);
        Die();
    }

    public override void Update() { }
}