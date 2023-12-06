using System.Collections.Generic;
using System.Linq;
using KingOfDestiny.Vip;
using KingOfDestiny.Vip.Data;

namespace KingOfDestiny.Configurations
{
    public class VipBenefitConfiguration
    {
        public readonly VipBenefitData BenefitData;
        public readonly bool IsNew;
        public readonly string Description;

        public VipBenefitConfiguration(VipBenefitData benefitData, bool isNew, string description)
        {
            BenefitData = benefitData;
            IsNew = isNew;
            Description = description;
        }

        public VipBenefitKind BenefitType => BenefitData?.Kind ?? VipBenefitKind.None;
    }

    public sealed class VipLevelConfiguration
    {
        public readonly string Id; //why is it a string...
        public readonly int PointsRequired;
        public readonly IReadOnlyDictionary<VipBenefitKind, VipBenefitConfiguration> VipBenefitsByKind;

        public IReadOnlyList<VipBenefitConfiguration> VipBenefits => VipBenefitsByKind.Values.ToList();

        public VipLevelConfiguration(string id, int pointsRequired, IReadOnlyDictionary<VipBenefitKind, VipBenefitConfiguration> vipBenefits)
        {
            Id = id;
            PointsRequired = pointsRequired;
            VipBenefitsByKind = vipBenefits;
        }
    }
    
    public sealed class VipConfiguration
    {
        public readonly IReadOnlyList<VipLevelConfiguration> VipLevels;

        public VipConfiguration(IReadOnlyList<VipLevelConfiguration> vipLevels)
        {
            VipLevels = vipLevels;
        }
    }
}