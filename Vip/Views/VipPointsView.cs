using KingOfDestinyUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingOfDestiny.Vip.Views
{
    public sealed class VipPointsView : MonoBehaviour
    {
        private const float MAX_LEVEL_VALUE = 1f;
        
        [SerializeField] private string pointsFormat = "{0}/{1}";
        [SerializeField] private string acquirePointsFormat = "Acquire {0} VIP points to reach {1} VIP";
        [SerializeField] private string maxLevelReachedText = "Max VIP level reached!";

        [SerializeField] private TMP_Text nextLevelInfoLabel = default;
        [SerializeField] private TMP_Text pointsLabel = default;
        [SerializeField] private Slider pointsProgressSlider = default; 
        
        public void SetData(int points, int pointsCap, int pointsLeft, string nextLevelId, bool isMaxLevelReached)
        {
            pointsLabel.gameObject.SetActive(!isMaxLevelReached);
            
            if (isMaxLevelReached)
            {
                nextLevelInfoLabel.text = maxLevelReachedText;
                pointsProgressSlider.value = MAX_LEVEL_VALUE;
            }
            else
            {
                pointsLabel.text = string.Format(pointsFormat, Utilities.FormatNumber(points), Utilities.FormatNumber(pointsCap));
                nextLevelInfoLabel.text = string.Format(acquirePointsFormat, Utilities.FormatNumber(pointsLeft), nextLevelId);
                pointsProgressSlider.value = (float)points / pointsCap;
            }
        }

    }
}