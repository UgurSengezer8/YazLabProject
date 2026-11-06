using UnityEngine;
using UnityEngine.InputSystem;

public class LLMMenuManager : MonoBehaviour
{
    [Header("Menü Paneli")]
    public GameObject menuPanel; 
    private bool isMenuOpen = true;
    private PlayerInputs playerInputs;
    
    private void Awake()
    {
        playerInputs = new PlayerInputs();
        playerInputs.Player.Enable();
        playerInputs.Player.Tab.performed += OpenMenu;

    }

    public void OpenMenu(InputAction.CallbackContext context)
    {
        if (isMenuOpen ==false)
        {
            isMenuOpen = true;

            if (menuPanel != null)
                menuPanel.SetActive(true);

            Time.timeScale = 0f;
        }
        else
        {
            CloseMenu() ;
        }



    }

    public void CloseMenu()
    {
        isMenuOpen = false;

        if (menuPanel != null)
            menuPanel.SetActive(false);

        Time.timeScale = 1f;

        
    }
}
