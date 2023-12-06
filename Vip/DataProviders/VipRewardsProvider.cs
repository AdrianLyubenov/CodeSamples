using System.Collections.Generic;
using KingOfDestiny.Common.Data;
using KingOfDestiny.Vip.Data;

namespace KingOfDestiny.Vip.DataProviders
{
    public interface IVipRewardsProvider : IReadonlyDataProvider<IReadOnlyList<CurrencyRewardData>>
    {

    }

    public class VipRewardsProvider : DataProvider<IReadOnlyList<CurrencyRewardData>>, IVipRewardsProvider
    {
        
    }
}