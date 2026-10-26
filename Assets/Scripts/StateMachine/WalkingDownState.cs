using UnityEngine;

public class WalkingDownState : IState
{
    public void EnterState(PlayerMovement player)
    {
        Debug.Log("Entering Walking Down State");
        PlayerAnimationControl.Instance.ChangeAnimation("WalkingDown");
    }

    public void ExitState(PlayerMovement player)
    {
        Debug.Log("Exiting Walking Right State");
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
            case PlayerMovement.LookDirection.Left:
                PlayerMovement.Instance.ChangeState(new WalkingLeftState());
                break;
            case PlayerMovement.LookDirection.Right:
                PlayerMovement.Instance.ChangeState(new WalkingRigtState());
                break;
            default:
                break;
        }
    }
}
