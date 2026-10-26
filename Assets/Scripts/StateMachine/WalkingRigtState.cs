using UnityEngine;

public class WalkingRigtState : IState
{
    public void EnterState(PlayerMovement player)
    {
        Debug.Log("Entering Walking Right State");
        PlayerAnimationControl.Instance.ChangeAnimation("WalkingRight");

    }

    public void ExitState(PlayerMovement player)
    {
        Debug.Log("Exiting Walking Right State");
    }

    public void UpdateState(PlayerMovement player)
    {
        switch (PlayerMovement.Instance.currentLookDirection)
        {
            case PlayerMovement.LookDirection.Up:
                PlayerMovement.Instance.ChangeState(new WalkingUpState());
                break;
            case PlayerMovement.LookDirection.Down:
                PlayerMovement.Instance.ChangeState(new WalkingDownState());
                break;
            case PlayerMovement.LookDirection.Left:
                PlayerMovement.Instance.ChangeState(new WalkingLeftState());
                break;
            case PlayerMovement.LookDirection.None:
                PlayerMovement.Instance.ChangeState(new IdleState());
                break;
            default:
                break;
        }
    }
}
