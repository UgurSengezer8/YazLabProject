using UnityEngine;

public class PushingBoxUpState : IState
{
    public void EnterState(PlayerMovement player)
    {
        Debug.Log("Entering Pushing Box Up State");
        PlayerAnimationControl.Instance.ChangeAnimation("PlayerPushUp");
    }

    public void ExitState(PlayerMovement player)
    {
        Debug.Log("Exiting Pushing Box Up State");
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
