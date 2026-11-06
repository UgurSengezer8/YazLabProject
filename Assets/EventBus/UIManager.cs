using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private EventBinding<UIEvent> UIEventBinding;
    
    [SerializeField] public TextMeshProUGUI sayacText;
    [SerializeField] public TextMeshProUGUI responseText;
    [SerializeField] public TMP_InputField inputFieldText;
    private EventBinding<GetResponseEvent> responseBinding;



    private void OnEnable()
    {
        EventBus<UIEvent>.Subscribe(UIEventBinding);
        responseBinding = new EventBinding<GetResponseEvent>(UpdateResponseText);
        EventBus<GetResponseEvent>.Subscribe(responseBinding);
    }
    public void OnClickSendRequestButton()
    {
        EventBus<SendRequestEvent>.Publish(new SendRequestEvent()
        {
            content = inputFieldText.text
        });
    }
    public void UpdateResponseText(GetResponseEvent responseTex)
    {
        responseText.text = responseTex.content;
    }
    private void OnDisable()
    {
        EventBus<UIEvent>.UnSubscribe(UIEventBinding);
        EventBus<GetResponseEvent>.UnSubscribe(responseBinding);
    }
    

}
