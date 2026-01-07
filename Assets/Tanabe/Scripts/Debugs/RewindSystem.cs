using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewindSystem : MonoBehaviour
{
    // 保持するパラメータの構造体
    private struct Parameter
    {
        public Vector3 position;            // 位置
        public Quaternion rotation;         // 回転角
        public Vector3 scale;               // スケール
        public Vector3 velocity;            // 速度
        public Vector3 angularVelocity;     // 回転速度
        public float time;                  // 保持した時間
    }

    //ーーーーーーーーーーーーーーーーーーーーーーーーーー
    // 編集パラメータ
    //ーーーーーーーーーーーーーーーーーーーーーーーーーー

    [Header("パラメータの保持処理を行う頻度　通常：0.5"), SerializeField]
    private float RENTENTION_TIME = 0.5f;

    [Header("巻き戻し速度　通常：1.0"), SerializeField]
    private float REWIND_SPEED = 1.0f;

    [Header("過去のパラメータを保持する最大時間の上限　※0以下で上限なし"), SerializeField]
    private float MAX_PASTTIME;

    [Header("移動速度の保持：trueで使用"), SerializeField]
    private bool m_isRententionVelocity = false;

    //ーーーーーーーーーーーーーーーーーーーーーーーーーー
    // 内部パラメータ
    //ーーーーーーーーーーーーーーーーーーーーーーーーーー

    // リジッドボディ
    private Rigidbody m_rb;
    // 過去のパラメータを格納するリスト
    private List<Parameter> m_parametersList = new List<Parameter>();
    // 経過時間
    private float m_time = 0.0f;
    // 巻き戻しを行う頻度のカウンター
    private float m_rententionCount = 0.0f;
    // 巻き戻し中か判断するフラグ
    private bool m_isRewinding = false;
    // 巻き戻しキーを入力しているかのフラグ
    private bool m_isRewindExecute = false;
    // 現在実行しているコルーチン
    private Coroutine m_currentCoroutine;
    // 現在のパラメータ
    Parameter m_currentParameter;
    // 重力の有無フラグ
    bool m_defaultUseGravity = false;

    //ーーーーーーーーーーーーーーーーーーーーーーーーーー
    // 内部実装関数
    //ーーーーーーーーーーーーーーーーーーーーーーーーーー

    // 開始関数
    public virtual void Start()
    {
        Application.targetFrameRate = 60;
        m_rb = GetComponent<Rigidbody>();
        if (m_rb != null)
        {
            m_defaultUseGravity = m_rb.useGravity;
        }
        // 初期パラメータの保持　※ここで格納されるパラメータは削除されない
        ParameterRentention();
    }

    // 更新関数
    public virtual void Update()
    {
        // 巻き戻しが行われていなければ通す
        if (!m_isRewinding && !m_isRewindExecute)
        {
            // パラメータの保持を行うか判断する
            this.ParameterRententionDecision();
        }

        // 巻き戻しが行われていない状態でZキーが押されたら通す
        if (!m_isRewindExecute && !m_isRewinding && Input.GetKeyDown(KeyCode.Z))
        {
            // 巻き戻しキーが押された
            m_isRewindExecute = true;
            // 巻き戻し処理の準備を行う
            RewindPreparation();
            Debug.Log("リワインド開始！");
        }
        // Zキーを離したら通す
        if (m_isRewindExecute && Input.GetKeyUp(KeyCode.Z))
        {
            // 巻き戻しキーが離された
            m_isRewindExecute = false;
        }

        // 通常
        if (!m_isRewinding)
        {
            // 経過時間のカウントアップ
            m_time += Time.deltaTime;
        }
        // 巻き戻し中
        else
        {
            // 経過時間のカウントダウン
            m_time -= Time.deltaTime * REWIND_SPEED;
            // 経過時間が0未満なら巻き戻し処理を停止する
            if (m_time < 0.0f)
            {
                // 経過時間の初期化
                m_time = 0.0f;
                // 巻き戻しキーを離した状態にする
                m_isRewindExecute = false;
            }
            // 巻き戻し処理
            Rewind();
        }

    }


    //ーーーーーーーーーーーーーーーーーーーーーーーーーー
    // 内部関数
    //ーーーーーーーーーーーーーーーーーーーーーーーーーー

    // 現在のパラメータの取得
    private Parameter GetCurrentParameter()
    {
        Parameter parameter;
        parameter.position = this.transform.position;
        parameter.rotation = this.transform.rotation;
        parameter.scale = this.transform.localScale;
        if (m_rb != null)
        {
            parameter.velocity = m_rb.velocity;
            parameter.angularVelocity = m_rb.angularVelocity;
        }
        else
        {
            parameter.velocity = Vector3.zero;
            parameter.angularVelocity = Vector3.zero;
        }
        parameter.time = m_time;

        return parameter;
    }

    // 現在のパラメータの設定
    private void SetCurrentParameter(Parameter parameter)
    {
        this.transform.position = parameter.position;
        this.transform.rotation = parameter.rotation;
        this.transform.localScale = parameter.scale;
        if (m_rb != null)
        {
            m_rb.velocity = parameter.velocity;
            m_rb.angularVelocity = parameter.angularVelocity;
        }
    }


    //ーーーーーーーーーーーーーーーーーーーーーーーーーー
    // 外部関数
    //ーーーーーーーーーーーーーーーーーーーーーーーーーー

    // 巻き戻しキーを入力しているか
    public bool GetIsRewindExecute()
    {
        return m_isRewindExecute;
    }

    // 巻き戻し中か
    public bool GetIsRewinding()
    {
        return m_isRewinding;
    }


    //ーーーーーーーーーーーーーーーーーーーーーーーーーー
    // 仮想関数
    //ーーーーーーーーーーーーーーーーーーーーーーーーーー

    // 現在のパラメータの保持判断
    public virtual void ParameterRententionDecision()
    {
        // 巻き戻し頻度のカウントアップ
        m_rententionCount += Time.deltaTime;

        // 保持する最大時間が過ぎていたらtrueを返す
        bool isMaxRentention = m_time >= MAX_PASTTIME & MAX_PASTTIME > 0.0f;

        // 巻き戻しを行う時間になったら通す
        if (m_rententionCount > RENTENTION_TIME)
        {
            m_rententionCount = 0.0f;
            // 現在のパラメータを保持する
            ParameterRentention();

            // 保持する最大時間の上限を過ぎていたら通す
            if (isMaxRentention)
            {
                Debug.Log("保持する時間の上限が杉田玄白");
                // 経過時間の調整
                m_time = MAX_PASTTIME;
                // 保持しているパラメータのソート
                SortParameter();
            }
        }
    }

    // 現在のパラメータを保持する
    public virtual void ParameterRentention()
    {
        // 保持するパラメータの追加
        m_parametersList.Add(this.GetCurrentParameter());
    }

    // 保持するパラメータのソート
    public virtual void SortParameter()
    {
        // オーバーした時間の算出
        float overTime = m_parametersList[m_parametersList.Count - 1].time - MAX_PASTTIME;
        for (int i = 0; i < m_parametersList.Count; i++)
        {
            Parameter newParameter = m_parametersList[i];
            // オーバーした分の時間を引く
            newParameter.time = Mathf.Max(0.0f, newParameter.time - overTime);
            // パラメータの更新
            m_parametersList[i] = newParameter;
        }
        // 保持したパラメータの中で最も古いデータを削除する
        m_parametersList.RemoveAt(0);
    }

    // 巻き戻し処理準備
    public virtual void RewindPreparation()
    {
        // 巻き戻し処理フラグを実行中にする
        m_isRewinding = true;

        // リジッドボディがあれば通す　※巻き戻し処理中に不自然な動きがでないように速度に関係する値を全てゼロにする
        if (m_rb != null)
        {
            // 速度をゼロにする
            m_rb.velocity = Vector3.zero;
            // 回転速度をゼロにする
            m_rb.angularVelocity = Vector3.zero;
            // 重力を無効にする
            m_rb.useGravity = false;
            // キネマティックを有効にする
            m_rb.isKinematic = true;
            // 衝突判定を無効にする
            m_rb.detectCollisions = false;
        }

        // 保持しているリストから現在の経過時間をオーバーしているデータを全て削除する
        DeleteTimeOverParameter();

        // 現在のパラメータの取得
        m_currentParameter = this.GetCurrentParameter();
    }

    // 巻き戻し処理
    public void Rewind()
    {
        // 最後に保持したパラメータの取得
        Parameter rewindParameter = m_parametersList[m_parametersList.Count - 1];
        float currentTime = rewindParameter.time;

        // 巻き戻し処理を行う
        if (m_time > currentTime)
        {
            // 補間の経過時間の算出
            float lerpTime = (m_time - currentTime) / RENTENTION_TIME;
            // 誤差の補間
            if (lerpTime < 0.0f)
            {
                lerpTime = 0.0f;
            }

            this.transform.position = Vector3.Lerp(rewindParameter.position, m_currentParameter.position, lerpTime);
            this.transform.rotation = Quaternion.Lerp(rewindParameter.rotation, m_currentParameter.rotation, lerpTime);
            this.transform.localScale = Vector3.Lerp(rewindParameter.scale, m_currentParameter.scale, lerpTime);

            // 子クラスの巻き戻し処理を実行する
            Rewinding(lerpTime);

            return;
        }

        // 巻き戻しキーを押しているかつ、保持しているパラメータが２つ以上あれば通す
        if (m_isRewindExecute && m_parametersList.Count > 1)
        {
            // 保持しているリストから現在の経過時間をオーバーしているデータを全て削除する
            DeleteTimeOverParameter();
            // 現在のパラメータの取得
            m_currentParameter = this.GetCurrentParameter();
        }
        else
        {
            // 巻き戻しフラグをfalseにする
            m_isRewinding = false;
            m_isRewindExecute = false;
            // 巻き戻しデータのカウントをリセットする
            m_rententionCount = 0.0f;

            // リジッドボディがあれば通す
            if (m_rb != null)
            {
                // 重力の状態を元の状態に戻す
                m_rb.useGravity = m_defaultUseGravity;
                // キネマティックを無効にする
                m_rb.isKinematic = false;
                // 衝突判定を有効にする
                m_rb.detectCollisions = true;
                // 速度の設定
                if(m_isRententionVelocity)
                {
                    m_rb.velocity = rewindParameter.velocity;
                }
                else
                {
                    m_rb.velocity = Vector3.zero;
                }
                // 回転速度の設定
                m_rb.angularVelocity = rewindParameter.angularVelocity;
            }
            Debug.Log("リワインド終了！");
        }
    }

    // 子クラスの巻き戻し処理 ※このスクリプトを継承したスクリプト用
    public virtual void Rewinding(float lerpTime)
    {
        // do nothing
    }

    // 保持しているリストから現在の経過時間をオーバーしているデータを全て削除する
    public virtual void DeleteTimeOverParameter()
    {
        for (int i = m_parametersList.Count - 1; i >= 1; i--)
        {
            // 保持している時間が現在の経過時間より小さければこれ以上処理しない
            if (m_time > m_parametersList[i].time) { break; }
            // 指定された場所のデータを削除する
            DeleteParameterAt(i);
        }
    }

    // 指定された場所のデータを削除する
    public virtual void DeleteParameterAt(int deleteNumber)
    {
        m_parametersList.RemoveAt(deleteNumber);
    }
}
