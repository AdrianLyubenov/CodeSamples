using KingOfDestiny.Vip.ViewModels;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace KingOfDestiny.Vip.Views
{
    public sealed class VipHudBannerView : MonoBehaviour
    {
        [SerializeField] private string vipLevelFormat = default;
        [Space]
        [SerializeField] private Image badgeIcon = default;
        [SerializeField] private TMP_Text levelText = default;
        [SerializeField] private TMP_Text lockedText = default;

        private IVipBadgeViewModel _viewModel;
        
        public void SetModel(IVipBadgeViewModel viewModel)
        {
            _viewModel = viewModel;

            UpdateView();
            
            _viewModel.Icon.Subscribe((_) => UpdateView()).AddTo(this);
        }

        private void UpdateView()
        {
            badgeIcon.sprite = _viewModel.Icon.Value;
            
            int level = _viewModel.Level.Value;
            
            levelText.gameObject.SetActive(level > 0);
            lockedText.gameObject.SetActive(level <= 0);

            levelText.text = string.Format(vipLevelFormat, level);
        }
    }
}