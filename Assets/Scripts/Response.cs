using UnityEngine;

public class Response : MonoBehaviour
{
    public string model;
    public string created_at;
    public string response;      
    public bool done;
    public int[] context;       

    public long total_duration;
    public long load_duration;
    public int prompt_eval_count;
    public int eval_count;
}
