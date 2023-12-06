using System;
using DG.Tweening;
using KingOfDestiny.Configurations;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KingOfDestiny.Vip.Views
{
    [Serializable]
    public class AnimationTiming
    {
        [field: SerializeField] public float Duration { get; private set; }
        [field: SerializeField] public float Delay { get; private set; }
    }

    public sealed class VipLevelUpView : MonoBehaviour
    {
        [SerializeField] private AnimationTiming titleAnimationTiming = default;
        [SerializeField] private AnimationTiming burstParticlesTiming = default;
        [SerializeField] private AnimationTiming bannerAnimationTiming = default;
        [SerializeField] private AnimationTiming bannerParticlesTiming = default;
        [SerializeField] private AnimationTiming levelLabelAnimationTiming = default;
        [SerializeField] private AnimationTiming messageTitleLabelAnimationTiming = default;
        [SerializeField] private AnimationTiming messageDescriptionLabelAnimationTiming = default;
        [SerializeField] private AnimationTiming benefitsAnimationTiming = default;
        [SerializeField] private AnimationTiming confettiParticlesTiming = default;
        [SerializeField] private AnimationTiming buttonAnimationTiming = default;
        
        [SerializeField] private float fadeInDuration = .6f;
        [SerializeField] private Ease mainAnimationEase = Ease.Flash;
        [Space] 
        [SerializeField] private CanvasGroup content;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Transform bannerContainer;
        [SerializeField] private Image bannerIcon = default;
        [SerializeField] private TMP_Text levelLabel = default;
        [SerializeField] private TMP_Text messageTitleLabel = default;
        [SerializeField] private TMP_Text messageDescriptionLabel = default;
        [SerializeField] private GameObject confettiParticles = default;
        [SerializeField] private GameObject shineParticles = default;
        [SerializeField] private GameObject bannerParticles = default;
        [SerializeField] private VipBenefitsContainerView vipBenefitsContainerView = default;
        [SerializeField] private Transform titleView = default;
        [SerializeField] private Button claimButton = default;
        [SerializeField] private CanvasGroup benefits = default;

        private Action _onCompleted;

        private Sequence _bannerAnimation;

        private readonly int scrollCap = 5;

        private void Awake()
        {
            confettiParticles.SetActive(false);
            shineParticles.SetActive(false);
            bannerParticles.SetActive(false);

            Hide();
            
            claimButton.onClick.AddListener(OnClaimButtonClicked);
        }

        private void OnDestroy()
        {
            claimButton.onClick.RemoveListener(OnClaimButtonClicked);
        }

        public void Show(int level, Sprite banner, VipLevelConfiguration vipLevelConfiguration, Action onCompleted)
        {
            bannerIcon.sprite = banner;
            levelLabel.text = level.ToString();
            vipBenefitsContainerView.UpdateView(vipLevelConfiguration, false);
            scrollRect.vertical = vipLevelConfiguration.VipBenefitsByKind.Count > scrollCap;
            
            _onCompleted = onCompleted;
            Animate();
        }

        private void OnClaimButtonClicked()
        {
            content.DOFade(0f, fadeInDuration).OnComplete(() =>
            {
                _onCompleted?.Invoke();
                Hide();
            });
        }

        [Button]
        public void TestAnimation()
        {
            Hide();
            Animate();
        }

        private void Animate()
        {
            content.alpha = 1f;
            content.blocksRaycasts = true;
            
            bannerContainer.localScale = Vector3.zero;
            titleView.localScale = Vector3.zero;
            levelLabel.transform.localScale = Vector3.zero;
            messageTitleLabel.transform.localScale = Vector3.zero;
            messageDescriptionLabel.transform.localScale = Vector3.zero;
            benefits.alpha = 0f;
            claimButton.transform.localScale = Vector3.zero;

            _bannerAnimation = DOTween.Sequence();

            _bannerAnimation.Append(titleView.DOScale(Vector3.one, titleAnimationTiming.Duration)
                .SetEase(mainAnimationEase).SetDelay(titleAnimationTiming.Delay));
            
            _bannerAnimation.AppendCallback(() => shineParticles.SetActive(true)).SetDelay(burstParticlesTiming.Delay);
            
            _bannerAnimation.Append(bannerContainer.DOScale(Vector3.one, bannerAnimationTiming.Duration)
                .SetEase(mainAnimationEase).SetDelay(bannerAnimationTiming.Delay));
            
            _bannerAnimation.AppendCallback(() => bannerParticles.SetActive(true)).SetDelay(bannerParticlesTiming.Delay);
            
            _bannerAnimation.Append(levelLabel.transform.DOScale(Vector3.one, levelLabelAnimationTiming.Duration)
                .SetEase(mainAnimationEase).SetDelay(levelLabelAnimationTiming.Delay));
            
            _bannerAnimation.Append(messageTitleLabel.transform.DOScale(Vector3.one, messageTitleLabelAnimationTiming.Duration)
                .SetEase(mainAnimationEase).SetDelay(messageTitleLabelAnimationTiming.Delay));
            
            _bannerAnimation.Append(messageDescriptionLabel.transform.DOScale(Vector3.one, messageDescriptionLabelAnimationTiming.Duration)
                .SetEase(mainAnimationEase).SetDelay(messageDescriptionLabelAnimationTiming.Delay));
            
            _bannerAnimation.Append(benefits.DOFade(1f, benefitsAnimationTiming.Duration)
                .SetEase(mainAnimationEase).SetDelay(benefitsAnimationTiming.Delay));
            
            _bannerAnimation.AppendCallback(() => confettiParticles.SetActive(true)).SetDelay(confettiParticlesTiming.Delay);
            
            _bannerAnimation.Append(claimButton.transform.DOScale(Vector3.one, buttonAnimationTiming.Duration)
                .SetEase(mainAnimationEase)).SetDelay(buttonAnimationTiming.Delay);
        }

        private void Hide()
        {
            confettiParticles.SetActive(false);
            shineParticles.SetActive(false);
            bannerParticles.SetActive(false);
            content.alpha = 0f;
            content.blocksRaycasts = false;
        }
    }
}