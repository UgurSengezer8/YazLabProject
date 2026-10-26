using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    private PlayerInputs playerInputs;
    private Vector2 moveInput;
    private Vector2 oldDirection;
    private bool isWalking;
    private void Awake()
    {
        playerInputs = new PlayerInputs();
        playerInputs.Player.Enable();
    }
    void Update()
    {

        moveInput = playerInputs.Player.Move.ReadValue<Vector2>();
        if (oldDirection!=moveInput)
        {
            if (moveInput.magnitude>=0.01f)
            {
                isWalking = true;
            }
            else
            {
                isWalking= false;
            }
            EventBus<PlayerMoveEvent>.Publish(new PlayerMoveEvent
            {
                direction = moveInput,
                isWalking = this.isWalking

            });
            Debug.Log("yön değişti");
        }
        oldDirection = moveInput;
        
    }
}
