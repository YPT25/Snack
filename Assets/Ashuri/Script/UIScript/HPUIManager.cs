using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

/// <summary>
/// ローカルプレイヤーの HP を監視して UI を更新するクラス（デバッグ付き・Mirror対応）
/// Canvas にアタッチして使う想定です。実行してコンソール出力を確認してください。
/// </summary>
public class HPUIManager : MonoBehaviour
{
    [Header("HPバー")]
    [Tooltip("HPバー本体（Image） - Image Type を 'Filled' にしておくこと")]
    [SerializeField] private Image _hpFill;

    // ローカルプレイヤー参照（CharacterBase）
    private CharacterBase _localPlayer;

    // 最大HP（クライアントで使用する）
    private float _maxHP = 1f;

    // 補間用現在HP
    private float _currentHP;

    // ターゲットHP
    private float _targetHP;

    // 補間速度
    private float _lerpSpeed = 7f;

    // 直前にログ出力したターゲットHP（変化時にログを出す）
    private float _lastLoggedTargetHP = -9999f;

    // ---------------------------------------------------
    // 最初に呼ばれる処理：基本チェックとローカルプレイヤー探索を開始
    private void Start()
    {
        // 重要：_hpFill にセットされているかチェック
        if (_hpFill == null)
        {
            Debug.LogError("[HPUIManager] _hpFill が Inspector にセットされていません。Canvas の HPFill を割り当ててください。");
        }
        else
        {
            // Image Type が Filled かどうか確認（今回の実装は FillAmount を使うため）
            if (_hpFill.type != Image.Type.Filled)
            {
                Debug.LogWarning("[HPUIManager] _hpFill Image.type が Filled ではありません。Inspector で Image Type を 'Filled' にしてください。");
            }
        }

        // Mirror を使う場合は NetworkClient 側から localPlayer を取得するのが確実
        StartCoroutine(FindLocalPlayerRoutine());
    }

    // ---------------------------------------------------
    // ローカルプレイヤーを探す処理（NetworkClient を優先して使用し、見つからなければフォールバック）
    private IEnumerator FindLocalPlayerRoutine()
    {
        // まずは NetworkClient の接続済み identity を待つ（推奨ケース）
        float timeout = 5f;
        float t = 0f;
        while (NetworkClient.connection == null || NetworkClient.connection.identity == null)
        {
            // すぐに見つかる場合がほとんどだが、接続待ちやシーンロード中だと時間がかかる
            t += Time.deltaTime;
            if (t > timeout) break;
            yield return null;
        }

        // NetworkClient から取得できたらそれを使う
        if (NetworkClient.connection != null && NetworkClient.connection.identity != null)
        {
            var go = NetworkClient.connection.identity.gameObject;
            _localPlayer = go.GetComponent<CharacterBase>();
            if (_localPlayer != null)
            {
                //Debug.Log("[HPUIManager] NetworkClient.connection.identity からローカルプレイヤーを検出しました: " + go.name);
            }
        }

        // もし取得できていなければフォールバックで FindObjectsOfType を使う（レガシー）
        if (_localPlayer == null)
        {
            Debug.LogWarning("[HPUIManager] NetworkClient から取得できませんでした。FindObjectsOfType でフォールバックします。");
            while (_localPlayer == null)
            {
                var players = FindObjectsOfType<CharacterBase>();
                foreach (var p in players)
                {
                    var nid = p.GetComponent<NetworkIdentity>();
                    if (nid != null && nid.isLocalPlayer)
                    {
                        _localPlayer = p;
                        break;
                    }
                }
                if (_localPlayer == null) yield return null;
            }
            //Debug.Log("[HPUIManager] FindObjectsOfType でローカルプレイヤーを検出しました: " + _localPlayer.name);
        }

        // ローカルプレイヤーが得られたら初期値をセット
        if (_localPlayer != null)
        {
            _maxHP = Mathf.Max(1f, _localPlayer.GetMaxHP()); // 0 防止
            _currentHP = _localPlayer.GetHp();
            _targetHP = _currentHP;
            _lastLoggedTargetHP = _targetHP - 1f; // 強制ログ
            //Debug.Log($"[HPUIManager] 初期HPセット max:{_maxHP} cur:{_currentHP}");
        }
        else
        {
            Debug.LogError("[HPUIManager] ローカルプレイヤーが見つかりません。CharacterBase が NetworkIdentity を持っているか確認してください。");
        }

        // UI 更新ループを開始
        StartCoroutine(UpdateUIRoutine());
    }

    // ---------------------------------------------------
    // 毎フレーム HP を監視して UI を更新するコルーチン
    private IEnumerator UpdateUIRoutine()
    {
        while (true)
        {
            // ローカルプレイヤーがある場合、ターゲットHP を取得
            if (_localPlayer != null)
            {
                float newHp = _localPlayer.GetHp();

                // もしサーバー側でしか HP を操作していてクライアントに反映されていない場合、
                // newHp が常に同じになるはず → その場合はサーバー同期周りを疑う
                if (!Mathf.Approximately(newHp, _targetHP))
                {
                    // ターゲットHP が変わったらログを出す（変化があれば）
                    Debug.Log($"[HPUIManager] ターゲットHPが変化しました old:{_targetHP} -> new:{newHp}");
                    _targetHP = newHp;
                }
            }

            // 補間して現在HPを更新
            _currentHP = Mathf.Lerp(_currentHP, _targetHP, Time.deltaTime * _lerpSpeed);

            // UI へ反映
            UpdateHPBar();

            // 1 フレーム待機
            yield return null;
        }
    }

    // ---------------------------------------------------
    // 実際に UI を更新する処理（FillAmount 方式）
    private void UpdateHPBar()
    {
        // 安全チェック：_hpFill と _maxHP
        if (_hpFill == null)
        {
            return;
        }

        if (_maxHP <= 0f)
        {
            Debug.LogWarning("[HPUIManager] _maxHP が不正です。0 以下になっています。");
            _maxHP = 1f;
        }

        // 正規化
        float normalized = Mathf.Clamp01(_currentHP / _maxHP);

        // FillAmount 更新
        _hpFill.fillAmount = normalized;

        // デバッグ：ターゲットHP が変わったときだけログ（spam を避ける）
        if (!Mathf.Approximately(_lastLoggedTargetHP, _targetHP))
        {
            Debug.Log($"[HPUIManager] UI反映: normalized={normalized:F3} current:{_currentHP:F2} target:{_targetHP:F2} max:{_maxHP:F2}");
            _lastLoggedTargetHP = _targetHP;
        }
    }

    /// <summary>
    /// ローカルプレイヤーの取得
    /// </summary>
    /// <param name="_localPlayer"></param>
    public void SetLocalPlayer(Player_Tanabe _localPlayerTanabe)
    {
        _localPlayer = _localPlayerTanabe;
    }
}
