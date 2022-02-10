using System.Collections.Generic;
using UnityEngine.Events;

public class EventManager
{
    private class InternalEvent<T> : UnityEvent<T> { }

    private static readonly Dictionary<System.Type, object> eventDictionary = new Dictionary<System.Type, object>();

    public static void Subscribe<T>(UnityAction<T> listener)
    {
        object thisEvent;
        if (eventDictionary.TryGetValue(typeof(T), out thisEvent))
        {
            ((UnityEvent<T>)thisEvent).AddListener(listener);
        }
        else
        {
            var ev = new InternalEvent<T>();
            ev.AddListener(listener);
            eventDictionary.Add(typeof(T), ev);
        }
    }

    public static void Unsubscribe<T>(UnityAction<T> listener)
    {
        object thisEvent;
        if (eventDictionary.TryGetValue(typeof(T), out thisEvent))
        {
            ((UnityEvent<T>)thisEvent).RemoveListener(listener);
        }
    }

    public static void Broadcast<T>(T context) where T : struct
    {
        object thisEvent;
        if (eventDictionary.TryGetValue(typeof(T), out thisEvent))
        {
            ((UnityEvent<T>)thisEvent).Invoke(context);
        }
    }

    public static T SyncBroadcast<T>(T context) where T : class
    {
        object thisEvent;
        if (eventDictionary.TryGetValue(typeof(T), out thisEvent))
        {
            ((UnityEvent<T>)thisEvent).Invoke(context);
        }
        return context;
    }

    public static void CleanUp()
    {
        eventDictionary.Clear();
    }
}
