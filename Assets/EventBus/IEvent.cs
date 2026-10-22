using UnityEngine;

public interface IEvent 
{
}
public struct UIEvent : IEvent
{
    public int value;
}
public struct SoundEvent : IEvent
{

}
public struct SendRequestEvent : IEvent
{
    public string content;
}
public struct GetResponseEvent : IEvent
{
    public string content;
}