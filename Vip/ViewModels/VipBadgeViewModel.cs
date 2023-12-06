using UniRx;
using UnityEngine;

namespace KingOfDestiny.Vip.ViewModels
{
    public sealed class VipBadgeViewModel : IVipBadgeViewModel
    {
        private readonly ReactiveProperty<int> _level = new ReactiveProperty<int>();
        private readonly ReactiveProperty<Sprite> _icon = new ReactiveProperty<Sprite>();

        public IReadOnlyReactiveProperty<int> Level => _level;
        public IReadOnlyReactiveProperty<Sprite> Icon => _icon;

        public void SetLevel(int level) => _level.Value = level;
        public void SetIcon(Sprite icon) => _icon.Value = icon;
    }
}