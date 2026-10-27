using UnityEngine;

public class IdleState : IState
{
    public void EnterState(PlayerMovement player)
    {
        PlayerAnimationControl.Instance.ChangeAnimation("Idle");

    }

    public void ExitState(PlayerMovement player)
    {
        Debug.Log("Exiting Idle State");

    }

    public void UpdateState(PlayerMovement player)
    {
        if (player.isWalking)
        {
            if (player.isTouchingBox)
            {
                switch (PlayerMovement.Instance.currentLookDirection)
                {
                    case PlayerMovement.LookDirection.Up:
                        PlayerMovement.Instance.ChangeState(new PushingBoxUpState());
                        break;
                    case PlayerMovement.LookDirection.Right:
                        PlayerMovement.Instance.ChangeState(new PushingBoxRightState());
                        break;
                    case PlayerMovement.LookDirection.Down:
                        PlayerMovement.Instance.ChangeState(new PushingBoxDownState());
                        break;
                    case PlayerMovement.LookDirection.Left:
                        PlayerMovement.Instance.ChangeState(new PushingBoxLeftState());
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (PlayerMovement.Instance.currentLookDirection)
                {
                    case PlayerMovement.LookDirection.Left:
                        PlayerMovement.Instance.ChangeState(new WalkingLeftState());
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
        

    }
}
