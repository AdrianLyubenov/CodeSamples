using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ProgressBarController : MonoBehaviour
{
    private const int BRONZE_MEDAL_REQUIREMENT = 10;
    private const int SILVER_MEDAL_REQUIREMENT = 20;

    public Text progressText;
    public Text goldCupReqText;
    public Image progressImage;
    public GameObject bronzeCupMarker;
    public GameObject silverCupMarker;
    public GameObject goldCupMarker;

    private bool progressBarSet = false;

    public void SetMonthData(int daysInMonth, int challengesWon)
    {
        progressText.text = challengesWon.ToString() + "/" + daysInMonth.ToString();
        goldCupReqText.text = daysInMonth.ToString();

        bronzeCupMarker.SetActive(challengesWon >= BRONZE_MEDAL_REQUIREMENT);
        silverCupMarker.SetActive(challengesWon >= SILVER_MEDAL_REQUIREMENT);
        goldCupMarker.SetActive(challengesWon == daysInMonth);

        var rect = progressImage.rectTransform.rect;
        var xMin = progressImage.transform.localPosition.x + rect.xMin;
        float xBarEnd = rect.xMin;
        float bronzeX = bronzeCupMarker.transform.localPosition.x;
        float silverX = silverCupMarker.transform.localPosition.x;
        float goldX = goldCupMarker.transform.localPosition.x;
        if (challengesWon <= BRONZE_MEDAL_REQUIREMENT)
        {
            xBarEnd = xMin + (bronzeX - xMin) * challengesWon / BRONZE_MEDAL_REQUIREMENT;
        }
        else if (challengesWon <= SILVER_MEDAL_REQUIREMENT)
        {
            xBarEnd = bronzeX + (silverX - bronzeX) * (challengesWon - BRONZE_MEDAL_REQUIREMENT) / (SILVER_MEDAL_REQUIREMENT - BRONZE_MEDAL_REQUIREMENT);
        }
        else if (challengesWon < daysInMonth)
        {
            xBarEnd = silverX + (goldX - silverX) * (challengesWon - SILVER_MEDAL_REQUIREMENT) / (daysInMonth - SILVER_MEDAL_REQUIREMENT);
        }
        else
        {
            xBarEnd = xMin + rect.width;
        }

        float targetFill = (xBarEnd - xMin) / rect.width;
        if (progressBarSet)
        {
            progressImage.DOKill();
            progressImage.DOFillAmount(targetFill, 0.5f);
        }
        else
        {
            progressImage.fillAmount = targetFill;
            progressBarSet = true;
        }
    }
}
