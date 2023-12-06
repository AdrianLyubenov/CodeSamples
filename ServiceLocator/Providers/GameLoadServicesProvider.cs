using System;
using System.Collections.Generic;
using UnityEngine;

public class GameLoadServicesProvider : ServicesProvider
{
    [SerializeField] private AudioController _audioController;

    protected override void PrepareServicesToAdd()
    {
        _services = new Dictionary<Type, object>();
        
        _services.Add(typeof(AudioController), _audioController);

        foreach (var monoDependency in _services)
        {
            if (monoDependency.Value == null)
            {
                Debug.LogError("Dependency is missing: " + monoDependency.Key.Name);
            }
        }
    }
}
