using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageSwitcher : MonoBehaviour
{
    public Image[] images; // 3つのImageUIを格納する配列
    public Sprite newSprite; // 変更後のスプライト
    private int currentIndex = 0; // 現在のインデックス

    void Start()
    {
        // 初期状態では、すべてのImageUIを元のスプライトに設定
        foreach (var image in images)
        {
            image.sprite = image.sprite; // 元のスプライトを設定
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 現在のImageUIを新しいスプライトに切り替え
            if (currentIndex < images.Length)
            {
                images[currentIndex].sprite = newSprite; // 新しいスプライトに変更
                currentIndex++; // インデックスを増加
            }
        }
    }
}
