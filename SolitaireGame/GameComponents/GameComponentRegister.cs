using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class GameComponentRegister : MonoBehaviour
    {
        public ComponentProvider componentProvider;
        public List<MonoBehaviour> components;

        private List<MonoBehaviour> registeredComponents = new List<MonoBehaviour>();

        private void Awake()
        {
            if (componentProvider != null)
            {
                for (int i = 0; i < components.Count; i++)
                {
                    if (!componentProvider.components.HasType(components[i].GetType()))
                    {
                        Debug.Log("REGISTER " + components[i].GetType());
                        componentProvider.Register(components[i]);
                        registeredComponents.Add(components[i]);
                    }
                }
            }
            else
            {
                throw new ArgumentNullException("ComponentProvider is missing!");
            }
        }

        private void OnDestroy()
        {
            foreach(object component in registeredComponents)
            {
                componentProvider.Unregister(component);
            }
        }
    }
