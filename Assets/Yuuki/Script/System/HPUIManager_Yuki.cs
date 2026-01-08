using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

/// <summary>
/// ローカルプレイヤーの HP を監視して UI を更新するクラス（デバッグ付き・Mirror対応）
/// Canvas にアタッチして使う想定です。実行してコンソール出力を確認してください。
/// </summary>
public class HPUIManager_Yuuki : MonoBehaviour
{
    [Header("HPバー")]
    [Tooltip("HPバー本体（Image） - Image Type を 'Filled' にしておくこと")]
    [SerializeField] private Image _hpFill;

    private CharacterBase _localPlayer;

    private float _maxHP = 1f;
    private float _currentHP;
    private float _targetHP;

    [SerializeField] private float _lerpSpeed = 7f;

    // いま掴んでる localPlayer の identity を覚えておく
    private NetworkIdentity _cachedIdentity;

    private void Start()
    {
        if (_hpFill == null)
            Debug.LogError("[HPUIManager] _hpFill が Inspector にセットされていません。");

        StartCoroutine(UpdateUIRoutine());
    }

    private IEnumerator UpdateUIRoutine()
    {
        while (true)
        {
            // 1) まず「今のローカルプレイヤーidentity」を毎回チェック
            NetworkIdentity currentIdentity = null;
            if (NetworkClient.connection != null)
                currentIdentity = NetworkClient.connection.identity;

            // 2) identity が変わった（=リスポーン等で差し替わった）なら掴み直す
            if (currentIdentity != null && currentIdentity != _cachedIdentity)
            {
                _cachedIdentity = currentIdentity;
                _localPlayer = currentIdentity.GetComponent<CharacterBase>();

                if (_localPlayer != null)
                {
                    _maxHP = Mathf.Max(1f, _localPlayer.GetMaxHP()); // 0除算防止
                    _currentHP = _localPlayer.GetHp();
                    _targetHP = _currentHP;

                    Debug.Log($"[HPUIManager] LocalPlayer rebind -> {_localPlayer.name} max={_maxHP} cur={_currentHP}");
                }
                else
                {
                    Debug.LogError("[HPUIManager] identity から CharacterBase を取得できませんでした。");
                }
            }

            // 3) 掴めてる間はHP追従
            if (_localPlayer != null)
            {
                _targetHP = _localPlayer.GetHp();

                // キャラ差し替えで MaxHP が変わる可能性があるなら毎回更新してもOK
                // （重くないので安全寄りにするなら更新推奨）
                _maxHP = Mathf.Max(1f, _localPlayer.GetMaxHP());
            }

            // 4) 補間
            _currentHP = Mathf.Lerp(_currentHP, _targetHP, Time.deltaTime * _lerpSpeed);

            // 5) UI反映
            UpdateHPBar();

            yield return null;
        }
    }

    private void UpdateHPBar()
    {
        if (_hpFill == null) return;

        float normalized = (_maxHP <= 0f) ? 0f : Mathf.Clamp01(_currentHP / _maxHP);
        _hpFill.fillAmount = normalized;
    }
}