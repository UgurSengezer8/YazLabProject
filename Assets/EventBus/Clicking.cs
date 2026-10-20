using UnityEngine;
using UnityEngine.InputSystem;

public class Clicking : MonoBehaviour
{
    int sayac = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            sayac++;
            
            EventBus<UIEvent>.Publish(new UIEvent());
            Debug.Log("Yeni Input System: Fareye tıklandı!");
        }

    }
}
