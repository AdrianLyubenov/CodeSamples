using KingOfDestiny.Configurations;
using UniRx;
using UnityEngine;

namespace KingOfDestiny.Vip.ViewModels
{
    public sealed class VipPopupViewModel : IVipPopupViewModel
    {
        private readonly ReactiveProperty<int> _currentLevel = new ReactiveProperty<int>();
        private readonly ReactiveProperty<Sprite> _badgeIcon = new ReactiveProperty<Sprite>(); 
        private readonly ReactiveProperty<int> _currentPoints = new ReactiveProperty<int>();
        private readonly ReactiveProperty<VipLevelConfiguration> _nextLevelConfiguration =
            new ReactiveProperty<VipLevelConfiguration>();
        private readonly ReactiveProperty<int> _pointsNeededForUpgrade = new ReactiveProperty<int>();
        private readonly ReactiveProperty<VipLevelConfiguration> _benefitsLevelConfiguration =
            new ReactiveProperty<VipLevelConfiguration>();
        private readonly ReactiveProperty<bool> _isMaxLevelReached = new ReactiveProperty<bool>();

        public VipPopupViewModel(int level,
            Sprite badgeIcon, 
            int currentPoints, 
            int pointsNeededForUpgrade, 
            VipLevelConfiguration nextLevelConfiguration,
            VipLevelConfiguration currentLevelConfiguration,
            bool isMaxLevelReached)
        {
            _currentLevel.Value = level;
            _badgeIcon.Value = badgeIcon;
            _currentPoints.Value = currentPoints;
            _pointsNeededForUpgrade.Value = pointsNeededForUpgrade;
            _nextLevelConfiguration.Value = nextLevelConfiguration;
            _benefitsLevelConfiguration.Value = currentLevelConfiguration;
            CurrentPage.Value = level == 0 ? 1 : level;
            _isMaxLevelReached.Value = isMaxLevelReached;
        }
        
        public IReadOnlyReactiveProperty<int> CurrentLevel => _currentLevel;
        public IReadOnlyReactiveProperty<Sprite> BadgeIcon => _badgeIcon;
        public IReadOnlyReactiveProperty<int> CurrentPoints => _currentPoints;
        public IReadOnlyReactiveProperty<VipLevelConfiguration> NextLevelConfiguration => _nextLevelConfiguration;

        public IReadOnlyReactiveProperty<VipLevelConfiguration> BenefitsLevelConfiguration =>
            _benefitsLevelConfiguration;
        public IReadOnlyReactiveProperty<int> PointsNeededForUpgrade => _pointsNeededForUpgrade;
        public IReadOnlyReactiveProperty<bool> IsMaxLevelReached => _isMaxLevelReached;
        public ReactiveProperty<int> CurrentPage { get; } = new ReactiveProperty<int>();
        public bool IsFirstPage { get; set; }
        public bool IsLastPage { get; set; }

        public void SetLevelConfigurationToDisplay(VipLevelConfiguration vipLevelConfiguration)
        {
            _benefitsLevelConfiguration.Value = vipLevelConfiguration;
        }
    }
}