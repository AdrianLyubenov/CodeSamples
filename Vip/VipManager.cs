using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KingOfDestiny.Configurations;
using KingOfDestiny.Transport;
using KingOfDestiny.Vip.Data;
using KingOfDestinyUtilities.Utils.Collections;

namespace KingOfDestiny.Vip
{
    public interface IVipManager
    {
        event Action<VipData> VipDataUpdated;
        event Action<VipData> VipLeveledUp;

        VipData GetVipData();
        void ShowUpdate();
        bool TryGetLevelConfiguration(int levelIndex, out VipLevelConfiguration vipLevelConfiguration);
        bool IsMaxLevelReached(int level);
        bool HasVipBenefitOfType(VipBenefitKind vipBenefitKind);
        bool IsTournamentMultiplierLevelIncreasedByVip(int level);
        bool TryGetCurrentBenefitOfType(VipBenefitKind vipBenefitKind, out VipBenefitConfiguration result);
        List<MultiplierEntry> GetUpdatedMultiplierValuesByBenefit(VipBenefitData vipBenefitData, float[] rawValues);
        Task ClaimVipBenefit();
        bool TryGetVipRewards(out IReadOnlyList<CurrencyRewardData> vipReward);
    }

    public sealed class VipManager : IInitializable, IDisposable, IVipManager
    {
        public event Action<VipData> VipDataUpdated;
        public event Action<VipData> VipLeveledUp;
        public event Action<VipData> VipPointsIncreased;

        private readonly VipDomainService _vipDomainService;
        private readonly ApiService _apiService;

        private VipData _vipData;
        private VipData _pendingVipData;
        
        public VipManager(VipDomainService vipDomainService, ApiService apiService)
        {
            _vipDomainService = vipDomainService;
            _apiService = apiService;
        }

        void IInitializable.Initialize(ServiceLocator _)
        {
            Subscribe();
        }
        
        void IDisposable.Dispose()
        {
            Unsubscribe();
        }
        
        public VipData GetVipData() => _vipDomainService.VipData;
        
        public bool TryGetLevelConfiguration(int levelIndex, out VipLevelConfiguration vipLevelConfiguration)
        {
            vipLevelConfiguration = default;

            if (levelIndex < 0 || IsMaxLevelReached(levelIndex - 1)) return false;
            
            vipLevelConfiguration = _vipDomainService.VipConfiguration.VipLevels[levelIndex];

            return true;
        }

        public bool IsMaxLevelReached(int level)
        {
            return level >= _vipDomainService.VipConfiguration.VipLevels.Count - 1;
        }

        public bool HasVipBenefitOfType(VipBenefitKind vipBenefitKind)
        {
            VipLevelConfiguration vipLevelDescription = _vipDomainService.VipConfiguration.VipLevels[_vipDomainService.VipData.VipLevelIndex];

            if (vipLevelDescription != null)
            {
                return vipLevelDescription.VipBenefitsByKind.Values.Any(x => x.BenefitType == vipBenefitKind);
            }

            return false;
        }

        public bool IsTournamentMultiplierLevelIncreasedByVip(int level)
        {
            VipLevelConfiguration currentConfiguration =
                _vipDomainService.VipConfiguration.VipLevels[_vipDomainService.VipData.VipLevelIndex];

            if (currentConfiguration.VipBenefits.Find(x => x.BenefitType == VipBenefitKind.cityStarsMultipliers,
                out VipBenefitConfiguration result))
            {
                if (result.BenefitData is VipBenefitCityStarsMultiplierData cityStarsMultiplierBenefit)
                {
                    return cityStarsMultiplierBenefit.Value.Any(x => x.Level == level);
                }
            }

            return false;
        }

        public bool TryGetCurrentBenefitOfType(VipBenefitKind vipBenefitKind, out VipBenefitConfiguration result)
        {
            VipLevelConfiguration vipLevelDescription = _vipDomainService.VipConfiguration.VipLevels[_vipDomainService.VipData.VipLevelIndex];

            result = default;
            
            if (vipLevelDescription != null)
            {
                return vipLevelDescription.VipBenefitsByKind.TryGetValue(vipBenefitKind, out result);
            }

            return false;
        }
        
        public List<MultiplierEntry> GetUpdatedMultiplierValuesByBenefit(VipBenefitData vipBenefitData, float[] rawValues)
        {
            var entries = new List<MultiplierEntry>();
            var vipBenefitCityStarsMultiplierData = vipBenefitData as VipBenefitCityStarsMultiplierData;

            for (var index = 0; index < rawValues.Length; index++)
            {
                float rawValue = rawValues[index];
                CityStarsMultiplier modifiedValue =
                    vipBenefitCityStarsMultiplierData?.Value?.FirstOrDefaultI(x => x.Level - 1 == index);

                entries.Add(modifiedValue != null
                    ? new MultiplierEntry(modifiedValue.Value, true)
                    : new MultiplierEntry(rawValue, false));
            }

            return entries;
        }

        public async Task ClaimVipBenefit()
        {
            await _apiService.ClaimVipRewards();
        }

        public bool TryGetVipRewards(out IReadOnlyList<CurrencyRewardData> vipReward)
        {
            vipReward = _vipDomainService.VipReward;

            return vipReward != null;
        }

        private void OnVipDataUpdated(VipData vipData)
        {
            _pendingVipData = vipData;
        }

        public void ShowUpdate()
        {
            if (_pendingVipData == null) return;
            if (_vipData != null)
            {
                if (_vipData.VipLevelIndex < _pendingVipData.VipLevelIndex)
                {
                    VipLeveledUp?.Invoke(_pendingVipData);
                }
                else if (_vipData.VipPoints < _pendingVipData.VipPoints)
                {
                    VipPointsIncreased?.Invoke(_pendingVipData);
                }
            }

            _vipData = _pendingVipData;
            VipDataUpdated?.Invoke(_pendingVipData);
            _pendingVipData = null;
        }

        private void VipPointsIncrease(VipData vipData)
        {
            EventsManager.OnAnalyticsEvent.OnNext(new VipStatusAE(vipData));
        }
        
        private void Subscribe()
        {
            _vipDomainService.DataUpdated += OnVipDataUpdated;
            VipPointsIncreased += VipPointsIncrease;
        }

        private void Unsubscribe()
        {
            _vipDomainService.DataUpdated -= OnVipDataUpdated;
            VipPointsIncreased -= VipPointsIncrease;
        }
    }
}