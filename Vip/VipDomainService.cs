using System;
using System.Collections.Generic;
using KingOfDestiny.Common.Data;
using KingOfDestiny.Configurations;
using KingOfDestiny.Vip.Data;
using KingOfDestiny.Vip.DataProviders;
using KODTransportModel.DTO;

namespace KingOfDestiny.Vip
{
    public sealed class VipDomainService : IDisposable
    {
        public event Action<VipData> DataUpdated;
        
        private readonly DataProvider<VipData> _vipDataProvider;
        private readonly DataProvider<VipConfiguration> _vipConfigurationProvider;
        private readonly DataProvider<VipBenefitsConfigurations> _vipBenefitsConfigurationsProvider;
        private readonly TitleData _titleData;
        private readonly IDataConverter<VipConfigurationDto, VipConfiguration> _vipConfigurationConverter;
        private readonly IDataConverter<VipDto, VipData> _vipDataConverter;
        private readonly IDataConverter<VipBenefitsConfigurationsDto, VipBenefitsConfigurations> _vipBenefitsConverter;
        private readonly ReadOnlyData _readOnlyData;
        private readonly IVipRewardsProvider _vipRewardsProvider;

        public VipConfiguration VipConfiguration => _vipConfigurationProvider.Data;
        public VipData VipData => _vipDataProvider.Data;
        public IReadOnlyList<CurrencyRewardData> VipReward => _vipRewardsProvider.Data;

        public VipDomainService(DataProvider<VipData> vipDataProvider,
            DataProvider<VipConfiguration> vipConfigurationProvider,
            DataProvider<VipBenefitsConfigurations> vipBenefitsConfigurationsProvider,
            TitleData titleData,
            IDataConverter<VipConfigurationDto, VipConfiguration> vipConfigurationConverter,
            IDataConverter<VipDto, VipData> vipDataConverter,
            IDataConverter<VipBenefitsConfigurationsDto, VipBenefitsConfigurations> vipBenefitsConverter,
            ReadOnlyData readOnlyData,
            IVipRewardsProvider vipRewardsProvider)
        {
            _vipDataProvider = vipDataProvider;
            _vipConfigurationProvider = vipConfigurationProvider;
            _vipBenefitsConfigurationsProvider = vipBenefitsConfigurationsProvider;
            _titleData = titleData;
            _vipConfigurationConverter = vipConfigurationConverter;
            _vipDataConverter = vipDataConverter;
            _vipBenefitsConverter = vipBenefitsConverter;
            _readOnlyData = readOnlyData;
            _vipRewardsProvider = vipRewardsProvider;
            
            Initialize();
        }

        void Initialize()
        {
            _titleData.DataPopulated += OnTitleDataPopulated;
            _vipDataProvider.DataUpdated += OnVipDataUpdated;
            _readOnlyData.DataPopulated += OnReadOnlyDataPopulated;
        }

        void IDisposable.Dispose()
        {
            _titleData.DataPopulated -= OnTitleDataPopulated;
            _vipDataProvider.DataUpdated -= OnVipDataUpdated;
            _readOnlyData.DataPopulated -= OnReadOnlyDataPopulated;
        }
        
        private void OnTitleDataPopulated()
        {
            VipBenefitsConfigurations convertedVipBenefitsConfiguration =
                _vipBenefitsConverter.Convert(_titleData.VipBenefitsConfigurationsDto);
            
            _vipBenefitsConfigurationsProvider.SetData(convertedVipBenefitsConfiguration);
            
            VipConfiguration convertedVipConfiguration = _vipConfigurationConverter.Convert(_titleData.VipConfigurationDto);
            
            _vipConfigurationProvider.SetData(convertedVipConfiguration);
        }
        
        private void OnVipDataUpdated(VipData vipData)
        {
            DataUpdated?.Invoke(vipData);
        }
        
        private void OnReadOnlyDataPopulated()
        {
            _vipDataProvider.SetData(_vipDataConverter.Convert(_readOnlyData.VipDto));
        }
    }
}