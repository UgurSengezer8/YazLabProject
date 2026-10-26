using UnityEngine;

public class WalkingLeftState : IState
{
    public void EnterState(PlayerMovement player)
    {
        Debug.Log("Entering Walking left State");
        PlayerAnimationControl.Instance.ChangeAnimation("WalkingLeft");

    }

    public void ExitState(PlayerMovement player)
    {
        Debug.Log("Exiting Walking left State");

    }

    public void UpdateState(PlayerMovement player)
    {

        switch (PlayerMovement.Instance.currentLookDirection)
        {
            case PlayerMovement.LookDirection.None:
                PlayerMovement.Instance.ChangeState(new IdleState());
                break;
            case PlayerMovement.LookDirection.Up:
                PlayerMovement.Instance.ChangeState(new WalkingUpState());
                break;
            case PlayerMovement.LookDirection.Down:
                PlayerMovement.Instance.ChangeState(new WalkingDownState());
                break;
            case PlayerMovement.LookDirection.Right:
                PlayerMovement.Instance.ChangeState(new WalkingRigtState());
                break;
            default:
                break;
        }
    }
}
