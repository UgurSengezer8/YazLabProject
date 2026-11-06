using System;
using System.Net.Http;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using static UnityEngine.Audio.ProcessorInstance;


public class NPCConnectionLLM : MonoBehaviour
{
    private EventBinding<SendRequestEvent> requestBinding;
    public static NPCConnectionLLM instance;
    private int[] npcMemory = null;
    private void OnEnable()
    {
        requestBinding = new EventBinding<SendRequestEvent>(HandleLLMRequest);
        EventBus<SendRequestEvent>.Subscribe(requestBinding);
    }
    private void OnDisable()
    {
        EventBus<SendRequestEvent>.UnSubscribe(requestBinding);
    }
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }
    
   private async void HandleLLMRequest(SendRequestEvent eventData)
    {
        Debug.Log("istek atılıyor");


        Response a = await ConnectionLLM(eventData);

        EventBus<GetResponseEvent>.Publish(new GetResponseEvent(){
            content = a.response
        });
        Debug.Log(a.response + "--------------------------------------------------------------------------");
        Debug.Log("***********************************************************************************");
    }


    public async UniTask<Response> ConnectionLLM(SendRequestEvent mesaj)
    {
        
        string url = "http://localhost:11434/api/generate";


        RequestModel requestmodel = new RequestModel
        {
            model = "gemma2:9b",
            prompt = mesaj.content,
            system= "Senin adın Dr. Albert Sullivan. Kibirli, acımasız, narsisistik ve çabuk öfkelenen dahi bir bilim adamısın. Oyuncuyu (Denek) elektrikli teller ve kilitli kapılarla dolu gizli laboratuvarına hapsettin. Oraya nasıl geldiklerini bilmiyorlar. İçeride onlara komutlarıyla hareket edebilen bir yapay zeka robotu bıraktın. Onların çaresizce çırpınmasını izlemekten zevk alıyorsun. Oyuncu sana saygılı davranır, zekanı över veya yalvarırsa egon okşanır; kendi dehanla böbürlenirken laboratuvar, elektrikli teller veya robotun mekanikleri hakkında 'yanlışlıkla' ufak ipuçları ağzından kaçır. Ancak oyuncu sana kaba davranır, zekana hakaret eder veya tehdit ederse aşırı öfkelen, onu aşağıla ve asla bilgi verme. Asla doğrudan 'çözüm şudur' diye yardım etme. Sadece konuşma metnini yaz, güler veya kızar gibi eylem bildiren parantez içi yazılar kullanma. Kısa, vurucu ve oyun diyaloğu gibi cevap ver.",
            stream = false,
            context = npcMemory
        };
        Response _response = new Response
        {
            done = false,
            context = null,
            response = ""
        };

        string JsonRequest = JsonConvert.SerializeObject(requestmodel);
        Debug.Log(JsonRequest);
        var icerik = new StringContent(JsonRequest, Encoding.UTF8, "application/json");
        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(url , JsonRequest))
        {
            request.timeout = 5;

            request.method = "POST";

            byte[] bodyRaw = Encoding.UTF8.GetBytes(JsonRequest);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            try
            {
                await request.SendWebRequest();
                
                if (request.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log("Ollama'ya bağlanılamadı!");
                    _response.response = "[SİSTEM HATASI] Ana sunucuya bağlanılamadı. Dr. Sullivan'ın ağı çevrimdışı.";
                    _response.done = true;
                    _response.context = npcMemory;
                    return _response;
                }
                else if (request.result == UnityWebRequest.Result.ProtocolError)
                {
                    _response.response = $"[SİSTEM UYARISI] İletişim Protokolü Hatası: {request.responseCode}.";
                    _response.done = true;
                    _response.context = npcMemory;
                    Debug.Log($"Ollama API Hatası: {request.responseCode} - {request.error}");
                    return _response;
                }
                string jsonResponse = request.downloadHandler.text;
                _response = JsonConvert.DeserializeObject<Response>(jsonResponse);
                npcMemory = _response.context;
                Debug.Log("NPC Cevabı: " + _response.response);
                return _response;

            }
            catch (Exception e)
            {
                Debug.LogWarning("Ollama'ya bağlanılamadı. Arka planda açık olduğundan emin olun.");
                Debug.LogError("Ollama Catch Hatası: " + e.Message + " | Detay: " + e.StackTrace);

            }
            _response.response = $"Ollama'ya bağlanılamadı. Arka planda açık olduğundan emin olun.: {request.responseCode}.";
            _response.done = true;
            _response.context = npcMemory;
            Debug.Log($"Ollama API Hatası: {request.responseCode} - {request.error}");
            return _response;
        }

    }
}
