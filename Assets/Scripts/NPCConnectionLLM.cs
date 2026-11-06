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
            DontDestroyOnLoad(this);
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
            model = "gemma2:2b",
            prompt = mesaj.content,
            system= "Sen Hank adında, karanlık bir zindan kapısını koruyan devasa ve aksi bir ork muhafızısın. Oyuncuya her zaman üstten bakan, alaycı ve kaba bir tavırla cevap ver. Asla kibar olma veya ona yardım etme. Sadece söyleyeceğin sözü yaz, eylem veya duygu belirtme.",
            stream = false,
            context = null
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
                    Debug.LogError("Ollama'ya bağlanılamadı!");
                    return _response;
                }
                else if (request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError($"Ollama API Hatası: {request.responseCode} - {request.error}");
                    return _response;
                }
                string jsonResponse = request.downloadHandler.text;
                _response = JsonConvert.DeserializeObject<Response>(jsonResponse);
                npcMemory = _response.context;
                Debug.Log("NPC Cevabı: " + _response.response);
            }
            catch (Exception)
            {
                Debug.LogWarning("Ollama'ya bağlanılamadı. Arka planda açık olduğundan emin olun.");
                
            }
            
            return _response;
        }

    }
}
