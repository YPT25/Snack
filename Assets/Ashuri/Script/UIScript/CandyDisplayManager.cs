using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ItemStateMachine;

public class CandyDisplayManager : MonoBehaviour
{
    [Header("お菓子のUI")]
    [Tooltip("ポップコーンのイラスト")]
    [SerializeField] private Image _popcornSprite;

    [Tooltip("ふわふわのイラスト")]
    [SerializeField] private Image _fluffySprite;

    private ItemStateMachine _itemStateMachine;
    // Start is called before the first frame update
    void Start()
    {
        // プレイヤーが現れるまで待つ処理を開始
        StartCoroutine(WaitForPlayer());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // プレイヤーが生成されるまで探し続ける処理
    private IEnumerator WaitForPlayer()
    {
        // プレイヤーが見つかるまでループ
        while (_itemStateMachine == null)
        {
            _itemStateMachine = FindObjectOfType<ItemStateMachine>();
            yield return null;  // 1フレーム待つ
        }

        // プレイヤーが見つかったので武器UIを更新
        UpdateWeaponUI(_itemStateMachine.GetItemStateType());
    }

    // 武器UIを更新する処理
    private void UpdateWeaponUI(ItemStateMachine.ItemStateType id)
    {
        // UIを一度全部消す処理
        _popcornSprite.gameObject.SetActive(false);
        _fluffySprite.gameObject.SetActive(false);

        // ハンマー表示処理
        if (id == ItemStateMachine.ItemStateType.THROW)
        {
            _popcornSprite.gameObject.SetActive(true);
        }

        // 銃の表示処理
        if (id == ItemStateMachine.ItemStateType.TRAP)
        {
            _fluffySprite.gameObject.SetActive(true);
        }
    }
}
