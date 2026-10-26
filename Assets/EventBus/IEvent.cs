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
public struct PlayerMoveEvent : IEvent
{
    public Vector2 direction;
    public bool isWalking;
}