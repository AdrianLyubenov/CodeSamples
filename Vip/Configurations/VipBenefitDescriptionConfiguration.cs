using System;
using System.Collections.Generic;
using KingOfDestiny.Vip;

namespace KingOfDestiny.Configurations
{
    [Serializable]
    public sealed class VipBenefitDescriptionConfiguration
    {
        public readonly VipBenefitKind BenefitKind;
        public readonly string Description;

        public VipBenefitDescriptionConfiguration(VipBenefitKind benefitKind, string description)
        {
            BenefitKind = benefitKind;
            Description = description;
        }

        public VipBenefitDescriptionConfiguration()
        {
            
        }
    }

    [Serializable]
    public sealed class VipBenefitsConfigurations
    {
        public readonly IReadOnlyDictionary<VipBenefitKind, VipBenefitDescriptionConfiguration> BenefitsDescriptions;

        public VipBenefitsConfigurations(IReadOnlyDictionary<VipBenefitKind, VipBenefitDescriptionConfiguration> benefitsDescriptions)
        {
            BenefitsDescriptions = benefitsDescriptions;
        }

        public VipBenefitsConfigurations()
        {
            
        }
    }
}