using System;
public class CalendarModel
{
    private int selectedDay = 1;
    private int selectedMonth = 1;
    private int selectedYear = 2021;

    private int visibleMonth = 1;
    private int visibleYear = 2021;

    private readonly string[] MONTH_NAME_KEYS = new string[]{
        "DAILY_JANUARY",
        "DAILY_FEBRUARY",
        "DAILY_MARCH",
        "DAILY_APRIL",
        "DAILY_MAY",
        "DAILY_JUNE",
        "DAILY_JULY",
        "DAILY_AUGUST",
        "DAILY_SEPTEMBER",
        "DAILY_OCTOBER",
        "DAILY_NOVEMBER",
        "DAILY_DECEMBER"
    };

    private readonly string[] MONTH_SHORT_NAME_KEYS = new string[]{
        "DAILY_SHORT_JAN",
        "DAILY_SHORT_FEB",
        "DAILY_SHORT_MAR",
        "DAILY_SHORT_APR",
        "DAILY_SHORT_MAY",
        "DAILY_SHORT_JUN",
        "DAILY_SHORT_JUL",
        "DAILY_SHORT_AUG",
        "DAILY_SHORT_SEP",
        "DAILY_SHORT_OCT",
        "DAILY_SHORT_NOV",
        "DAILY_SHORT_DEC"
    };

    private static readonly DateTime MIN_DATE = new DateTime(2021, 1, 1);

    public int SelectedDay {
        get => selectedDay;
    }

    public int SelectedMonth {
        get => selectedDay;
    }

    public int SelectedYear {
        get => selectedDay;
    }

    public int VisibleMonth {
        get => visibleMonth;
    }

    public int VisibleYear {
        get => visibleYear;
    }

    public void SelectCurrentDay()
    {
        var now = DateTime.Now;
        selectedDay = now.Day;
        selectedMonth = now.Month;
        selectedYear = now.Year;
    }

    public void SelectDay(int dayIdx)
    {
        var date = MIN_DATE.AddDays(dayIdx);
        selectedDay = date.Day;
        selectedMonth = date.Month;
        selectedYear = date.Year;
    }

    public void SelectDayInVisibleMonth(int dayOfMonth)
    {
        selectedDay = dayOfMonth;
        selectedMonth = visibleMonth;
        selectedYear = visibleYear;
    }

    public void SetVisibleToCurrent()
    {
        visibleMonth = selectedMonth;
        visibleYear = selectedYear;
    }

    public void SetPrevVisibleMonth()
    {
        if (CanSetPrevMonth())
        {
            visibleMonth--;
            if (visibleMonth < 1)
            {
                visibleYear--;
                visibleMonth = 12;
            }
        }
    }

    public void SetNextVisibleMonth()
    {
        if (CanSetNextMonth())
        {
            visibleMonth++;
            if (visibleMonth > 12)
            {
                visibleYear++;
                visibleMonth = 1;
            }
        }
    }

    public bool CanSetPrevMonth()
    {
        return !(visibleMonth == MIN_DATE.Month && visibleYear == MIN_DATE.Year);
    }

    public bool CanSetNextMonth()
    {
        var now = DateTime.Now;
        return !(visibleMonth == now.Month && visibleYear == now.Year);
    }

    public int GetDaysInVisibleMonth()
    {
        return DateTime.DaysInMonth(visibleYear, VisibleMonth);
    }

    public int GetAvailableDaysInVisibleMonth()
    {
        var now = DateTime.Now;
        if (visibleMonth == now.Month && visibleYear == now.Year)
        {
            return now.Day;
        }
        else
        {
            return DateTime.DaysInMonth(visibleYear, VisibleMonth);
        }
    }


    public int GetFirstDayOfVisibleMonth()
    {
        DateTime date = new DateTime(visibleYear, visibleMonth, 1);
        switch (date.DayOfWeek)
        {
            case DayOfWeek.Sunday: return 0;
            case DayOfWeek.Monday: return 1;
            case DayOfWeek.Tuesday: return 2;
            case DayOfWeek.Wednesday: return 3;
            case DayOfWeek.Thursday: return 4;
            case DayOfWeek.Friday: return 5;
            case DayOfWeek.Saturday: return 6;
        }
        return 0;
    }

    public bool IsVisibleDaySelected(int day)
    {
        return day == selectedDay && visibleMonth == selectedMonth && visibleYear == selectedYear;
    }

    public string GetVisibleMonthText()
    {
        return I2.Loc.LocalizationManager.GetTermTranslation(MONTH_NAME_KEYS[visibleMonth-1]) + " " + visibleYear.ToString();
    }

    public string GetSelectedDayText()
    {
        return I2.Loc.LocalizationManager.GetTermTranslation(MONTH_SHORT_NAME_KEYS[selectedMonth-1]) + " " + SelectedDay.ToString();
    }

    public bool IsSelectedDayToday()
    {
        var now = DateTime.Now;
        return selectedDay == now.Day && selectedMonth == now.Month && selectedYear == now.Year;
    }

    public int GetSelectedDayIdx()
    {
        DateTime selectedDate = new DateTime(selectedYear, selectedMonth, selectedDay);
        return (selectedDate - MIN_DATE).Days;
    }

    public int GetVisibleDayIdx(int day)
    {
        DateTime visibleDate = new DateTime(visibleYear, visibleMonth, day);
        return (visibleDate - MIN_DATE).Days;
    }

    public static bool IsDayIdxToday(int dayIdx)
    {
        DateTime now = DateTime.Now;
        return (now - MIN_DATE).Days == dayIdx;
    }

    public static int GetTodayDayIdx()
    {
        DateTime now = DateTime.Now;
        return (now - MIN_DATE).Days;
    }

    public static int GetDayIdxFromEpochMillis(long millis)
    {
        DateTime date = DateTimeOffset.FromUnixTimeMilliseconds(millis).DateTime;
        return (date - MIN_DATE).Days;
    }

    public static DateTime GetDateFromDayIdx(int dayIdx)
    {
        return MIN_DATE.AddDays(dayIdx);
    }
}
