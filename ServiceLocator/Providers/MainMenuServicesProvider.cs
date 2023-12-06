using System;
using System.Collections.Generic;
using Main.Scripts.Game;
using UnityEngine;

public class MainMenuServicesProvider : ServicesProvider
{
    [SerializeField] private AudioController _audioController;

    protected override void PrepareServicesToAdd()
    {
        _services = new Dictionary<Type, object>();

        var statManager = new StatManager();
        _services.Add(typeof(StatManager), statManager);
        _services.Add(typeof(GroupsData), new GroupsData());
        _services.Add(typeof(PlayerData), new PlayerData());
        _services.Add(typeof(TitleData), new TitleData());
        _services.Add(typeof(LevelPlayManager), new LevelPlayManager());
        _services.Add(typeof(Web3AzureAuthentication), new Web3AzureAuthentication());
        _services.Add(typeof(AudioController), _audioController);
        _services.Add(typeof(DynamicPopupEventManager), new DynamicPopupEventManager(statManager));

        foreach (var monoDependency in _services)
        {
            if (monoDependency.Value == null)
            {
                Debug.LogError("Dependency is missing: " + monoDependency.Key.Name);
            }
        }
    }
}
