using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private EventBinding<UIEvent> UIEventBinding;
    
    [SerializeField] public TextMeshProUGUI sayacText;
    [SerializeField] public TextMeshProUGUI responseText;
    [SerializeField] public TextMeshProUGUI dusunText;

    [SerializeField] public TMP_InputField inputFieldText;
    private EventBinding<GetResponseEvent> responseBinding;
    public GameObject TextPrefab;
    public GameObject UISatir;

    private bool dusun = false;

    public GameObject dusunuyor;


    private void Start()
    {
        UISatir = GameObject.FindGameObjectWithTag("UISatir");
    }
    private void OnEnable()
    {
        EventBus<UIEvent>.Subscribe(UIEventBinding);
        responseBinding = new EventBinding<GetResponseEvent>(UpdateResponseText);
        EventBus<GetResponseEvent>.Subscribe(responseBinding);
    }
    public void OnClickSendRequestButton()
    {
        Debug.Log(inputFieldText.text);
        EventBus<SendRequestEvent>.Publish(new SendRequestEvent()
        {
            content = inputFieldText.text
        });
        dusun = true;
        dusunText.text = "Düşünüyor...";
        dusunuyor.SetActive(true);
    }
    public void UpdateResponseText(GetResponseEvent responseTex)
    {
        
        if (UISatir != null)
        {
            // 3. Yeni nesneyi oluştur ve ebeveynini (parent) belirle
            // Instantiate parametreleri: (Oluşturulacak Şey, Pozisyon, Dönüş, Ebeveyn Transformu)
            GameObject newChild = Instantiate(TextPrefab, UISatir.transform.position, Quaternion.identity, UISatir.transform);

            newChild.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = responseTex.content.ToString();
            // Opsiyonel: Oluşturulan yeni nesnenin ismini düzenle (sonundaki "(Clone)" yazısını kaldırmak için)
            newChild.name = TextPrefab.name;
            dusun = false;
            dusunuyor.SetActive(false);

            Debug.Log($"{newChild.name}, başarıyla {UISatir.name} nesnesinin içine eklendi.");
        }
        else
        {
            Debug.LogWarning($"Sahnede '' adında bir nesne bulunamadı!");
        }
    }
    private void OnDisable()
    {
        EventBus<UIEvent>.UnSubscribe(UIEventBinding);
        EventBus<GetResponseEvent>.UnSubscribe(responseBinding);
    }
    

}
