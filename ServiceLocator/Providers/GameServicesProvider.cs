using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameServicesProvider : ServicesProvider
{
    [SerializeField] private PopupsController _popupsController;
    [SerializeField] private AudioController _audioController;
#if ENABLE_GODS
    [SerializeField] private GodsController _godsController;
#endif

    protected override void PrepareServicesToAdd()
    {
        _services = new Dictionary<Type, object>();

        _services.Add(typeof(GroupsData), new GroupsData());
        _services.Add(typeof(TitleData), new TitleData());
        _services.Add(typeof(PlayerProfile), new PlayerProfile());

        _services.Add(typeof(PlayerData), new PlayerData());

        _services.Add(typeof(ServerCom), new ServerCom());
        _services.Add(typeof(PopupsController), _popupsController);

        _services.Add(typeof(AudioController), _audioController);
#if ENABLE_GODS
        _services.Add(typeof(GodsController), _godsController);
#endif

        foreach (var monoDependency in _services)
        {
            if (monoDependency.Value == null)
            {
                Debug.LogError("Dependency is missing: " + monoDependency.Key.Name);
            }
        }
    }
    /*
    protected override void PrepareServicesToRemoveOnSceneChange()
    {
        _servicesToRemoveOnSceneChange = new List<Type>();

        _servicesToRemoveOnSceneChange.Add(typeof(GameData));
        _servicesToRemoveOnSceneChange.Add(typeof(TitleData));
        _servicesToRemoveOnSceneChange.Add(typeof(PlayerProfile));
    }*/
}
