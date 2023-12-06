using KingOfDestiny.Vip.Presenters;
using KingOfDestiny.Vip.Views;
using UnityEngine;

namespace KingOfDestiny.Vip.Installers
{
    public sealed class VipLevelUpInstaller : MonoBehaviour
    {
        [SerializeField] private VipLevelUpView vipLevelUpView = default;

        private VipLevelUpPresenter _vipLevelUpPresenter;
        
        private void Awake()
        {
            InitializeDependencies();
        }

        private void InitializeDependencies()
        {
            var vipIconsProvider = ServiceLocator.Instance.GetInstanceOfType<VipIconsProvider>();
            var vipManager = ServiceLocator.Instance.GetInstanceOfType<IVipManager>();
            var rewardsController = ServiceLocator.Instance.GetInstanceOfType<RewardsController>();
            var gameData = ServiceLocator.Instance.GetInstanceOfType<GameData>();
            
            _vipLevelUpPresenter = new VipLevelUpPresenter(vipManager, vipLevelUpView, rewardsController, 
                vipIconsProvider, gameData);
            
            _vipLevelUpPresenter.Subscribe();
        }

        private void OnDestroy()
        {
            _vipLevelUpPresenter.Unsubscribe();
        }
    }
}