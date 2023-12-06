using KingOfDestiny.Configurations;
using UniRx;
using UnityEngine;

namespace KingOfDestiny.Vip.ViewModels
{
    public interface IVipPopupViewModel
    {
        IReadOnlyReactiveProperty<int> CurrentLevel { get; }
        IReadOnlyReactiveProperty<Sprite> BadgeIcon { get; }

        IReadOnlyReactiveProperty<int> CurrentPoints { get; }
        IReadOnlyReactiveProperty<VipLevelConfiguration> NextLevelConfiguration { get; }
        IReadOnlyReactiveProperty<VipLevelConfiguration> BenefitsLevelConfiguration { get; }
        IReadOnlyReactiveProperty<int> PointsNeededForUpgrade { get; }
        IReadOnlyReactiveProperty<bool> IsMaxLevelReached { get; }
        ReactiveProperty<int> CurrentPage { get; }
        bool IsFirstPage { get; }
        bool IsLastPage { get; }
    }
}