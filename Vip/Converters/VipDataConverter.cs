using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using KingOfDestiny.Common.Data;
using KingOfDestiny.Configurations;
using KingOfDestiny.Vip.Data;
using KingOfDestinyUtilities.Utils.Collections;
using KODTransportModel.DTO;
using Newtonsoft.Json;

namespace KingOfDestiny.Vip.Converter
{
    public sealed class VipDataConverter : IDataConverter<VipDto, VipData>,
        IDataConverter<VipConfigurationDto, VipConfiguration>
    {
        private const string DEFAULT_BENEFIT_DESCRIPTION_FORMAT = "Description of ability with value {0}.";
        
        private readonly IReadonlyDataProvider<VipBenefitsConfigurations> _vipBenefitsConfigurationsProvider;
        
        public VipDataConverter(IReadonlyDataProvider<VipBenefitsConfigurations> vipBenefitsConfigurationsProvider)
        {
            _vipBenefitsConfigurationsProvider = vipBenefitsConfigurationsProvider;
        }
        
        public VipData Convert(VipDto target)
        {
            if (target == null)
            {
                KLogger.LogError($"Vip dto is null.");

                return default;
            }

            var benefits = new List<VipBenefitData>();

            if (target.ClaimableBenefits != null)
            {
                foreach (VipBenefitDto benefitDto in target.ClaimableBenefits)
                {
                    TryParse(benefitDto.Type, out var benefitKind);
                    
                    benefits.Add(ParseBenefit(benefitKind, benefitDto.Value));
                }
            }

            return new VipData(target.VipLevel, target.VipPoints, benefits);
        }

        private VipBenefitData ParseBenefit(VipBenefitKind vipBenefitKind, object value)
        {
            try
            {
                switch (vipBenefitKind)
                {
                    case VipBenefitKind.avatar:
                    case VipBenefitKind.item:
                    {
                        return new VipBenefitIdData(vipBenefitKind, (string)value);
                    }
                    case VipBenefitKind.cityStarsMultipliers:
                    {
                        var multipliersDto =
                            JsonConvert.DeserializeObject<VipTournamentMultiplierLevelConfigurationDto[]>(
                                value.ToString());
                        
                        var cityStarsMultipliers = new List<CityStarsMultiplier>();

                        if (multipliersDto != null)
                        {
                            foreach (VipTournamentMultiplierLevelConfigurationDto multiplierLevelConfigurationDto in
                                multipliersDto)
                            {
                                float.TryParse(multiplierLevelConfigurationDto.Value,
                                    out var multiplierValue);
                                
                                int.TryParse(multiplierLevelConfigurationDto.Stars, out int starsValue);
                                
                                cityStarsMultipliers.Add(new CityStarsMultiplier(starsValue, multiplierValue));
                            }
                        }

                        return new VipBenefitCityStarsMultiplierData(vipBenefitKind, cityStarsMultipliers.ToArray());
                    }
                    case VipBenefitKind.oneTimeCurrency:
                    {
                        var rewardsDto = JsonConvert.DeserializeObject<CurrencyRewardDto[]>(value.ToString());

                        return new VipBenefitCurrencyRewardsData(vipBenefitKind,
                            IEnumerableUtils.Select(rewardsDto, x => new CurrencyRewardData(x.Code, x.Value))
                                .ToArray());
                    }

                    default:
                    {
                        int.TryParse(value.ToString(), out var numericValue);
                        
                        return new VipBenefitNumericData(vipBenefitKind, numericValue);
                    }
                }
            }
            catch (Exception e)
            {
                KLogger.LogError(e.Message);
            }
            
            return default;
        }
        
        public VipConfiguration Convert(VipConfigurationDto target)
        {
            var levels = new List<VipLevelConfiguration>();

            if (target?.VipLevels != null)
            {
                for (var index = 0; index < target.VipLevels.Length; index++)
                {
                    var levelConfigurationDto = target.VipLevels[index];

                    var previousLevel = index == 0 ? null : target.VipLevels[index - 1];

                    levels.Add(Convert(levelConfigurationDto, previousLevel));
                }
            }

            return new VipConfiguration(levels);
        }

        public VipLevelConfiguration Convert(VipLevelConfigurationDto target, VipLevelConfigurationDto previousDto)
        {
            var benefits = new Dictionary<VipBenefitKind, VipBenefitConfiguration>();

            foreach (var benefitDto in target.VipBenefits)
            {
                VipBenefitConfiguration benefitConfiguration = Convert(benefitDto, previousDto.VipBenefits);
                
                benefits.Add(benefitConfiguration.BenefitData.Kind, benefitConfiguration);
            }
            
            int.TryParse(target.PointsRequired, out int pointsRequired);

            var multiplierLevelsAffected = new List<int>();
            
            return new VipLevelConfiguration(target.Id, pointsRequired, benefits);
        }

        public VipBenefitConfiguration Convert(VipBenefitConfigurationDto target,
            IReadOnlyList<VipBenefitConfigurationDto> previousLevelBenefits)
        {
            bool isNew = previousLevelBenefits == null || 
                         IEnumerableUtils.All(previousLevelBenefits, x => x.BenefitType != target.BenefitType);

            string description = DEFAULT_BENEFIT_DESCRIPTION_FORMAT;

            if (!TryParse(target.BenefitType, out VipBenefitKind vipBenefitKind))
            {
                KLogger.LogError($"Wrong vip benefit type [{target.BenefitType}]");
            }

            if (_vipBenefitsConfigurationsProvider.Data.BenefitsDescriptions.TryGetValue(vipBenefitKind,
                out VipBenefitDescriptionConfiguration benefitDescriptionConfiguration))
            {
                description = benefitDescriptionConfiguration.Description;
            }

            return new VipBenefitConfiguration(ParseBenefit(vipBenefitKind, target.Value), isNew, description);
        }
        
        private bool TryParse(string benefitType, out VipBenefitKind result)
        {
            return Enum.TryParse(benefitType, out result);
        }
    }
}