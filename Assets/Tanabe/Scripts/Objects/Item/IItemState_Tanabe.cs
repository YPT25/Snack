using UnityEngine;

public interface IItemState_Tanabe
{
    void Enter();
    void Update();
    void OnTriggerEnter(GameObject other);
    void OnTriggerExit(Collider other);
    void OnCollisionExit(Collider other);
    void Exit();
}