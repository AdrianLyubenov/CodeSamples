using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingOfDestiny.Vip.Views
{
    public sealed class VipBenefitsControlView : MonoBehaviour
    {
        [SerializeField] private string titleFormat = "VIP {0} benefits";

        [SerializeField] private Button leftArrow = default;
        [SerializeField] private Button rightArrow = default;
        [SerializeField] private TMP_Text titleLabel = default;
        [SerializeField] private GameObject lockedIcon = default;

        public event Action LeftArrowClicked;
        public event Action RightArrowClicked;

        public void SetData(string vipLevelId, bool isFirstPage, bool isLastPage, bool isLocked)
        {
            titleLabel.text = string.Format(titleFormat, vipLevelId);
            leftArrow.gameObject.SetActive(!isFirstPage);
            rightArrow.gameObject.SetActive(!isLastPage);
            lockedIcon.gameObject.SetActive(isLocked);
        }

        public void Subscribe()
        {
            leftArrow.onClick.AddListener(OnLeftButtonClicked);
            rightArrow.onClick.AddListener(OnRightButtonClicked);
        }

        public void Unsubscribe()
        {
            leftArrow.onClick.RemoveListener(OnLeftButtonClicked);
            rightArrow.onClick.RemoveListener(OnRightButtonClicked);
        }

        private void OnLeftButtonClicked() => LeftArrowClicked?.Invoke();

        private void OnRightButtonClicked() => RightArrowClicked?.Invoke();
    }
}