using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    [CreateAssetMenu(fileName = "ComponentProvider", menuName = "Data/ComponentProvider")]
    public class ComponentProvider : ScriptableObject
    {
        public GameComponents components = new GameComponents();
        
        public void Register(object component)
        {
            components.Register(component);
        }

        public void Unregister(object component)
        {
            components.Unregister(component);
        }
    }
