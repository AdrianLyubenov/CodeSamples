using KingOfDestiny.Vip.ViewModels;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace KingOfDestiny.Vip.Views
{
    public sealed class VipPopup : Popup
    {
        [SerializeField] private VipIconView vipIconView = default;
        [SerializeField] private VipPointsView vipPointsView = default;
        [SerializeField] private VipBenefitsControlView vipBenefitsControlView = default;
        [SerializeField] private VipBenefitsContainerView vipBenefitsContainer = default;
        [SerializeField] private GameObject bannerParticles = default;

        [Space] [SerializeField] private Button m_CloseButton;
        
        private IVipPopupViewModel _vipPopupViewModel;

        private void OnDestroy()
        {
            Unsubscribe();
        }

        public override void Show()
        {
            m_CloseButton
                .OnClickAsObservable()
                .Subscribe(_ =>
                {
                    bannerParticles.SetActive(false);
                    base.Hide();
                })
                .AddTo(this);

            base.Show();
        }

        public void SetViewModel(IVipPopupViewModel vipPopupViewModel)
        {
            _vipPopupViewModel = vipPopupViewModel;

            Subscribe();

            UpdateBadge();
            UpdateSlider();
            UpdatePage();
        }

        private void UpdateBadge()
        {
            vipIconView.Set(_vipPopupViewModel.CurrentLevel.Value, _vipPopupViewModel.BadgeIcon.Value);
            bannerParticles.gameObject.SetActive(_vipPopupViewModel.CurrentLevel.Value != 0);
        }

        private void UpdateSlider()
        {
            var previousVipLevelPointsRequired = _vipPopupViewModel.BenefitsLevelConfiguration.Value.PointsRequired;
            var currentVipLevelPointsRequired = _vipPopupViewModel.CurrentPoints.Value - previousVipLevelPointsRequired;
            var nextVipLevelPointsRequired = _vipPopupViewModel.NextLevelConfiguration.Value?.PointsRequired - previousVipLevelPointsRequired ?? 0;
                
            vipPointsView.SetData(currentVipLevelPointsRequired,
                nextVipLevelPointsRequired,
                _vipPopupViewModel.PointsNeededForUpgrade.Value,
                _vipPopupViewModel.NextLevelConfiguration.Value?.Id ?? string.Empty,
                _vipPopupViewModel.IsMaxLevelReached.Value);
        }

        private void UpdatePage()
        {
            bool isPageLocked = _vipPopupViewModel.CurrentPage.Value >
                                _vipPopupViewModel.CurrentLevel.Value;

            vipBenefitsControlView.SetData(_vipPopupViewModel.CurrentPage.ToString(), _vipPopupViewModel.IsFirstPage,
                _vipPopupViewModel.IsLastPage, isPageLocked);

            vipBenefitsContainer.UpdateView(_vipPopupViewModel.BenefitsLevelConfiguration.Value, isPageLocked);
        }

        private void Subscribe()
        {
            vipBenefitsControlView.Subscribe();
            vipBenefitsControlView.LeftArrowClicked += VipBenefitsControlViewOnLeftArrowClicked;
            vipBenefitsControlView.RightArrowClicked += VipBenefitsControlViewOnRightArrowClicked;

            _vipPopupViewModel.BenefitsLevelConfiguration.Subscribe((_) => UpdatePage()).AddTo(this);
        }

        private void Unsubscribe()
        {
            vipBenefitsControlView.Unsubscribe();
            vipBenefitsControlView.LeftArrowClicked -= VipBenefitsControlViewOnLeftArrowClicked;
            vipBenefitsControlView.RightArrowClicked -= VipBenefitsControlViewOnRightArrowClicked;
        }

        private void VipBenefitsControlViewOnLeftArrowClicked()
        {
            if (_vipPopupViewModel.IsFirstPage) return;

            _vipPopupViewModel.CurrentPage.Value--;
        }

        private void VipBenefitsControlViewOnRightArrowClicked()
        {
            if (_vipPopupViewModel.IsLastPage) return;

            _vipPopupViewModel.CurrentPage.Value++;
        }
    }
}