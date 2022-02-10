using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class GameComponents : MonoBehaviour
    {
        public List<object> components;
        private Dictionary<string, object> componentsByTypeName;

        internal void Register(object component)
        {
            components.Add(component);
        }

        internal void Unregister(object component)
        {
            components.Remove(component);
        }

        internal bool HasType(Type type)
        {
            if (componentsByTypeName.ContainsKey(type.FullName))
                return true;
            foreach (var each in components)
            {
                if (each != null && type.IsAssignableFrom(each.GetType()))
                {
                    return true;
                }
            }
            return false;
        }

        public GameComponents()
        {
            components = new List<object>();
            componentsByTypeName = new Dictionary<string, object>();
        }

        public void Clean()
        {
            components = null;
        }

        public T Get<T>() where T : class
        {
            T t = TryGet<T>();
            if (t == null)
                throw new NullReferenceException("Can't find component of type " + typeof(T).FullName + ".");
            return t;
        }

        public T TryGet<T>() where T : class
        {
            if (componentsByTypeName != null)
            {
                Type type = typeof(T);
                string typeName = type.FullName;
                if (componentsByTypeName.TryGetValue(typeName, out object component))
                    return (T)component;
                foreach (var each in components)
                {
                    if (type.IsAssignableFrom(each.GetType()))
                    {
                        componentsByTypeName[typeName] = each;
                        return (T)each;
                    }
                }
            }
            return default;
        }

        public List<T> GetAll<T>() where T : class
        {
            List<T> list = new List<T>();
            foreach (var each in components)
            {
                if (each is T tmp)
                {
                    list.Add(tmp);
                }
            }
            return list;
        }
    }
