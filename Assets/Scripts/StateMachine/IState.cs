using UnityEngine;

public interface IState
{
    void EnterState(PlayerMovement player);
    void ExitState(PlayerMovement player);
    void UpdateState(PlayerMovement player);

}
