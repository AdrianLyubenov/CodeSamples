using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KODTransportModel.DTO;
using UnityEngine;

namespace KingOfDestiny.Vip
{
    public interface IVipIconsProvider
    {
        Sprite GetIcon(int level);
    }

    public sealed class VipIconsProvider : IVipIconsProvider
    {
        private const string ICON_ID_FORMAT = "Icon_Vip_Badge_{0}";
        
        private readonly Dictionary<int, Sprite> _iconsById = new Dictionary<int, Sprite>();

        private readonly AddressablesController _addressablesController;
        private readonly TitleData _titleData;
        
        public VipIconsProvider(AddressablesController addressablesController, TitleData titleData)
        {
            _addressablesController = addressablesController;
            _titleData = titleData;
        }

        public async Task CacheData(Action onCompleted)
        {
            if (_titleData.VipConfigurationDto?.VipLevels != null)
            {
                foreach (VipLevelConfigurationDto vipLevel in _titleData.VipConfigurationDto.VipLevels)
                {
                    var icon = await _addressablesController.LoadSpriteAsync(string.Format(ICON_ID_FORMAT,
                        vipLevel.Id));

                    if (int.TryParse(vipLevel.Id, out int levelIndex))
                    {
                        _iconsById.Add(levelIndex, icon);
                    }
                }
            }

            onCompleted?.Invoke();
        }

        public Sprite GetIcon(int level)
        {
            _iconsById.TryGetValue(level, out var result);

            return result;
        }
    }
}