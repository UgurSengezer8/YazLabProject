using UnityEngine;

public class PushingBoxLeftState : IState
{
    public void EnterState(PlayerMovement player)
    {
        Debug.Log("Entering Pushing Box Left State");
        PlayerAnimationControl.Instance.ChangeAnimation("WalkingDown");
    }

    public void ExitState(PlayerMovement player)
    {
        Debug.Log("Exiting Pushing Box Left State");
    }

    public void UpdateState(PlayerMovement player)
    {

        if (player.isWalking)
        {
            if (player.isTouchingBox)
            {
                switch (PlayerMovement.Instance.currentLookDirection)
                {

                    case PlayerMovement.LookDirection.Down:
                        PlayerMovement.Instance.ChangeState(new PushingBoxDownState());
                        break;
                    case PlayerMovement.LookDirection.Right:
                        PlayerMovement.Instance.ChangeState(new PushingBoxRightState());
                        break;
                    case PlayerMovement.LookDirection.Up:
                        PlayerMovement.Instance.ChangeState(new PushingBoxUpState());
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (PlayerMovement.Instance.currentLookDirection)
                {
                    case PlayerMovement.LookDirection.Down:
                        PlayerMovement.Instance.ChangeState(new WalkingDownState());
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
        else
        {
            PlayerMovement.Instance.ChangeState(new IdleState());
        }
    }
}
