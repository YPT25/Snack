using UnityEngine;

public interface IPlayerState_Tanabe
{
    // ステートの変更に使用する関数
    void Enter();
    void Update();
    void FixedUpdate();
    void Exit();
}
