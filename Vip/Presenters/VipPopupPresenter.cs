using System;
using System.Threading.Tasks;
using KingOfDestiny.Configurations;
using KingOfDestiny.Vip.Data;
using KingOfDestiny.Vip.ViewModels;
using KingOfDestiny.Vip.Views;
using UniRx;
using UnityEngine;

namespace KingOfDestiny.Vip.Presenters
{
    public sealed class VipPopupPresenter : IVipPopupPresenter, IDisposable
    {
        private readonly VipPopup _popup = default;
        private readonly IVipManager _vipManager;
        private readonly VipIconsProvider _vipIconsProvider;

        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        private VipPopupViewModel _vipPopupViewModel;

        public VipPopupPresenter(IVipManager vipManager, VipPopup vipBenefitsPopup, VipIconsProvider vipIconsProvider)
        {
            _vipManager = vipManager;
            _popup = vipBenefitsPopup;
            _vipIconsProvider = vipIconsProvider;
        }

        public void Init()
        {
            VipData vipData = _vipManager.GetVipData();
            
            Sprite vipIcon = _vipIconsProvider.GetIcon(vipData.VipLevelIndex);

            int pointsRequiredForNextLevel = 0;
            
            if (_vipManager.TryGetLevelConfiguration(vipData.VipLevelIndex + 1,
                out VipLevelConfiguration nextVipLevelConfiguration))
            {
                pointsRequiredForNextLevel = nextVipLevelConfiguration.PointsRequired - vipData.VipPoints;
            }

            _vipManager.TryGetLevelConfiguration(vipData.VipLevelIndex, out VipLevelConfiguration currentLevelConfiguration);

            _vipPopupViewModel = new VipPopupViewModel(vipData.VipLevelIndex,
                vipIcon, vipData.VipPoints, 
                pointsRequiredForNextLevel, 
                nextVipLevelConfiguration,
                currentLevelConfiguration,
                _vipManager.IsMaxLevelReached(vipData.VipLevelIndex));

            _vipPopupViewModel.IsFirstPage = vipData.VipLevelIndex <= 1;
            _vipPopupViewModel.IsLastPage = _vipManager.IsMaxLevelReached(vipData.VipLevelIndex);
            
            _popup.SetViewModel(_vipPopupViewModel);

            Subscribe();
        }

        private void Subscribe()
        {
            _vipPopupViewModel.CurrentPage.Subscribe(OnCurrentPageChanged).AddTo(_compositeDisposable);
        }

        private void OnCurrentPageChanged(int page)
        {
            _vipPopupViewModel.IsFirstPage = page <= 1;
            _vipPopupViewModel.IsLastPage = _vipManager.IsMaxLevelReached(page);

            _vipManager.TryGetLevelConfiguration(page, out VipLevelConfiguration vipLevelConfiguration);
            
            _vipPopupViewModel.SetLevelConfigurationToDisplay(vipLevelConfiguration);
        }

        void IDisposable.Dispose()
        {
            _compositeDisposable?.Dispose();
        }
    }
}