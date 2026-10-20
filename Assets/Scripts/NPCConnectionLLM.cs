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
    public static NPCConnectionLLM instance;
    private int[] npcMemory = null;
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
    public void Start()
    {
        
        ConnectionLLM("merhaba").Forget();
    }
   
    
    public async UniTask<Response> ConnectionLLM(string mesaj)
    {
        
        string url = "http://localhost:11434/api/generate";


        RequestModel requestmodel = new RequestModel
        {
            model = "gemma2:2b",
            prompt = mesaj,
            system= "Karanlık bir zindanın kapısında nöbet tutan aksi ve şüpheci bir ork muhafızısın. Oyuncuya her zaman kaba ve tehditkar bir şekilde cevap ver. Yalnızca istenen JSON formatında yanıt üret; başka hiçbir açıklama yapma. Adın Hank.",
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

                throw;
            }
            
            return _response;
        }

    }
}
