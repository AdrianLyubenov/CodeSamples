using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/*
 * This is the container, responsible for keeping the type to instance collection, which is used by the DependenciesProvider.
 */
public class Kernel
{
    private Dictionary<Type, object> _container;

    public Kernel()
    {
        _container = new Dictionary<Type, object>();
    }

    // TODO: Remove generic, use obj.GetType()
    public void Add<T>(T obj)
        where T : class
    {
        if (_container.ContainsKey(typeof(T)))
        {
            Debug.LogError("Type {" + typeof(T).Name + "} already exists.");
            return;
        }

        _container.Add(typeof(T), obj);
    }

    public void AddOrUpdate(Type t, object obj)
    {
        if (_container.ContainsKey(t))
        {
            KLogger.Log("Type {" + t.Name + "} already exists. Replaing....");
            _container[t] = obj;
            return;
        }

        KLogger.Log("Type {" + t.Name + "} added to the service locator.");
        _container.Add(t, obj);
    }

    internal bool HasInstanceOfType<T>() where T : class
    {
        return _container.ContainsKey(typeof(T));
    }

    public void Add(Type t, object obj)
    {
        if (_container.ContainsKey(t))
        {
            KLogger.LogError("Type {" + t.Name + "} already exists.");
            return;
        }

        KLogger.Log("Type {" + t.Name + "} added to the service locator.");
        _container.Add(t, obj);
    }

    public List<T> GetAllOfType<T>()
        where T : class
    {
        List<T> list = _container.Values
            .Where(x => (x as T) != null)
            .Select(x => x as T)
            .ToList();

        if (list == null)
        {
            return new List<T>();
        }

        return list;
    }

    public T GetInstanceOfType<T>()
        where T : class
    {
        if (_container.ContainsKey(typeof(T)))
        {
            return _container[typeof(T)] as T;
        }
        else
        {
            Debug.LogError("Instance of type " + typeof(T).Name + " does not exist in the container.");
            return null;
        }
    }

    public bool Contains<T>()
    {
        return _container.ContainsKey(typeof(T));
    }

    public bool Contains(Type t)
    {
        return _container.ContainsKey(t);
    }

    public void Remove<T>()
    {
        if (_container.ContainsKey(typeof(T)))
        {
            _container.Remove(typeof(T));
        }
    }

    public void Remove(Type t)
    {
        if (_container.ContainsKey(t))
        {
            _container.Remove(t);
        }
    }

    public void Clear()
    {
        _container.Clear();
    }
}