using System;
using System.Collections.Generic;
using KingOfDestiny.Common.Data;
using KingOfDestiny.Configurations;
using KODTransportModel.DTO;

namespace KingOfDestiny.Vip.Converter
{
    public sealed class VipBenefitDescriptionConverter : IDataConverter<VipBenefitsConfigurationsDto, VipBenefitsConfigurations>
    {
        public VipBenefitsConfigurations Convert(VipBenefitsConfigurationsDto target)
        {
            Dictionary<VipBenefitKind, VipBenefitDescriptionConfiguration> descriptions =
                new Dictionary<VipBenefitKind, VipBenefitDescriptionConfiguration>();

            if (target?.BenefitsDescriptions != null)
            {
                foreach (var descriptionDto in target.BenefitsDescriptions)
                {
                    if (!TryParse(descriptionDto.BenefitType, out VipBenefitKind vipBenefitKind))
                    {
                        KLogger.LogError($"Wrong vip benefit type");
                        continue;
                    }

                    descriptions.Add(vipBenefitKind,
                        new VipBenefitDescriptionConfiguration(vipBenefitKind, descriptionDto.Description));
                }
            }

            return new VipBenefitsConfigurations(descriptions);
        }

        private bool TryParse(string benefitType, out VipBenefitKind result)
        {
            return Enum.TryParse(benefitType, out result);
        }
    }
}