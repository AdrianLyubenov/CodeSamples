using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using Spine;
using System.Collections.Generic;
using DG.Tweening;

public class CurrentDayController : MonoBehaviour
{
    public Text dayText;
    public GameObject todayLabel;
    public DailyChallangesPopup dailyChallangesPopup;

    public GameObject crownInactive;
    public GameObject crownNormal;
    public GameObject crownToday;
    public Text crownText;

    public Text buttonText;
    public GameObject clickBlocker;
    public Button button;
    public List<SkeletonGraphic> crownAnimations;
    private List<TrackEntry> animationTrackEntries = new List<TrackEntry>();

    private int dayIdx;

    public void SetData(string dayDescription, bool isToday, int dayIdx, ChallengeWinType winType)
    {
        this.dayIdx = dayIdx;
        dayText.text = dayDescription;
        todayLabel.SetActive(isToday);

        switch(winType)
        {
            case ChallengeWinType.NOT_WON:
                crownInactive.SetActive(true);
                crownNormal.SetActive(false);
                crownToday.SetActive(false);
                crownText.text = I2.Loc.LocalizationManager.GetTermTranslation("DAILY_NOT_SOLVED");
                buttonText.text = I2.Loc.LocalizationManager.GetTermTranslation("DAILY_PLAY");
                break;
            case ChallengeWinType.WON:
                crownInactive.SetActive(false);
                crownNormal.SetActive(true);
                crownToday.SetActive(false);
                crownText.text = I2.Loc.LocalizationManager.GetTermTranslation("DAILY_SOLVED");
                buttonText.text = I2.Loc.LocalizationManager.GetTermTranslation("DAILY_REDEAL");
                break;
            case ChallengeWinType.WON_TODAY:
                crownInactive.SetActive(false);
                crownNormal.SetActive(false);
                crownToday.SetActive(true);
                crownText.text = I2.Loc.LocalizationManager.GetTermTranslation("DAILY_SOLVED_ON_DAY");
                buttonText.text = I2.Loc.LocalizationManager.GetTermTranslation("DAILY_REDEAL");
                break;
        }
    }

    public void StartChallenge()
    {
        dailyChallangesPopup.StartDailyChallenge(dayIdx);
    }

    public Image AnimateCrown(ChallengeWinType winType)
    {
        crownInactive.SetActive(true);
        Image crownImg = ((winType == ChallengeWinType.WON_TODAY) ? crownToday : crownNormal).GetComponent<Image>();
        crownImg.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        clickBlocker.SetActive(true);
        button.enabled = false;
        crownImg.DOFade(1.0f, 2.0f).OnComplete(() => {
            clickBlocker.SetActive(false);
            button.enabled = true;
        });
        for (int i = 0; i < crownAnimations.Count; i++)
        {
            crownAnimations[i].gameObject.SetActive(true);
            crownAnimations[i].AnimationState.ClearTracks();
            TrackEntry track = crownAnimations[i].AnimationState.AddAnimation(0, "animation", false, 0f);
            track.Complete += OnAnimComplete;
            animationTrackEntries.Add(track);
        }
        return crownImg;
    }

    private void OnAnimComplete(TrackEntry trackEntry)
    {
        trackEntry.Complete -= OnAnimComplete;
        animationTrackEntries.Remove(trackEntry);
        if (animationTrackEntries.Count == 0)
        {
            for (int i = 0; i < crownAnimations.Count; i++)
            {
                crownAnimations[i].gameObject.SetActive(false);
            }
            crownInactive.SetActive(false);
        }
    }
}
