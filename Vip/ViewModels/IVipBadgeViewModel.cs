using UniRx;
using UnityEngine;

namespace KingOfDestiny.Vip.ViewModels
{
    public interface IVipBadgeViewModel
    {
        IReadOnlyReactiveProperty<int> Level { get; }
        IReadOnlyReactiveProperty<Sprite> Icon { get; }
    }
}