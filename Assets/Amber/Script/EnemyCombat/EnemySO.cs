using Mirror;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "ScriptableObjects/Entity/Enemy/EnemyData")]
public class EnemySO : DamageableSO
{
    //範囲
    public float _detectRange;
    //助けに行く時に止める距離
    public float _assistDistance;

    //プレイヤー何人まで助けを呼ぶ
    public int _soloLimit;
    //プレイヤー何人に減らしたら助けの要求を消す
    public int _removeHelp;
    
    public float _atkCooldown;
    public float _atkPlayerCooldown;
    public float _spd;
}