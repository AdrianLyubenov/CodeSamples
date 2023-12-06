using System.Collections.Generic;
using KingOfDestiny.Configurations;

namespace KingOfDestiny.Vip.Data
{
    public class CityStarsMultiplier
    {
        public readonly int Level;
        public readonly float Value;

        public CityStarsMultiplier(int level, float value)
        {
            Level = level;
            Value = value;
        }
    }

    public class VipBenefitData
    {
        public readonly VipBenefitKind Kind;

        public VipBenefitData(VipBenefitKind kind)
        {
            Kind = kind;
        }
    }

    public class VipBenefitData<T> : VipBenefitData
    {
        public readonly T Value;

        public VipBenefitData(VipBenefitKind kind, T value) : base(kind)
        {
            Value = value;
        }
    }

    public class VipBenefitIdData : VipBenefitData<string>
    {
        public VipBenefitIdData(VipBenefitKind kind, string value) : base(kind, value)
        {
        }

        public string FormattedValue => Value;
    }
    
    public class VipBenefitNumericData : VipBenefitData<int>
    {
        public VipBenefitNumericData(VipBenefitKind kind, int value) : base(kind, value)
        {
        }
    }

    public class VipBenefitCurrencyRewardsData : VipBenefitData<CurrencyRewardData[]>
    {
        public VipBenefitCurrencyRewardsData(VipBenefitKind kind, CurrencyRewardData[] value) : base(kind, value)
        {
        }
    }

    public class VipBenefitCityStarsMultiplierData : VipBenefitData<CityStarsMultiplier[]>
    {
        public VipBenefitCityStarsMultiplierData(VipBenefitKind kind, CityStarsMultiplier[] value) : base(kind, value)
        {
        }
    }

    public sealed class VipData
    {
        public readonly int VipLevelIndex;
        public readonly int VipPoints;
        public readonly IReadOnlyList<VipBenefitData> Benefits;

        public VipData(int vipLevelIndex, int vipPoints, IReadOnlyList<VipBenefitData> benefits)
        {
            VipLevelIndex = vipLevelIndex;
            VipPoints = vipPoints;
            Benefits = benefits;
        }
    }
    
    public class MultiplierEntry
    {
        public readonly float Value;
        public readonly bool IsModifiedByVip;

        public MultiplierEntry(float value, bool isModifiedByVip)
        {
            Value = value;
            IsModifiedByVip = isModifiedByVip;
        }
    }
}