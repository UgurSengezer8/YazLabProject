using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    NPCConnectionLLM denem = new NPCConnectionLLM();
    public static GameManager Instance { get; private set; }
    public event EventHandler ButtonPresed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (Instance==null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    void Start()
    {
        var A =  NPCConnectionLLM.instance.ConnectionLLM("merhaba");
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void buttonPres()
    {
        ButtonPresed?.Invoke(this,EventArgs.Empty);
    }
}
