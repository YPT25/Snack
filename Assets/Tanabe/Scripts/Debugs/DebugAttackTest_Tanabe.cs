using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DebugAttackTest_Tanabe : NetworkBehaviour
{
    private CharacterBase m_parentCharacter;

    // Trigger衝突判定処理
    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (m_parentCharacter == null || other.isTrigger) { return; }

        // キャラクターデータの取得
        CharacterBase characterBase = other.GetComponent<CharacterBase>();
        // キャラクターでなければreturnする
        if (characterBase == null || characterBase.GetCharacterType() != CharacterBase.CharacterType.ENEMY_TYPE) { return; }
        // 敵から攻撃力を取得してダメージとして計算する
        characterBase.RpcDamage(m_parentCharacter.GetPower());
    }

    // 親となるキャラクターオブジェクトの設定
    public void SetParentCharacter(CharacterBase _characterBase)
    {
        m_parentCharacter = _characterBase;
    }

    // 親となるキャラクターオブジェクトの設定
    [Command]
    public void CmdSetParentCharacter(CharacterBase _characterBase)
    {
        m_parentCharacter = _characterBase;
    }
}
