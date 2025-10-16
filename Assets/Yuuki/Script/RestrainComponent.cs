using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 対象を「拘束状態」にするコンポーネント
/// ・移動や攻撃入力を封じる
/// ・一定時間後に解除され、自動で破棄される
/// </summary>
public class RestrainComponent : MonoBehaviour
{
    private bool isRestrained = false;

    public void StartRestrain(float duration)
    {
        if (!isRestrained)
        {
            isRestrained = true;
            StartCoroutine(RestrainCoroutine(duration));
        }
    }

    private IEnumerator RestrainCoroutine(float duration)
    {
        // 移動や攻撃を封じるためのフラグ
        // TODO: Player/NPC 側の Update 内で isRestrained をチェックして無効化
        Debug.Log($"{name} は拘束された！");

        yield return new WaitForSeconds(duration);

        Debug.Log($"{name} の拘束が解除された");
        Destroy(this);
    }
}
