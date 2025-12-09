using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageableData", menuName = "ScriptableObjects/Entity/DamageableData")]
public class DamageableSO : ScriptableObject
{
    //戦闘スタッツ
    public float _atk;
    public float _maxHP;
}
