using UnityEngine;

public interface IItemState_Tanabe
{
    void Enter();
    void Update();
    void OnTriggerEnter(Collider other);
    void OnTriggerExit(Collider other);
    void OnCollisionExit(Collider other);
    void Exit();
}