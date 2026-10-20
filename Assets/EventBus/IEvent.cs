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
