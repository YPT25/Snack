using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ItemStateMachine : NetworkBehaviour
{
    public enum ItemType
    {
        NONE_TYPE,
        POINT,
        USE,
        THROW,
        TRAP,
        TRAP_BOMB,
        BUFF,
        SETPART,
    }

    public enum ItemStateType
    {
        DROP,
        SUCK,
        HANDS,
        TRAP,
        PREPARINGTHROW,
        THROW,
        PARTEQUIPPED,
    }

    // 現在のステート
    private IItemState_Tanabe currentState;
    [SyncVar, Header("アイテムの種類"), SerializeField] private ItemType m_itemType;
    [Header("移動速度 ※デフォルト値:30.0"), SerializeField, Range(0f, 100f)] public float moveSpeed;
    [Header("回転速度 ※デフォルト値:30.0"), SerializeField, Range(0f, 100f)] public float rotateSpeed;
    [Header("ポイント"), SerializeField, Range(0f, 100f)] private float m_point;

    private Rigidbody m_rb = null;
    private Collider m_collider = null;
    [SyncVar] private Transform playerTransform = null;
    [SyncVar] private Player_Tanabe m_playerData;
    [SyncVar] private ItemStateType m_stateType;

    [Header("バフの種類"), SerializeField] private BuffManager_Tanabe.Buff.BuffType m_buffType;

    [Header("エフェクトのオブジェクト"), SerializeField] private GameObject m_effectObject;

    // 開始関数
    void Start()
    {
        m_rb = GetComponent<Rigidbody>();
        m_collider = GetComponent<Collider>();
        Debug.Log("Item_Start");
        // 初期のステートの設定
        ChangeState(this, ItemStateType.DROP);

        if(m_itemType == ItemType.THROW)
        {
            // プレハブをGameObject型で取得
            //GameObject obj = (GameObject)Resources.Load("Explosion_2_Bomb_Yellow");

            m_effectObject = Instantiate(m_effectObject);
            m_effectObject.transform.parent = this.transform;
            m_effectObject.SetActive(false);
            m_effectObject.transform.localPosition = new Vector3(0f, 1f, 0f);
        }
        else if(m_itemType == ItemType.TRAP_BOMB)
        {
            // プレハブをGameObject型で取得
            //GameObject obj = (GameObject)Resources.Load("Explosion_2_Bomb_Purple");

            m_effectObject = Instantiate(m_effectObject);
            m_effectObject.transform.parent = this.transform;
            m_effectObject.SetActive(false);
            //m_effectObject.transform.localPosition = new Vector3(0f, 2f, 0f);
            m_effectObject.transform.localPosition = new Vector3(0f, 0f, 0f);
        }
    }

    // 更新関数
    [ServerCallback]
    void Update()
    {
        currentState?.Update();
    }

    [ClientRpc]
    public void RpcChangeState(ItemStateMachine _item, ItemStateType _newStateType)
    {
        //this.ChangeState(_item, _newStateType);
        if (_item != null)
        {
            _item.ChangeState(_item, _newStateType);
        }
    }

    [Command]
    public void CmdChangeState(ItemStateMachine _item, ItemStateType _newStateType)
    {
        //this.ChangeState(_item, _newStateType);
        if (_item != null)
        {
            // サーバー側でも状態変更
            _item.ChangeState(_item, _newStateType);

            // クライアント全員に反映
            RpcChangeState(_item, _newStateType);
        }
    }

    [ServerCallback]
    public void ServerChangeState(ItemStateMachine _item, ItemStateType _newStateType)
    {
        if (_item != null)
        {
            // サーバー側でも状態変更
            _item.ChangeState(_item, _newStateType);

            // クライアント全員に反映
            RpcChangeState(_item, _newStateType);
        }
    }

    // 現在のステートの変更
    public void ChangeState(ItemStateMachine _item, ItemStateType _newStateType)
    {
        m_stateType = _newStateType;
        switch(_newStateType)
        {
            case ItemStateType.DROP:
                this.ChangeState(new DropState(_item));
                break;
            case ItemStateType.SUCK:
                this.ChangeState(new SuckState(_item));
                break;
            case ItemStateType.HANDS:
                this.ChangeState(new HandsState(_item));
                break;
            case ItemStateType.TRAP:
                this.ChangeState(new TrapState(_item));
                break;
            case ItemStateType.PREPARINGTHROW:
                this.ChangeState(new PreparingThrowState(_item));
                break;
            case ItemStateType.THROW:
                this.ChangeState(new ThrowState(_item));
                break;
            case ItemStateType.PARTEQUIPPED:
                this.ChangeState(new PartEquippedState(_item));
                break;
            default:
                break;
        }
    }

    private void ChangeState(IItemState_Tanabe newState)
    {
        Debug.Log("Item_ChageState");
        // 現在のステートの終了処理
        currentState?.Exit();
        // 新たなステートの設定
        currentState = newState;
        // 新たなステートの開始処理
        currentState.Enter();
    }

    // isTrigger衝突判定
    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        // 現在のステートにisTrigger衝突が起きたことを通知する
        currentState?.OnTriggerEnter(other.gameObject);
    }

    [ServerCallback]
    private void OnTriggerStay(Collider other)
    {
        // 現在のステートにisTrigger衝突が起きたことを通知する
        currentState?.OnTriggerEnter(other.gameObject);
    }

    [ServerCallback]
    private void OnTriggerExit(Collider other)
    {
        // 現在のステートにisTrigger衝突が外れたことを通知する
        currentState?.OnTriggerExit(other);
    }

    [ServerCallback]
    private void OnCollisionStay(Collision collision)
    {
        if (m_itemType != ItemType.TRAP && m_itemType != ItemType.TRAP_BOMB) { return; }
        // 現在のステートにisTrigger衝突が起きたことを通知する
        currentState?.OnTriggerEnter(collision.gameObject);
    }

    [ServerCallback]
    private void OnCollisionExit(Collision collision)
    {
        if (m_itemType != ItemType.TRAP && m_itemType != ItemType.SETPART) { return; }
        // 現在のステートに衝突が離れたことを通知する
        currentState?.OnCollisionExit(collision.collider);
    }

    [ServerCallback]
    public void ServerAddPoint()
    {
        // ポイントに変換する
        this.GetPlayerData().AddPoint(this.GetPoint());
        this.RpcAddPoint();
        // このオブジェクトを破棄する
        this.DestroysGameObject();
    }

    [ClientRpc]
    private void RpcAddPoint()
    {
        // ポイントに変換する
        this.GetPlayerData().AddPoint(this.GetPoint());
        // このオブジェクトを破棄する
        this.DestroysGameObject();
    }

    [ClientRpc]
    public void RpcExplode()
    {
        this.SetIsKinematic(true);
        this.GetEffectObject().SetActive(true);
        this.GetEffectObject().transform.parent = null;
        this.transform.localScale = Vector3.zero;
        this.GetColiider().enabled = false;
    }

    // このオブジェクトを破棄する
    public void DestroysGameObject(GameObject _gameObject = null)
    {
        if(_gameObject == null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Destroy(_gameObject);
        }
    }

    private void OnDestroy()
    {
        if(m_itemType != ItemType.TRAP) { return; }
        // 現在のステートの終了処理
        currentState?.Exit();
    }

    // このアイテムの種類の取得
    public ItemType GetItemType()
    {
        return m_itemType;
    }

    // このアイテムの現在のステートの取得
    public ItemStateType GetItemStateType()
    {
        return m_stateType;
    }

    public BuffManager_Tanabe.Buff.BuffType GetBuffType()
    {
        return m_buffType;
    }

    public Rigidbody GetRigidbody()
    {
        return m_rb;
    }

    public bool GetIsKinematic()
    {
        return m_rb.isKinematic;
    }

    public Collider GetColiider()
    {
        return m_collider;
    }

    // ポイントの取得
    public float GetPoint()
    {
        return m_point;
    }

    // プレイヤーのトランスフォームの取得
    public Transform GetPlayerTransform()
    {
        return playerTransform;
    }

    // プレイヤーデータの取得
    public Player_Tanabe GetPlayerData()
    {
        return m_playerData;
    }

    public GameObject GetEffectObject()
    {
        return m_effectObject;
    }


    // プレイヤーのトランスフォームの設定
    public void SetPlayerTransform(Transform _transform)
    {
        if(playerTransform != null) { return; }
        playerTransform = _transform;
    }

    // プレイヤーデータの設定
    public void SetPlayerData(Player_Tanabe _playerData)
    {
        m_playerData = _playerData;
        if(_playerData != null)
        {
            playerTransform = m_playerData.transform;
        }
        else
        {
            playerTransform = null;
        }
    }

    // プレイヤーデータの設定
    [ClientRpc]
    public void RpcSetPlayerData(Player_Tanabe _playerData)
    {
        m_playerData = _playerData;
        if(_playerData != null)
        {
            playerTransform = m_playerData.transform;
        }
        else
        {
            playerTransform = null;
        }
    }

    // 重力の有無設定
    public void SetUseGravity(bool flag)
    {
        if(m_rb == null) { return; }
        m_rb.useGravity = flag;
    }

    // キネマティックの設定
    public void SetIsKinematic(bool flag)
    {
        if (m_rb == null) { return; }
        m_rb.isKinematic = flag;
    }

}
