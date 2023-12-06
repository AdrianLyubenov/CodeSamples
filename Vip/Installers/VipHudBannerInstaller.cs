using KingOfDestiny.Vip.Presenters;
using KingOfDestiny.Vip.Views;
using UnityEngine;

namespace KingOfDestiny.Vip.Installers
{
    public sealed class VipHudBannerInstaller : MonoBehaviour
    {
        [SerializeField] private VipHudBannerView vipHudBannerView = default;

        private VipHudBannerPresenter _vipHudBannerPresenter;
        
        private void Awake()
        {
            InitializeDependencies();
        }

        private void InitializeDependencies()
        {
            var vipIconsProvider = ServiceLocator.Instance.GetInstanceOfType<VipIconsProvider>();
            var vipManager = ServiceLocator.Instance.GetInstanceOfType<IVipManager>();
            
            _vipHudBannerPresenter = new VipHudBannerPresenter(vipIconsProvider, vipManager, vipHudBannerView);
            
            _vipHudBannerPresenter.Initialize();
            _vipHudBannerPresenter.Subscribe();
        }

        private void OnDestroy()
        {
            _vipHudBannerPresenter.Unsubscribe();
        }
    }
}