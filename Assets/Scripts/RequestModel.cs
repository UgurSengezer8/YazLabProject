using UnityEngine;

public class RequestModel
{
    public string model { get; set; } = "gemma2:2b";
    public string system { get; set; } 
    public string prompt { get; set; }
    public bool stream { get; set; }
    public object format { get; set; } = null;
    public int[] context { get; set; }
}
