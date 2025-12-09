using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Damageable<T> : NetworkBehaviour where T : DamageableSO
{
    [SerializeField] protected T _damageableData;
    public float _atk { get { return _damageableData._atk; } }
    public float _maxHP { get { return _damageableData._maxHP; } }
    protected float _hp;
    public float _currentHP { get { return _hp; } }
    protected float _hpPercent => _currentHP / _maxHP;

    protected virtual void Awake()
    {
        _hp = _maxHP;
    }
    public virtual void TakeDamage(float damage, GameObject damager)
    {
        Debug.Log($"{name} take {damage} damage");
        _hp = Mathf.Max(0, _hp - damage);
        if (_hp <= 0)
        {
            Debug.Log(name + " die");
            Destroy(gameObject);
        }
    }
}
