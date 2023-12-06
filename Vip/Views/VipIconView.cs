using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingOfDestiny.Vip.Views
{
    public sealed class VipIconView : MonoBehaviour
    {
        private const string LOCKED_LEVEL_LABEL = "1";
        
        [SerializeField] private GameObject _lock = default;
        [SerializeField] private Image _bannerIcon = default;
        [SerializeField] private TMP_Text _levelLabel = default;

        public void Set(int level, Sprite banner)
        {
            _lock.gameObject.SetActive(level == 0);
            _levelLabel.text = level == 0 ? LOCKED_LEVEL_LABEL : level.ToString();
            _bannerIcon.sprite = banner;
        }
    }
}