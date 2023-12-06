using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ServicesProvider : MonoBehaviour
{
    protected Dictionary<Type, object> _services;
    protected List<Type> _servicesToOverride;

    protected List<Type> _servicesToRemoveOnSceneChange;

    private void Awake()
    {
        if (ServiceLocator.Instance == null)
        {
            GameObject gameObject = new GameObject("ServiceLocator");
            ServiceLocator newServiceLocator = gameObject.AddComponent<ServiceLocator>();
            newServiceLocator.Init();
        }

        PrepareServicesToAdd();

        var _servicesTypes = _services.Keys.ToList();
        foreach (var t in _servicesTypes)
        {
            if (ServiceLocator.Instance.ContainsService(t))
            {
                // Destroying any services that already exist
                var mono = Convert.ChangeType(_services[t], t) as MonoBehaviour;
                if (mono != null)
                {
                    Destroy(mono.gameObject);
                }

                _services.Remove(t);
            }
        }

        ServiceLocator.Instance.AddServices(_services, _servicesToOverride);
    }

    private void Start()
    {
        ServiceLocator.Instance.InitializeServices(_services);
    }

    private void OnDestroy()
    {
        PrepareServicesToRemoveOnSceneChange();

        if (_servicesToRemoveOnSceneChange == null)
        {
            return;
        }

        foreach (var s in _servicesToRemoveOnSceneChange)
        {
            ServiceLocator.Instance.RemoveServices(_servicesToRemoveOnSceneChange);
        }
    }

    protected virtual void PrepareServicesToAdd()
    {
        _services = new Dictionary<Type, object>();

        /* 
        Examples:
        monoDependencies.Add(typeof(GameManager), gameManager);
        container.Add(new BonusesData());
        */

        foreach (var monoDependency in _services)
        {
            if (monoDependency.Value == null)
            {
                Debug.LogError("Dependency is missing: " + monoDependency.Key.Name);
            }
        }
    }

    protected virtual void PrepareServicesToRemoveOnSceneChange()
    {
        _servicesToRemoveOnSceneChange = new List<Type>();

        /* 
        Examples:
        monoDependencies.Add(typeof(GameManager), gameManager);
        container.Add(new BonusesData());
        */

        foreach (var service in _servicesToRemoveOnSceneChange)
        {
            if (service == null)
            {
                Debug.LogError("Dependency is missing: " + service.Name);
            }
        }
    }
}
