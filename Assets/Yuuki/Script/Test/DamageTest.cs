using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTest : MonoBehaviour
{
    [Header("このオブジェクトが与えるダメージ量")]
    public int damage = 10;

    private void OnTriggerEnter(Collider other)
    {

            // CharacterBaseを継承したクラスを取得
            CharacterBaseY player = other.GetComponent<CharacterBaseY>();
            if (player != null)
            {
                // ダメージ処理を呼び出す
                player.Damage(damage);
                Debug.Log($"[DamageTestObject] {other.name} に {damage} ダメージを与えた！ 残HP:{player.GetHp()}");
            }
        }
}
