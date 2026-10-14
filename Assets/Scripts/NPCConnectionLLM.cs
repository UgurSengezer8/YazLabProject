using System;
using System.Net.Http;
using System.Text;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using Newtonsoft.Json;
using UnityEngine;


public class NPCConnectionLLM : MonoBehaviour
{
    private int[] npcMemory = null;
    public void Start()
    {
        ConnectionLLM(" hi whats up").Forget();
    }
    private async UniTask ConnectionLLM(string mesaj)
    {
        
        string url = "http://localhost:11434/api/generate";

        RequestModel requestmodel = new RequestModel
        {
            model = "llama3.2:1b",
            prompt = mesaj,
            system= "You are a surly and suspicious orc guard standing watch at the door of a dark dungeon. Always respond to the player in a rude and threatening manner. Generate a response only in the requested JSON format; do not provide any other explanation.your name is hank",
            stream = false,
            context = null
        };

        string JsonRequest = JsonConvert.SerializeObject(requestmodel);
        Debug.Log(JsonRequest);
        var icerik = new StringContent(JsonRequest, Encoding.UTF8, "application/json");
        using (HttpClient client = new HttpClient())
        {
            var response = await client.PostAsync(url, icerik);
            string jsonCevap = await response.Content.ReadAsStringAsync();
            Response sonuc = JsonConvert.DeserializeObject<Response>(jsonCevap);
            npcMemory = sonuc.context;
            Console.WriteLine("NPC Cevabı: " + sonuc.response);
        }

    }
}
