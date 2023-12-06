using KingOfDestiny.Vip.Presenters;
using KingOfDestiny.Vip.Views;
using UnityEngine;

namespace KingOfDestiny.Vip.Installers
{
    public sealed class VipPopupInstaller : MonoBehaviour
    {
        [SerializeField] private VipPopup popup = default;
        
        private VipPopupPresenter _vipPopupPresenter;
        
        public IVipPopupPresenter VipPopupPresenter => _vipPopupPresenter;

        private void Awake()
        {
            InitializeDependencies();
        }

        private void InitializeDependencies()
        {
            var vipManager = ServiceLocator.Instance.GetInstanceOfType<IVipManager>();
            var vipIconsProvider = ServiceLocator.Instance.GetInstanceOfType<VipIconsProvider>();

            _vipPopupPresenter = new VipPopupPresenter(vipManager, popup, vipIconsProvider);
        }
    }
}