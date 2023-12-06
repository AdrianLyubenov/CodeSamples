using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuthenticationServicesProvider : ServicesProvider
{
    [SerializeField] private PopupsController _popupsController;
    [SerializeField] private NotificationController _notificationController;
    [SerializeField] private AudioController _audioController;
    [SerializeField] private ContentLoader _contentLoader;
    [SerializeField] private EnvironmentData _environmentData;
    [SerializeField] private LevelsDataProvider _levelsDataProvider;
    [SerializeField] private QuestsManager _questsManager;
    [SerializeField] private CoroutineHost _coroutineHost;

    protected override void PrepareServicesToAdd()
    {
        _services = new Dictionary<Type, object>();

        _services.Add(typeof(GroupsData), new GroupsData());

        _services.Add(typeof(TitleData), new TitleData());
        _services.Add(typeof(PlayerData), new PlayerData());
        _services.Add(typeof(PlayerProfile), new PlayerProfile());

        _services.Add(typeof(ServerCom), new ServerCom());
        _services.Add(typeof(PopupsController), _popupsController);
        _services.Add(typeof(NotificationController), _notificationController);
        _services.Add(typeof(AudioController), _audioController);
        _services.Add(typeof(ContentLoader), _contentLoader);
        _services.Add(typeof(EnvironmentData), _environmentData);
        _services.Add(typeof(LevelsDataProvider), _levelsDataProvider);
        _services.Add(typeof(QuestsManager), _questsManager);
        _services.Add(typeof(CoroutineHost), _coroutineHost);

        foreach (var monoDependency in _services)
        {
            if (monoDependency.Value == null)
            {
                Debug.LogError("Dependency is missing: " + monoDependency.Key.Name);
            }
        }

        _servicesToOverride = new List<Type>();
        _servicesToOverride.Add(typeof(TitleData));
        _servicesToOverride.Add(typeof(PlayerData));
        _servicesToOverride.Add(typeof(PlayerProfile));
    }
}