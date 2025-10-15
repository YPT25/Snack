using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyBox_Player : MPlayerBase
{
    protected override void OnAttackInput()
    {
        // ターゲットを取得（仮：目の前にあるものをターゲットとする）
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 2.0f))
        {
            CharacterBase target = hit.collider.GetComponent<CharacterBase>();
            if (target != null)
            {
                Attack(target);
            }
        }
    }

    public override void Attack(CharacterBase target)
    {
        base.Attack(target); // ダメージ処理はベースに任せる（必要ならここで削除）

        // 拘束処理を追加
        RestrainComponent restrain = target.gameObject.AddComponent<RestrainComponent>();
        restrain.StartRestrain(10.0f); // 10秒間拘束
        Debug.Log($"{name} が {target.name} を拘束した！");
    }
}
