using System.Collections.Generic;
using System.Linq;
using KingOfDestiny.Configurations;
using KingOfDestiny.Vip.Data;
using KingOfDestiny.Vip.Views;
using KingOfDestinyUtilities.Utils.Collections;
using UnityEngine;

namespace KingOfDestiny.Vip.Presenters
{
    public sealed class VipLevelUpPresenter
    {
        private readonly VipLevelUpView _vipLevelUpView;
        private readonly IVipManager _vipManager;
        private readonly RewardsController _rewardsController;
        private readonly VipIconsProvider _vipIconsProvider;
        private readonly GameData _gameData;
        
        public VipLevelUpPresenter(IVipManager vipManager, VipLevelUpView vipLevelUpView, RewardsController rewardsController,
            VipIconsProvider vipIconsProvider, GameData gameData)
        {
            _vipManager = vipManager;
            _vipLevelUpView = vipLevelUpView;
            _rewardsController = rewardsController;
            _vipIconsProvider = vipIconsProvider;
            _gameData = gameData;
        }
        
        public void Subscribe()
        {
            _vipManager.VipLeveledUp += OnLeveledUp;
        }
        
        public void Unsubscribe()
        {
            _vipManager.VipLeveledUp -= OnLeveledUp;
        }

        private void OnLeveledUp(VipData vipData)
        {
            Sprite bannerIcon =  _vipIconsProvider.GetIcon(vipData.VipLevelIndex);
            _vipManager.TryGetLevelConfiguration(vipData.VipLevelIndex, out VipLevelConfiguration levelConfiguration);
            
            _vipLevelUpView.Show(vipData.VipLevelIndex, bannerIcon, levelConfiguration, OnLevelUpPopupClosed);

            EventsManager.OnAnalyticsEvent.OnNext(new VipStatusAE(vipData));
        }

        private async void OnLevelUpPopupClosed() //TODO: remove after proper popup queue implementation
        {
            if (_vipManager.TryGetVipRewards(out IReadOnlyList<CurrencyRewardData> vipReward))
            {
                await _vipManager.ClaimVipBenefit();
                
                await _rewardsController.PlayRewardAnimation(IEnumerableUtils.Select(vipReward, x => 
                    new CurrencyReward(x.Code, x.Value)).ToList(), 0, false, 800f, false);
                
                _gameData.UpdateCurrencies(vipReward);
            }
        }
    }
}