using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using System;

public class EnemyScript : Damageable<EnemySO>
{
    protected EnemySO _enemyData => _damageableData;
    [SerializeField] bool _debug;

    public float _detectRange { get { return _enemyData._detectRange; } }
    public float _atkCooldown { get { return _enemyData._atkCooldown; } }
    private float _atkTimer = 0f;
    public float _atkPlayerCooldown { get { return _enemyData._atkPlayerCooldown; } }
    private bool _isAttacking = false;
    private EnemyScript _helping = null;
    private float _helpDist => _enemyData._assistDistance;
    private float _spd => _enemyData._spd;
    private bool _callingHelp = false;
    private int _soloLimit => _enemyData._soloLimit;
    private int _soloOK => _enemyData._removeHelp;
    private Dictionary<PlayerScriptA, float> _playerDistDict = new();
    protected EnemyState _currState;
    protected List<EnemyState> _allStates = new();
    private bool _isChasing = false;
    private Vector3 _tagLocation;

    //助けを呼ぶ・消す
    public static event System.Action<EnemyScript> OnEnemyNeedHelp;
    public static event System.Action<EnemyScript> OnEnemyNoNeedHelp;

    protected override void Awake()
    {
        base.Awake();

        _currState = new IdleState(this);
        _allStates.Add(_currState);
        _allStates.Add(new CallHelpState(this));
        _allStates.Add(new AttackState(this));
        _allStates.Add(new GoHelpState(this));
        _allStates.Add(new TagState(this));
    }
    protected virtual void OnEnable()
    {
        OnEnemyNeedHelp += RespondToHelp;
        OnEnemyNoNeedHelp += RespondToStopHelp;
    }
    protected virtual void OnDisable()
    {
        OnEnemyNeedHelp -= RespondToHelp;
        OnEnemyNoNeedHelp -= RespondToStopHelp;
    }
    protected virtual void Update()
    {
        DetectPlayers();
        UpdateState();
    }
    private void UpdateState()
    {
        _currState.UpdateState();
        if (_currState.CheckTransition(out Type nextStateType))
        {
            foreach (EnemyState state in _allStates)
            {
                if (state.GetType() == nextStateType)
                {
                    _currState.ExitState();
                    _currState = state;
                    state.EnterState();
                }
            }
        }
    }
    private void DetectPlayers()
    {
        if (_atkTimer > 0f)
            return;
        Collider[] colls = Physics.OverlapSphere(transform.position, _detectRange);
        _playerDistDict.Clear();
        foreach (Collider coll in colls)
        {
            if (coll.TryGetComponent(out PlayerScriptA p))
            {
                Vector3 dist = transform.position - p.transform.position;
                dist.y = 0f;
                _playerDistDict.Add(p, dist.magnitude);
            }
        }
    }
    private void Attack()
    {
        if (_atkTimer > 0f)
            return;
        Dictionary<PlayerScriptA, float> playerNearToFar =
            (from p in _playerDistDict orderby p.Value ascending select p).ToDictionary(x => x.Key, x => x.Value);
        _atkTimer = _atkCooldown;
        StartCoroutine(AttackNearToFar(_playerDistDict.Keys.ToList(), _atkPlayerCooldown));
    }
    private void CallHelp()
    {
        _callingHelp = true;
        OnEnemyNeedHelp?.Invoke(this);
    }
    private void CancelHelp()
    {
        _callingHelp = false;
        OnEnemyNoNeedHelp?.Invoke(this);
    }
    private IEnumerator AttackNearToFar(List<PlayerScriptA> playersNearToFar, float interval, int index = 0)
    {
        if (index >= playersNearToFar.Count)
        {
            yield return null;
            _isAttacking = false;
            StartCoroutine(CooldownAttack());
        }
        else
        {
            _isAttacking = true;
            PlayerScriptA target = playersNearToFar[index];
            if (target != null)
            {
                target.TakeDamage(_atk, gameObject);
                yield return new WaitForSeconds(interval);
            }
            StartCoroutine(AttackNearToFar(playersNearToFar, interval, ++index));
        }
    }
    private IEnumerator CooldownAttack()
    {
        yield return new WaitForSeconds(_atkCooldown);
        _atkTimer = 0f;
    }
    private void RespondToHelp(EnemyScript otherEnemy)
    {
        if (_isAttacking || _isChasing || _helping)
            return;
        _helping = otherEnemy;
    }
    private void RespondToStopHelp(EnemyScript otherEnemy)
    {
        if (_helping && _helping == otherEnemy)
            _helping = null;
    }
    private bool GoHelp()
    {
        Vector3 toHelping = _helping.transform.position - transform.position;
        toHelping.y = 0;
        if (toHelping.magnitude >= _helpDist)
        {
            toHelping.Normalize();
            //後でナヴメッシュとかにします
            transform.position += Time.deltaTime * _spd * toHelping;
            return false;
        }
        else
            return true;
    }
    public override void TakeDamage(float damage, GameObject damager)
    {
        base.TakeDamage(damage, damager);

        Vector3 toDamager = damager.transform.position - transform.position;
        toDamager.y = 0f;
        //タグをつける    
        if (toDamager.magnitude > _detectRange)
        {
            _isChasing = true;
            _tagLocation = damager.transform.position;
            _tagLocation.y = transform.position.y;
        }
    }
    private bool ChaseTag()
    {
        Vector3 toTag = _tagLocation - transform.position;
        toTag.y = 0f;
        if (toTag.magnitude <= 0.7f)
        {
            return true;
        }
        else
        {
            toTag.Normalize();
            //後でナヴメッシュとかにします
            transform.position += Time.deltaTime * _spd * toTag;
            return false;
        }
    }
    private void Untag()
    {
        _isChasing = false;
    }

    private void OnDrawGizmos()
    {
        if (!_debug)
            return;

        //攻撃範囲
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _detectRange);

        //助けを呼んでいる
        Gizmos.color = Color.yellow;
        if (_callingHelp)
            Gizmos.DrawCube(transform.position + transform.up, Vector3.one);

        Gizmos.color = Color.cyan;
        //助けに行く
        if (_helping)
            Gizmos.DrawLine(transform.position, _helping.transform.position);

        //タグ場所
        if (_isChasing)
            Gizmos.DrawLine(transform.position, _tagLocation);
    }

    //--------------------------------------//
    public abstract class EnemyState
    {
        protected EnemyScript _enemy;
        public EnemyState(EnemyScript enemy)
        {
            _enemy = enemy;
        }
        public virtual void EnterState() { }
        public virtual void ExitState() { }
        public abstract void UpdateState();
        public abstract bool CheckTransition(out Type nextStateType);
    };
    public class IdleState : EnemyState
    {
        public IdleState(EnemyScript enemy) : base(enemy) { }
        public override bool CheckTransition(out Type nextStateType)
        {
            if (_enemy._isChasing)
            {
                nextStateType = typeof(TagState);
                return true;
            }
            if (_enemy._helping)
            {
                nextStateType = typeof(GoHelpState);
                return true;
            }
            int playersInRange = _enemy._playerDistDict.Count;
            if (playersInRange >= 0)
            {
                nextStateType = typeof(AttackState);
                return true;
            }
            nextStateType = typeof(IdleState);
            return false;
        }
        public override void UpdateState() { }
    }
    public class CallHelpState : EnemyState
    {
        public CallHelpState(EnemyScript enemy) : base(enemy) { }
        public override bool CheckTransition(out Type nextStateType)
        {
            int playersInRange = _enemy._playerDistDict.Count;
            if (playersInRange <= _enemy._soloOK)
            {
                if (playersInRange <= 0)
                    nextStateType = typeof(IdleState);
                else
                    nextStateType = typeof(AttackState);
                return true;
            }
            nextStateType = typeof(CallHelpState);
            return false;
        }

        public override void UpdateState()
        {
            _enemy.CallHelp();
            _enemy.Attack();
        }
        public override void ExitState()
        {
            _enemy.CancelHelp();
        }
    }
    public class AttackState : EnemyState
    {
        public AttackState(EnemyScript enemy) : base(enemy) { }
        public override bool CheckTransition(out Type nextStateType)
        {
            int playersInRange = _enemy._playerDistDict.Count;
            if (playersInRange >= _enemy._soloLimit)
                nextStateType = typeof(CallHelpState);
            else if (playersInRange <= 0)
                nextStateType = typeof(IdleState);
            else
            {
                nextStateType = typeof(AttackState);
                return false;
            }
            return true;
        }

        public override void UpdateState()
        {
            _enemy.Attack();
        }
    }
    public class GoHelpState : EnemyState
    {
        public GoHelpState(EnemyScript enemy) : base(enemy) { }
        public override bool CheckTransition(out Type nextStateType)
        {
            if (!_enemy._helping)
            {
                nextStateType = typeof(IdleState);
                return true;
            }
            else
            {
                nextStateType = typeof(GoHelpState);
                return false;
            }
        }

        public override void UpdateState()
        {
            if (_enemy.GoHelp())
                _enemy.Attack();
        }
    }
    public class TagState : EnemyState
    {
        public TagState(EnemyScript enemy) : base(enemy) { }

        public override bool CheckTransition(out Type nextStateType)
        {
            //着いた
            if (_enemy.ChaseTag())
            {
                int playersInRange = _enemy._playerDistDict.Count;
                if (playersInRange > 0)
                    nextStateType = typeof(AttackState);
                else
                    nextStateType = typeof(IdleState);
                return true;
            }
            //まだ着いていない
            else
            {
                nextStateType = typeof(TagState);
                return false;
            }
        }

        public override void UpdateState() { }
        public override void ExitState()
        {
            _enemy.Untag();
        }
    }
}
