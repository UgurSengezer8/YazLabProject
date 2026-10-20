using System.Collections.Generic;
using UnityEngine;

public static class EventBus<T> where T : IEvent
{
    private static readonly HashSet<IEventBinding<T>> bindings = new HashSet<IEventBinding<T>>();

    public  static void Subscribe(EventBinding<T> binding) =>bindings.Add(binding);
    public  static void UnSubscribe(EventBinding<T> binding) => bindings.Remove(binding);

    public static void Publish ( T eventToPublish)
    {
        foreach (var binding in bindings)
        {
            binding.onEvent.Invoke(eventToPublish);
            binding.onEventNoArgs.Invoke();

        }
    }
    private static void Clear()
    {
        bindings.Clear();
        Debug.Log("Clearing" + typeof(T).Name + " bindings");
    }
}
