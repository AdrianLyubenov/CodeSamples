using Assets.Main.Scripts.Data.AzureFunctionMessages;
using Cysharp.Threading.Tasks;
using Nethereum.RPC.Eth.Subscriptions;
using Newtonsoft.Json;
using PlayFab.MultiplayerModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sirenix.Utilities;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public class LoaderElement
{
    public const int MAX_GODDATA_RETRIEVAL_AMOUNT = 10;
    private Action _onDone;
    private Action<string> _onError;

    private Stopwatch _timer = new Stopwatch();

    private static int _loadingCounter;
    public static bool AnyLoading => _loadingCounter > 0;

    public static Subject<Unit> OnLoadingStateChange = new Subject<Unit>();

    public static Dictionary<Type, LoaderElement> loadersInFlight = new Dictionary<Type, LoaderElement>();

    public virtual void Load(Action onDoneCallback, Action<string> onError)
    {
        Debug.Log($"STARTING LOADING OF {GetType()}!  ST: {Environment.StackTrace}");
        this._onDone += onDoneCallback;
        this._onError = onError;

        _loadingCounter++;
        _timer.Start();
        OnLoadingStateChange.OnNext(Unit.Default);
    }

    public virtual void LoadUnique(Action onDoneCallback, Action<string> onError)
    {
        this._onError = onError;
        this._onDone = onDoneCallback;
        if (loadersInFlight.ContainsKey(GetType()))
        {
            _loadingCounter++;
            OnError("Duplicate loader element can't be in flight at the same time!");
            return;
        }
        else
        {
            this._onDone += () => loadersInFlight.Remove(GetType());
            this._onError += (_) => loadersInFlight.Remove(GetType());
            loadersInFlight.Add(GetType(), this);
            Debug.Log($"STARTING UNIQUE LOADER");
            Load(_onDone, _onError);
        }
    }

    public void AddOnDoneAction(Action action)
    {
        this._onDone += action;
    }

    protected void LoadIsDone()
    {
        _timer.Stop();
        KLogger.Log($"{this.GetType().Name} | {_timer.Elapsed.TotalMilliseconds / 1000f}", Feature.ContentLoad);
        _onDone?.Invoke();
        _loadingCounter--;
        Debug.Log($"LOADING OF {GetType()} DONE !  ST: {Environment.StackTrace}");
        OnLoadingStateChange.OnNext(Unit.Default);
    }

    protected void OnError(string errorMessage)
    {
        Debug.Log($"ERROR LOADING: {GetType()}!  ST: {Environment.StackTrace}");
        _onError?.Invoke(errorMessage);
        _loadingCounter--;
        OnLoadingStateChange.OnNext(Unit.Default);
    }
}

public class LevelDataLoaderElement : LoaderElement
{
    public override void Load(Action onDoneCallback, Action<string> onError)
    {
        base.Load(onDoneCallback, onError);

        LoadGeneralLevelDataAsync();
    }

    private async void LoadGeneralLevelDataAsync()
    {
        var backgroundsTask = AddressableManager.Instance.LoadAssets<Sprite>(new List<string>(){"level_background"}, this);
        
        var _levelsDataProvider = ServiceLocator.Instance.GetInstanceOfType<LevelsDataProvider>();
        var _titleData = ServiceLocator.Instance.GetInstanceOfType<TitleData>();

#if UNITY_EDITOR
        if (_levelsDataProvider.useLocalLevelsGeneralData)
        {
            _titleData.PopulateLevelsData(_levelsDataProvider.localLevelData.ToDictionary(x => x.generalData.data.levelId, x => new ServerLevelData(){General = x.generalData.data, Rewards = x.constantData.rewards}));
            _levelsDataProvider.LoadLevelsConstantData();
            LoadIsDone();
            return;
        }
#endif
        var _serverCom = ServiceLocator.Instance.GetInstanceOfType<ServerCom>();
        var allLevelVersions = await _serverCom.GetAllLevelVersions();
        var levelsData = new Dictionary<string, ServerLevelData>();

        var levelsToDownload = allLevelVersions.Select(x => x.id).ToList();
        if (_levelsDataProvider.useCache &&  PlayerPrefs.HasKey("ALL_LEVELS_DATA"))
        {
            Debug.Log("Loading Levels from Cache!");

            levelsData = JsonConvert
                .DeserializeObject<Dictionary<string, ServerLevelData>>(PlayerPrefs.GetString("ALL_LEVELS_DATA"));
            foreach (var levelId in levelsData.Keys.Where(levelId => !levelsToDownload.Contains(levelId)).ToList())
            {
                levelsData.Remove(levelId);
            }
            var cachedLevelsVersion = levelsData.Select(x => new LevelVersion()
                {id = x.Value.General.levelId, timeStamp = x.Value._ts}).ToList();
            
            var levelsToUpdate = allLevelVersions.Except(cachedLevelsVersion, new LevelsVersionComparer()).Select(x => x.id).ToList();
            var newLevelsData = await _serverCom.GetLevelsByIDs(levelsToUpdate);
            foreach (var levelData in newLevelsData)
            {
                if (levelsData.ContainsKey(levelData.General.levelId))
                {
                    levelsData[levelData.General.levelId] = levelData;
                }
                else
                {
                    levelsData.Add(levelData.General.levelId, levelData);
                }
            }
        }
        else
        {
            Debug.Log("No Cache for Levels. Download Started!");
            
            var newLevelsData = await _serverCom.GetLevelsByIDs(levelsToDownload);
            newLevelsData.ForEach(x => levelsData.Add(x.General.levelId, x));
        }
        
        _titleData.PopulateLevelsData(levelsData);
        PlayerPrefs.SetString("ALL_LEVELS_DATA", JsonConvert.SerializeObject(levelsData));
        await backgroundsTask;
        _levelsDataProvider.LoadLevelsConstantData();
        LoadIsDone();
    }

}

public class PopupCampaignsLoaderElement : LoaderElement
{
    private Action _onDoneCallback;
    
    public PopupCampaignsLoaderElement(Action onDoneCallback)
    {
        _onDoneCallback += onDoneCallback;
    }
    public override void Load(Action onDoneCallback, Action<string> onError)
    {
        _onDoneCallback += onDoneCallback;
        base.Load(_onDoneCallback, onError);

        LoadPopupDataAsync();
    }

    private async void LoadPopupDataAsync()
    {
        var sCom = ServiceLocator.Instance.GetInstanceOfType<ServerCom>();
        var popupsData = await sCom.GetPopupCampaignsData();

        var data = JsonConvert.DeserializeObject<List<DynamicPopupData>>(popupsData, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
            
        var titleData = ServiceLocator.Instance.GetInstanceOfType<TitleData>();
        titleData.PopulatePopupsData(data);
        foreach (var internetContentElement in titleData.popupsData
                     .OrderByDescending(x => x.Priority).ThenBy(x => x.Triggers.Min())
                     .SelectMany(x => x.ContentElements).Where(x => x.Type == PopupContentType.InternetContent))
        {
            var contentEl = internetContentElement as InternetContentElement;
            switch (contentEl.InternetContentType)
            {
                case PopupInternetContentType.Image:
                case PopupInternetContentType.Gif:
                    TextureCache.Get().From(contentEl.SourceLink).SetLogging(false).Execute();
                    break;
                case PopupInternetContentType.Video:
                    VideoCache.DownloadVideo(contentEl.SourceLink);
                    break;
                default:
                    Debug.LogError("invalid internet content type");
                    break;
            }
        }
        LoadIsDone();
    }
}


public class QuestsLoaderElement : LoaderElement
{
    public override void Load(Action onDoneCallback, Action<string> onError)
    {
        base.Load(onDoneCallback, onError);

        LoadQuestsDataAsync();
    }

    private async void LoadQuestsDataAsync()
    {
        var questsManager = ServiceLocator.Instance.GetInstanceOfType<QuestsManager>();
        var sCom = ServiceLocator.Instance.GetInstanceOfType<ServerCom>();
#if UNITY_EDITOR
        if (questsManager.useLocalQuests)
        {
            questsManager.PopulateQuestsData(questsManager.localQuests.GetQuests());
            LoadIsDone();
            return;
        }
#endif
        questsManager.PopulateQuestsData(await sCom.GetActiveQuests());
        LoadIsDone();
    }
}

public class QuestsProgressLoaderElement : LoaderElement
{
    public override void Load(Action onDoneCallback, Action<string> onError)
    {
        base.Load(onDoneCallback, onError);

        LoadQuestsProgressAsync();
    }

    private async void LoadQuestsProgressAsync()
    {
        var questsManager = ServiceLocator.Instance.GetInstanceOfType<QuestsManager>();
        var sCom = ServiceLocator.Instance.GetInstanceOfType<ServerCom>();
#if UNITY_EDITOR
        if (questsManager.useLocalQuestsProgress)
        {
            questsManager.PopulateQuestsProgress(questsManager.localQuestProgress.GetProgress());
            LoadIsDone();
            return;
        }
#endif
        questsManager.PopulateQuestsProgress(await sCom.GetQuestsProgress());
        LoadIsDone();
    }
}

public class TitleDataLoaderElement : LoaderElement
{
    private Action _onDoneCallback;
    
    public TitleDataLoaderElement(Action onDoneCallback)
    {
        _onDoneCallback += onDoneCallback;
    }
    
    public override void Load(Action onDoneCallback, Action<string> onError)
    {
        _onDoneCallback += onDoneCallback;
        base.Load(_onDoneCallback, onError);

        LoadTitleDataAsync();
    }

    private async void LoadTitleDataAsync()
    {
        var sCom = ServiceLocator.Instance.GetInstanceOfType<ServerCom>();

        var titleDataResponse = await sCom.GetTitleData();

        ServiceLocator.Instance.GetInstanceOfType<TitleData>().PopulateData(titleDataResponse);
        LoadIsDone();
    }
}


public class StateCheckLoaderElement : LoaderElement
{
    public override void Load(Action onDoneCallback, Action<string> onError)
    {
        base.Load(onDoneCallback, onError);

        LoadPlayerDataAsync();
    }

    private async void LoadPlayerDataAsync()
    {
        var sCom = ServiceLocator.Instance.GetInstanceOfType<ServerCom>();
        var pData = ServiceLocator.Instance.GetInstanceOfType<PlayerData>();
        var newTriesData = await sCom.GetPlayerDataAsync(includeReadOnly: true,
            keys: new List<string>() {"PlaysRemaining", "ClaimedRewards"});
        if (!string.IsNullOrEmpty(newTriesData.ErrorMessage))
        {
            OnError("Failed Initial Title Data load" + newTriesData.ErrorMessage);
            return;
        }

        int newTries = int.Parse(newTriesData.Result["PlaysRemaining"].Value);
        pData.DailyPlaysRemaining.SetValueAndForceNotify(newTries);

        LoadIsDone();
    }
}

public class LimitedTimeEventsLoaderElement : LoaderElement
{
    private Action _onDoneCallback;
    
    public LimitedTimeEventsLoaderElement(Action onDoneCallback)
    {
        this._onDoneCallback += onDoneCallback;
    }
    
    public override void Load(Action onDoneCallback, Action<string> onError)
    {
        _onDoneCallback += onDoneCallback;
        base.Load(_onDoneCallback, onError);
        LoadLimitedTimeEventsAsync();
    }

    public async void LoadLimitedTimeEventsAsync()
    {
        var _titleData = ServiceLocator.Instance.GetInstanceOfType<TitleData>();
        var _levelsDataProvider = ServiceLocator.Instance.GetInstanceOfType<LevelsDataProvider>();
        var serverCom = ServiceLocator.Instance.GetInstanceOfType<ServerCom>();
        List<LevelSchedule> limitedTimeEvents;
#if UNITY_EDITOR
        if (_levelsDataProvider.useLocalLimitedTimeEvents)
        {
            Debug.Log("Loading pantheon data from local data");
            limitedTimeEvents = _levelsDataProvider.localLimitedTimeEvents.Select(x => x.schedule).ToList();
            _titleData.PopulateLimitedTimeEventsData(limitedTimeEvents);
            LoadIsDone();
            return;
        }
#endif
        
        Debug.Log("Loading limited time events data data from server");
        limitedTimeEvents = await serverCom.GetLimitedTimeEvents();
        
        _titleData.PopulateLimitedTimeEventsData(limitedTimeEvents);
        LoadIsDone();
    }
}

public class UnlockedLevelsLoaderElement : LoaderElement
{
    public override void Load(Action onDoneCallback, Action<string> onError)
    {
        base.Load(onDoneCallback, onError);

        LoadUnlockedLevelsAsync();
    }

    private async void LoadUnlockedLevelsAsync()
    {
        var serverCom = ServiceLocator.Instance.GetInstanceOfType<ServerCom>();
        var titleData = ServiceLocator.Instance.GetInstanceOfType<TitleData>();
        var _levelsDataProvider = ServiceLocator.Instance.GetInstanceOfType<LevelsDataProvider>();

        var panthData = titleData.pantheonsContentData;
#if UNITY_EDITOR
        if (_levelsDataProvider.useLocalUnlockedLevels)
        {
            _levelsDataProvider.SetUnlockedLevels(new HashSet<string>(_levelsDataProvider.localUnlockedLevels));
            LoadIsDone();
            return;
        }
#endif
        var unlockedLevels = await serverCom.GetLastUnlockedLevelForAllPantheons();
        HashSet<string> unlockedLevelsSet = new HashSet<string>();

        Debug.Log("Getting unlocked levels from server " + string.Join("\n", unlockedLevels.Select(x => x.Key + " | " + x.Value)));
        bool changed = false;
        foreach (var pantheonData in panthData)
        {
            var newMaxLevel = unlockedLevels.ContainsKey(pantheonData.general.pantheonId)
                ? unlockedLevels[pantheonData.general.pantheonId]
                : "";

            if (!string.IsNullOrEmpty(newMaxLevel))
            {
                foreach (var pantheonLevel in pantheonData.pantheonLevelsData.levels)
                {
                    unlockedLevelsSet.Add(pantheonLevel.levelId);
                    if (pantheonLevel.levelId == newMaxLevel)
                    {
                        break;
                    }
                }
            }
        }
        Debug.Log("Changed data from server " + changed);
        _levelsDataProvider.SetUnlockedLevels(unlockedLevelsSet);
        LoadIsDone();
    }
}

public class PlayerDataLoaderElement : LoaderElement
{
    public override void Load(Action onDoneCallback, Action<string> onError)
    {
        base.Load(onDoneCallback, onError);

        LoadPlayerDataAsync();
    }

    private async void LoadPlayerDataAsync()
    {
        var sCom = ServiceLocator.Instance.GetInstanceOfType<ServerCom>();
        var pData = ServiceLocator.Instance.GetInstanceOfType<PlayerData>();

        //first log in player
        var loginRq = await sCom.LogInPlayer(); // we want to complete login before continuing with data requests
        if (loginRq.Error != null)
        {
            Debug.LogError("Could not login");
        }
        else if (loginRq.Result.IsUnderMaintenance)
        {
            SceneManager.LoadScene("Maintenance");
            return;
        }
        
        //act on login (should be handled by data requests

        var playerDataRq = sCom.GetPlayerDataAsync(includeReadOnly: true);
        var playerInventoryRq = sCom.UpdatePlayerInventory();
        var followersRq = sCom.GetObtainedFollowers();
        //var claimRq = sCom.ClaimTimedRewards();


        var (playerData,
            playerInventory,
            followers) = await UniTask.WhenAll(playerDataRq, playerInventoryRq, followersRq);


        if (!string.IsNullOrEmpty(playerData.ErrorMessage))
        {
            OnError("Failed Initial Title Data load" + playerData.ErrorMessage);
            return;
        }

        pData.UnclaimedLeaderboardsRewards = loginRq.Result.LeaderboardsRewards;
        pData.UnclaimedQuestRewards = loginRq.Result.QuestRewards;
        
        pData.PopulateData(playerData.Result);
        pData.SaveLevelsData();


        if (!string.IsNullOrEmpty(playerInventory.ErrorMessage))
        {
            OnError("Failed Player Inventory Load!" + playerData.ErrorMessage);
            return;
        }

        pData.UpdateInventory(followers);
        LoadIsDone();
    }
}

public class PlayerProfileLoaderElement : LoaderElement
{
    public override void Load(Action onDoneCallback, Action<string> onError)
    {
        base.Load(onDoneCallback, onError);

        LoadPlayerProfileAsync();
    }

    private async void LoadPlayerProfileAsync()
    {
        var response = await ServiceLocator.Instance.GetInstanceOfType<ServerCom>().GetPlayerProfile();

        if (!string.IsNullOrEmpty(response.ErrorMessage))
        {
            OnError("Failed Initial Player Title Data load" + response.ErrorMessage);
            return;
        }

        var playerProfile = ServiceLocator.Instance.GetInstanceOfType<PlayerProfile>();
        playerProfile.displayName.Value = response.Result.PlayerProfile.DisplayName;
        playerProfile.avatarUrl.Value = response.Result.PlayerProfile.AvatarUrl;
        playerProfile.playfabId.Value = response.Result.PlayerProfile.PlayerId;
        playerProfile.creationDate.Value = response.Result.PlayerProfile.Created ?? DateTime.Now;
        
        LoadIsDone();
    }
}

public class FollowersForPlaysLoaderElement : LoaderElement
{
    public override void Load(Action onDoneCallback, Action<string> onError)
    {
        base.Load(onDoneCallback, onError);

        LoadFollowersForPlaysAsync();
    }

    private async void LoadFollowersForPlaysAsync()
    {
        var svCom = ServiceLocator.Instance.GetInstanceOfType<ServerCom>();
        var playerData = ServiceLocator.Instance.GetInstanceOfType<PlayerData>();

        playerData.followersForPlays = await svCom.GetPlaysWithFollowersPrice();

        LoadIsDone();
    }
}

public class NFTDataLoaderElement : LoaderElement
{
    private bool hardRefresh;

    public NFTDataLoaderElement(bool hardRefresh)
    {
        this.hardRefresh = hardRefresh;
    }
    private PlayerData playerData => ServiceLocator.Instance.GetInstanceOfType<PlayerData>();
    private ServerCom sCom = ServiceLocator.Instance.GetInstanceOfType<ServerCom>();

    public override void Load(Action onDoneCallback, Action<string> onError)
    {
        base.Load(onDoneCallback, onError);
        LoadGods();
    }
    
    public async Task LoadGods()
    {
        Debug.Log("Loading Gods!");

        List<GodPlayerData> godsStatusForPlayer;
        List<GodDataItem> godsDataForPlayer;
        
        if (PlayerPrefs.HasKey("GODS_FOF_PLAYER") && !hardRefresh)
        {
            godsStatusForPlayer = JsonConvert.DeserializeObject<List<GodPlayerData>>(PlayerPrefs.GetString("GODS_FOF_PLAYER"));
        }
        else
        {
            godsStatusForPlayer = await sCom.GetGodsStatusForPlayer();
            godsDataForPlayer = await sCom.GetGodsDataForPlayer();
            godsStatusForPlayer.ForEach(x => x.dataItem = godsDataForPlayer.FirstOrDefault(y => x.id == "Elder God #" + y.id));

            PlayerPrefs.SetString("GODS_FOF_PLAYER", JsonConvert.SerializeObject(godsStatusForPlayer));
        }
        playerData.PopulateGodsData(godsStatusForPlayer);

        await LoadGodsImages();
        EventsManager.OnNftsLoaded.OnNext(Unit.Default);
        await LoadGodBorderImages(playerData);

        var selectedGods = sCom.GetMenuSelectedGods();
        playerData.UnselectAllGods();

        if (selectedGods != null && selectedGods.godIds != null)
        {
            Debug.Log($"Selecting {selectedGods.godIds.Count} gods!");
            playerData.SetSelectedGods(selectedGods.godIds.Select(g => (g, true)));
        }

        Debug.Log("NFTs loaded successfully.");
        LoadIsDone();
    }

    private async UniTask LoadGodBorderImages(PlayerData playerData)
    {
        var tasks = new Dictionary<string, Task<TextureCacheResult>>();
        playerData.godBorderTextures.Clear();
        int i = 0;
        foreach (var item in playerData.gods)
        {
            string id = Regex.Match(item.id, @"\d+").Value;
            var url = "https://infinimerge-closedbeta.s3.eu-central-1.amazonaws.com/Assets/Gods/GodsWithBorders/" + id +
                      ".jpg";
            var result = TextureCache.Get().From(url).SetLogging(false).Execute();
            tasks.Add(id, result);
        }

        await Task.WhenAll(tasks.Values);
        foreach (var result in tasks)
        {
            if (playerData.godBorderTextures.ContainsKey(result.Key))
            {
                playerData.godBorderTextures[result.Key] = result.Value.Result.Texture;
            }
            else
            {
                playerData.godBorderTextures.Add(result.Key, result.Value.Result.Texture);
            }
        }
    }

    private async UniTask GetTexture(GodPlayerData data)
    {
        var log = false;
#if DEV_MODE
        log = true;
#endif
        if (data.dataItem == null || data.dataItem.Portrait == null) return;
        var fetchURL =
            "https://infinimerge-closedbeta.s3.eu-central-1.amazonaws.com/Assets/Gods/Standalone+Portraits+SMALL2/" +
            data.dataItem.Portrait;
        var cachedResult = await TextureCache.Get().From(fetchURL).SetLogging(log).Execute();
        data.texture = cachedResult.Texture;
    }

    public async UniTask<Unit> LoadGodsImages()
    {
        var tasks = new List<UniTask>();
        foreach (var item in playerData.gods)
        {
            tasks.Add(GetTexture(item));
        }

        await UniTask.WhenAll(tasks);
        return Unit.Default;
    }
}

// Needs title data loaded
public class WalletDataLoaderElement : LoaderElement
{
    private bool forceRefresh;

    public void SetForcedResyncOverride(bool value)
    {
        forceRefresh = value;
    }

    public override void Load(Action onDoneCallback, Action<string> onError)
    {
        base.Load(onDoneCallback, onError);
        LoadWalletDataFromServerAsync();
    }

    private async void LoadWalletDataFromServerAsync()
    {
        var sCom = ServiceLocator.Instance.GetInstanceOfType<ServerCom>();
        var playerProfile = ServiceLocator.Instance.GetInstanceOfType<PlayerProfile>();
        var playerData = ServiceLocator.Instance.GetInstanceOfType<PlayerData>();
        var cachedWalletId = PlayerPrefs.GetString("WALLET_ID", "");
        
        WalletRecord wr = await sCom.GetPlayerWalletId();
        if (wr != null && !string.IsNullOrEmpty(wr.WalletId))
        {
            if (playerProfile.walletAddress.Value == null || cachedWalletId != wr.WalletId)
            {
                RefreshWallet(wr.WalletId);
                RefreshGods();
            }
                        
            if (forceRefresh) 
                RefreshGods();

            await RefreshPasses();
        }
        else
        {
            PlayerPrefs.DeleteKey("WALLET_ID");    
            PlayerPrefs.DeleteKey("GODS_FOF_PLAYER");    
            playerData.gods.Clear();
            playerProfile.infiniPass.SetValueAndForceNotify(InfiniPass.None);
            playerProfile.walletAddress.Value = null;
        }
        LoadIsDone();
    }

    private void RefreshWallet(string walletID)
    {
        var playerProfile = ServiceLocator.Instance.GetInstanceOfType<PlayerProfile>();

        PlayerPrefs.SetString("WALLET_ID", walletID);    
        playerProfile.walletAddress.Value = walletID;
    }
    
    private async void RefreshGods()
    {
        EventsManager.OnWalletLoadBegin?.OnNext(Unit.Default);
        var nftLoader = new NFTDataLoaderElement(true);
        await nftLoader.LoadGods();
        EventsManager.OnWalletLoad.OnNext(Unit.Default);
    }

    private async Task RefreshPasses()
    {
        var passesLoader = new CheckInfiniPassLoaderElement();
        await passesLoader.LoadPasses();
    }
}

public class CheckInfiniPassLoaderElement : LoaderElement
{
    private readonly PlayerProfile _playerProfile;
    private readonly ServerCom _serverCom;

    public CheckInfiniPassLoaderElement()
    {
        _playerProfile = ServiceLocator.Instance.GetInstanceOfType<PlayerProfile>();
        _serverCom = ServiceLocator.Instance.GetInstanceOfType<ServerCom>();
    }

    public override void Load(Action onDoneCallback, Action<string> onError)
    {
        base.Load(onDoneCallback, onError);
        LoadPasses();
    }

    public async Task LoadPasses()
    {
        InfiniPass pass = await _serverCom.GetPlayerPasses();
        _playerProfile.infiniPass.SetValueAndForceNotify(pass);
        LoadIsDone();
    }
}

public class ReloadPlayerPackLoaderElement : LoaderElement
{
    public override void Load(Action onDoneCallback, Action<string> onError)
    {
        base.Load(onDoneCallback, onError);

        LoadAsync();
    }

    private async void LoadAsync()
    {
        var sCom = ServiceLocator.Instance.GetInstanceOfType<ServerCom>();
        var result = await sCom.GetPlayerGroupData();

        if (string.IsNullOrEmpty(result.ErrorMessage))
        {
            var playerData = ServiceLocator.Instance.GetInstanceOfType<PlayerData>();
            var newPackData = result.Result;
            if (playerData.packData.Value == null ||
                playerData.packData.Value.groupData.GroupName != newPackData.groupData.GroupName)
            {
                playerData.packData.Value = newPackData;
                playerData.packData.Value.PopulateAvatar(sCom);
            }
            else
            {
                playerData.packData.Value.groupScore = newPackData.groupScore;
                playerData.packData.Value.groupRank = newPackData.groupRank;
            }
        }
        else
        {
            UnityEngine.Debug.LogError(result.ErrorMessage);
        }

        LoadIsDone();
    }
}

public class InfiniPassMetadata
{
    public string name;
    public string image;
    public string animation_url;
    public string description;
}
