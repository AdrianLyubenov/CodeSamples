using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CalendarController : MonoBehaviour
{
    public List<CalendarDayController> dayControllers;
    private Dictionary<int, CalendarDayController> dayControllerDict = new Dictionary<int, CalendarDayController>();
    public Button prevMonthButton;
    public Button nextMonthButton;
    public Text currentMonthText;

    public CurrentDayController currentDayController;
    public ProgressBarController progressBarController;

    private CalendarModel model = new CalendarModel();
    private DailyChallengesModel dailyChallengesModel;

    public void Init(DailyChallengesModel dailyChallengesModel)
    {
        this.dailyChallengesModel = dailyChallengesModel;
        InitDayControllers();

        model.SelectCurrentDay();
        model.SetVisibleToCurrent();

        UpdateCalendarView();
    }

    public void Init(DailyChallengesModel dailyChallengesModel, int selectedDay)
    {
        this.dailyChallengesModel = dailyChallengesModel;
        InitDayControllers();

        model.SelectDay(selectedDay);
        model.SetVisibleToCurrent();

        UpdateCalendarView();
    }

    private void InitDayControllers()
    {
        foreach (var dayController in dayControllers)
        {
            dayController.Init(this);
        }
    }

    void UpdateCalendarView()
    {
        int firstDay = model.GetFirstDayOfVisibleMonth();
        int daysInMonth = model.GetDaysInVisibleMonth();
        int availableDays = model.GetAvailableDaysInVisibleMonth();

        int rows = (daysInMonth + firstDay + 6) / 7;
        int idx = 0;
        if (rows <= 4)
        {
            for (int i = 0; i < 7; ++i)
            {
                dayControllers[idx++].SetActive(false);
            }
        }
        for (int i = 0; i < firstDay; ++i)
        {
            dayControllers[idx++].SetActive(false);
        }
        dayControllerDict.Clear();
        for (int day = 1; day <= daysInMonth; ++day)
        {
            var dayController = dayControllers[idx++];
            dayControllerDict[day] = dayController;
            dayController.SetDayOfMonth(day);

            if (day <= availableDays)
            {
                int dayIdx = model.GetVisibleDayIdx(day);
                var challengeState = dailyChallengesModel.GetChallengeState(dayIdx);
                switch (challengeState.winType)
                {
                    case ChallengeWinType.NOT_WON:
                        dayController.SetDayType(DayType.NOT_WON);
                        break;
                    case ChallengeWinType.WON:
                        dayController.SetDayType(DayType.WON);
                        break;
                    case ChallengeWinType.WON_TODAY:
                        dayController.SetDayType(DayType.WON_TODAY);
                        break;
                }
            }
            else
            {
                dayController.SetDayType(DayType.INACCESSIBLE);
            }
            dayController.SetSelected(model.IsVisibleDaySelected(day));
            dayController.SetActive(true);
        }
        while(idx < dayControllers.Count)
        {
            dayControllers[idx++].SetActive(false);
        }

        nextMonthButton.interactable = model.CanSetNextMonth();
        prevMonthButton.interactable = model.CanSetPrevMonth();

        currentMonthText.text = model.GetVisibleMonthText();

        int selectedDayIdx = model.GetSelectedDayIdx();
        currentDayController.SetData(model.GetSelectedDayText(), model.IsSelectedDayToday(), selectedDayIdx, dailyChallengesModel.GetChallengeState(selectedDayIdx).winType);

        int challengesWon = dailyChallengesModel.GetNumberOfChallengesWon(model.GetVisibleDayIdx(1), model.GetVisibleDayIdx(daysInMonth));
        progressBarController.SetMonthData(daysInMonth, challengesWon);
    }

    private void SetButtonActive(Button button, bool active)
    {
        button.interactable = active;
    }

    public void SetNextMonth()
    {
        model.SetNextVisibleMonth();
        UpdateCalendarView();
    }

    public void SetPrevMonth()
    {
        model.SetPrevVisibleMonth();
        UpdateCalendarView();
    }

    public void SelectDayOfMonth(int dayOfMonth)
    {
        model.SelectDayInVisibleMonth(dayOfMonth);
        UpdateCalendarView();
    }

    public void MarkSelectedDayAsWon(int score)
    {
        int daysInMonth = model.GetDaysInVisibleMonth();
        int challengesWonPrev = dailyChallengesModel.GetNumberOfChallengesWon(model.GetVisibleDayIdx(1), model.GetVisibleDayIdx(daysInMonth));
        int dayIdx = model.GetSelectedDayIdx();

        ChallengeWinType prevWinType = dailyChallengesModel.GetChallengeState(dayIdx).winType;
        ChallengeWinType winType = model.IsSelectedDayToday() ? ChallengeWinType.WON_TODAY : ChallengeWinType.WON;
        dailyChallengesModel.SetChallengeWin(dayIdx, winType, score);

        int challengesWonAfter = dailyChallengesModel.GetNumberOfChallengesWon(model.GetVisibleDayIdx(1), model.GetVisibleDayIdx(daysInMonth));
        DailyChallengesLogger.LogCupWon(model.VisibleMonth, model.VisibleYear, challengesWonPrev, challengesWonAfter, daysInMonth);

        UpdateCalendarView();

        if (prevWinType == ChallengeWinType.NOT_WON)
        {
            Image crownImg = currentDayController.AnimateCrown(winType);
            if (dayControllerDict.ContainsKey(model.SelectedDay))
            {
                dayControllerDict[model.SelectedDay].StartFlyCrownAnim( crownImg );
            }
        }
    }
}
