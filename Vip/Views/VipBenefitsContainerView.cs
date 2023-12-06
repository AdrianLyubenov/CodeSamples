using System.Collections.Generic;
using System.Text;
using KingOfDestiny.Configurations;
using KingOfDestiny.Vip.Data;
using KingOfDestinyUtilities;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace KingOfDestiny.Vip.Views
{
    public sealed class VipBenefitsContainerView : MonoBehaviour
    {
        [SerializeField] private int scrollCap = 5;
        [Space] 
        [SerializeField] private ScrollRect scrollRectComponent = default;
        [SerializeField] private VipBenefitsCardView vipBenefitsCardPrefab = default;
        [SerializeField] private List<VipBenefitsCardView> vipBenefitsCards = default;
        [SerializeField] private Transform content = default;

        private readonly Dictionary<string, string> _currencyLocalizationsByCode = new Dictionary<string, string>()
        {
            {"CO", "Coins"},
            {"FV", "Favors"},
            {"TK", "Tokens"},
            {"DE", "Deals"}
        };
        
        public void UpdateView(VipLevelConfiguration vipLevelConfiguration, bool isLocked)
        {
            if (vipLevelConfiguration.VipBenefitsByKind == null || vipLevelConfiguration.VipBenefitsByKind.Count == 0)
            {
                foreach (var view in vipBenefitsCards)
                {
                    view.gameObject.SetActive(false);
                }
                
                return;
            }

            IReadOnlyList<VipBenefitConfiguration> benefits = vipLevelConfiguration.VipBenefits;
            
            for (int i = 0; i < benefits.Count; i++)
            {
                VipBenefitsCardView view;
                
                if (i < vipBenefitsCards.Count)
                {
                    view = vipBenefitsCards[i];
                }
                else
                {
                    view = Instantiate(vipBenefitsCardPrefab, content).GetComponent<VipBenefitsCardView>();
                    
                    vipBenefitsCards.Add(view);
                }
                
                view.gameObject.SetActive(true);

                VipBenefitConfiguration data = benefits[i];
                
                view.UpdateView(GetFormattedDescription(data), data.IsNew, isLocked);
            }

            for (int i = vipBenefitsCards.Count - 1; i >= vipLevelConfiguration.VipBenefitsByKind.Count; i--)
            {
                vipBenefitsCards[i].gameObject.SetActive(false);
            }

            scrollRectComponent.vertical = vipLevelConfiguration.VipBenefits.Count > scrollCap;
        }

        private string GetFormattedDescription(VipBenefitConfiguration vipBenefitConfiguration)
        {
            var formattedValue = new StringBuilder();
            
            switch (vipBenefitConfiguration.BenefitData)
            {
                case VipBenefitNumericData numericData:
                {
                    formattedValue.Append(numericData.Value.ToString("N0"));
                    break;
                }
                
                case VipBenefitCityStarsMultiplierData vipBenefitCityStarsMultiplierData:
                {
                    foreach (var item in vipBenefitCityStarsMultiplierData.Value)
                    {
                        formattedValue.Append($"{item.Level},");
                    }
                
                    formattedValue.Remove(formattedValue.Length - 1, 1);
                    break;
                }
                
                case VipBenefitIdData vipBenefitIdData:
                {
                    formattedValue.Append(vipBenefitIdData.Value);
                    break;
                }
                
                case VipBenefitCurrencyRewardsData vipBenefitCurrencyRewardsData:
                {
                    foreach (var reward in vipBenefitCurrencyRewardsData.Value)
                    {
                        formattedValue.Append($"{FormatRewardData(reward)},");
                    }
                
                    formattedValue.Remove(formattedValue.Length - 1, 1);
                    break;
                }
            }

            return string.Format(vipBenefitConfiguration.Description, $"<color=green>{formattedValue}</color>");
        }
        
        private string FormatRewardData(CurrencyRewardData currencyRewardDto)
        {
            return currencyRewardDto == null ? string.Empty : 
                $"{Utilities.FormatCurrencyNumber(currencyRewardDto.Value, false)} {_currencyLocalizationsByCode[currencyRewardDto.Code]}";
        }
    }
}