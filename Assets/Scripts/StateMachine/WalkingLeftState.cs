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
        if (player.isWalking)
        {
            if (player.isTouchingBox)
            {
                switch (PlayerMovement.Instance.currentLookDirection)
                {
                    case PlayerMovement.LookDirection.Left:
                        PlayerMovement.Instance.ChangeState(new PushingBoxDownState());
                        break;
                    case PlayerMovement.LookDirection.Up:
                        PlayerMovement.Instance.ChangeState(new PushingBoxUpState());
                        break;
                    case PlayerMovement.LookDirection.Down:
                        PlayerMovement.Instance.ChangeState(new PushingBoxDownState());
                        break;
                    case PlayerMovement.LookDirection.Right:
                        PlayerMovement.Instance.ChangeState(new PushingBoxRightState());
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (PlayerMovement.Instance.currentLookDirection)
                {
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
        else
        {

            PlayerMovement.Instance.ChangeState(new IdleState());
        }
    }
}
