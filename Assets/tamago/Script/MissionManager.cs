using System.Collections;
using UnityEngine;
using TMPro;

public class MissionManager : MonoBehaviour
{
    public TMP_Text missionText; // ミッションの表示用Text
    public string[] missions; // ミッションの配列
    public float missionDuration = 60f; // ミッションの時間（1分）
    private float timeRemaining;
    private bool missionActive = false; // ミッションがアクティブかどうか
    private int currentMissionIndex = 0; // 現在のミッションインデックス
    private bool spacePressed = false; // スペースキーが押されたかどうか

    void Start()
    {
        StartNextMission();
    }

    void Update()
    {
        // スペースキーが押された場合にフラグを立てる
        if (Input.GetKeyDown(KeyCode.Space) && missionActive)
        {
            spacePressed = true; // スペースキーが押された
        }
    }

    void StartNextMission()
    {
        if (currentMissionIndex < missions.Length)
        {
            missionText.text = missions[currentMissionIndex];
            timeRemaining = missionDuration;
            missionActive = true; // ミッションをアクティブにする
            spacePressed = false; // スペースキーの押下フラグをリセット
            StartCoroutine(MissionCycle()); // コルーチンを開始
        }
        else
        {
            missionText.text = "All missions completed!";
        }
    }

    IEnumerator MissionCycle()
    {
        while (timeRemaining > 0)
        {
            yield return new WaitForSeconds(1);
            timeRemaining--;
        }

        // 1分経過後にスペースが押されたか確認
        if (spacePressed)
        {
            // スペースが押されていた場合、次のミッションに進む
            currentMissionIndex++;
            StartNextMission(); // 次のミッションを開始
        }
        else
        {
            // スペースが押されていなかった場合、ゲームオーバー
            GameOver();
        }
    }

    void GameOver()
    {
        missionText.text = "Game Over!";
        // ゲームオーバー処理をここに追加
        Debug.Log("Game Over!"); // デバッグメッセージ
    }
}