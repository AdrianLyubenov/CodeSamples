using KingOfDestiny.Vip.Data;
using KingOfDestiny.Vip.ViewModels;
using KingOfDestiny.Vip.Views;
using UnityEngine;

namespace KingOfDestiny.Vip.Presenters
{
    public sealed class VipHudBannerPresenter
    {
        private readonly VipIconsProvider _vipIconsProvider;
        private readonly IVipManager _vipManager;
        private readonly VipHudBannerView _vipHudWidgetView;

        private VipBadgeViewModel _viewModel;
        
        public VipHudBannerPresenter(VipIconsProvider vipIconsProvider,
            IVipManager vipManager,
            VipHudBannerView vipHudWidgetView)
        {
            _vipIconsProvider = vipIconsProvider;
            _vipManager = vipManager;
            _vipHudWidgetView = vipHudWidgetView;
        }

        public void Initialize()
        {
            _viewModel = new VipBadgeViewModel();
            
            OnVipDataUpdated(_vipManager.GetVipData());
            
            _vipHudWidgetView.SetModel(_viewModel);
        }

        public void Subscribe()
        {
            _vipManager.VipDataUpdated += OnVipDataUpdated;
        }
        
        public void Unsubscribe()
        {
            _vipManager.VipDataUpdated -= OnVipDataUpdated;
        }
        
        private void OnVipDataUpdated(VipData data)
        {
            if (data == null) return;
            
            _viewModel.SetLevel(data.VipLevelIndex);

            Sprite vipIcon = _vipIconsProvider.GetIcon(data.VipLevelIndex);
            
            _viewModel.SetIcon(vipIcon);
        }
    }
}