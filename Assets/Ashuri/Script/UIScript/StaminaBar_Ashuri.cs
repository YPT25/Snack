using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBar_Ashuri : MonoBehaviour
{
    [Header("スタミナバー")]
    [Tooltip("スタミナバー本体（Image） - Image Type を 'Filled' にしておくこと")]
    [SerializeField] private Image _staminaFill;

    // ローカルプレイヤー参照（CharacterBase）
    private CharacterBase _localPlayer;

    // 最大スタミナ（クライアントで使用する）
    private float _maxStamina = 1f;

    // 補間用現在スタミナ
    private float _currentStamina;

    // ターゲットスタミナ
    private float _targetStamina;

    // 補間速度
    private float _lerpSpeed = 7f;

    // 直前にログ出力したターゲットStamina（変化時にログを出す）
    private float _lastLoggedTargetStamina = -9999f;

    // Start is called before the first frame update
    void Start()
    {
        // 重要：_StaminaFill にセットされているかチェック
        if (_staminaFill == null)
        {
            Debug.LogError("[StaminaUIManager] _staminaFill が Inspector にセットされていません。Canvas の StaminaFill を割り当ててください。");
        }
        else
        {
            // Image Type が Filled かどうか確認（今回の実装は FillAmount を使うため）
            if (_staminaFill.type != Image.Type.Filled)
            {
                Debug.LogWarning("[StaminaUIManager] _staminaFill Image.type が Filled ではありません。Inspector で Image Type を 'Filled' にしてください。");
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
                Debug.Log("[StaminaUIManager] NetworkClient.connection.identity からローカルプレイヤーを検出しました: " + go.name);
            }
        }

        // もし取得できていなければフォールバックで FindObjectsOfType を使う（レガシー）
        if (_localPlayer == null)
        {
            Debug.LogWarning("[StaminaUIManager] NetworkClient から取得できませんでした。FindObjectsOfType でフォールバックします。");
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
            Debug.Log("[StaminaUIManager] FindObjectsOfType でローカルプレイヤーを検出しました: " + _localPlayer.name);
        }

        // ローカルプレイヤーが得られたら初期値をセット
        if (_localPlayer != null)
        {
            _maxStamina = Mathf.Max(1f, _localPlayer.GetInitialParameter().stamina); // 0 防止
            _currentStamina = _localPlayer.GetStamina();
            _targetStamina = _currentStamina;
            _lastLoggedTargetStamina = _targetStamina - 1f; // 強制ログ
            Debug.Log($"[StaminaUIManager] 初期Staminaセット max:{_maxStamina} cur:{_currentStamina}");
        }
        else
        {
            Debug.LogError("[StaminaUIManager] ローカルプレイヤーが見つかりません。CharacterBase が NetworkIdentity を持っているか確認してください。");
        }

        // UI 更新ループを開始
        StartCoroutine(UpdateUIRoutine());
    }

    // ---------------------------------------------------
    // 毎フレーム Stamina を監視して UI を更新するコルーチン
    private IEnumerator UpdateUIRoutine()
    {
        while (true)
        {
            // ローカルプレイヤーがある場合、ターゲットStamina を取得
            if (_localPlayer != null)
            {
                float newStamina = _localPlayer.GetStamina();

                // もしサーバー側でしか Stamina を操作していてクライアントに反映されていない場合、
                // newStamina が常に同じになるはず → その場合はサーバー同期周りを疑う
                if (!Mathf.Approximately(newStamina, _targetStamina))
                {
                    // ターゲットStamina が変わったらログを出す（変化があれば）
                    Debug.Log($"[StaminaUIManager] ターゲットStaminaが変化しました old:{_targetStamina} -> new:{newStamina}");
                    _targetStamina = newStamina;
                }
            }

            // 補間して現在Staminaを更新
            _currentStamina = Mathf.Lerp(_currentStamina, _targetStamina, Time.deltaTime * _lerpSpeed);

            // UI へ反映
            UpdateStaminaBar();

            // 1 フレーム待機
            yield return null;
        }
    }

    // ---------------------------------------------------
    // 実際に UI を更新する処理（FillAmount 方式）
    private void UpdateStaminaBar()
    {
        // 安全チェック：_StaminaFill と _maxStamina
        if (_staminaFill == null)
        {
            return;
        }

        if (_maxStamina <= 0f)
        {
            Debug.LogWarning("[StaminaUIManager] _maxStamina が不正です。0 以下になっています。");
            _maxStamina = 1f;
        }

        // 正規化
        float normalized = Mathf.Clamp01(_currentStamina / _maxStamina);

        // FillAmount 更新
        _staminaFill.fillAmount = normalized;

        // デバッグ：ターゲットStamina が変わったときだけログ（spam を避ける）
        if (!Mathf.Approximately(_lastLoggedTargetStamina, _targetStamina))
        {
            Debug.Log($"[StaminaUIManager] UI反映: normalized={normalized:F3} current:{_currentStamina:F2} target:{_targetStamina:F2} max:{_maxStamina:F2}");
            _lastLoggedTargetStamina = _targetStamina;
        }
    }
}
