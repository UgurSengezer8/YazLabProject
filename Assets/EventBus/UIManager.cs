using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private EventBinding<UIEvent> UIEventBinding;
    private int sayac = 0;
    [SerializeField] public TextMeshProUGUI sayacText;
    
    private void OnEnable()
    {
        UIEventBinding = new EventBinding<UIEvent>(OnClickUIChange);
        EventBus<UIEvent>.Subscribe(UIEventBinding);
    }
    private void OnDisable()
    {
        EventBus<UIEvent>.UnSubscribe(UIEventBinding);

    }
    public void OnClickUIChange() 
    {
        sayac++;
        sayacText.text = sayac.ToString();
        Debug.Log("tıkk");
    }

}
