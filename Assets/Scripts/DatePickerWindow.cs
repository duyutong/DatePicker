using D.Unity3dTools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DatePickerWindow : MonoBehaviour
{
    public Text txtCurrDate;
    public Transform dayRoot;
    public SimpleLoopScrollView hourList;
    public SimpleLoopScrollView minuteList;
    public Transform dropList;
    public Text txtTimeShow;

    public Action<DateTime> onConfirm;
    public Action<DateTime> onValueChange;

    private long timestamp;
    private int currSelectIndex;
    private int currMonthCount;
    private int currYear;
    private int currMonth;
    private List<BtnDayItem> btnDayItems = new List<BtnDayItem>();
    private DateTime nowDateTime;
    private float timeCount;

    private DateTime currDateTime;
    private int currHour;
    private int currMinute;

    // Start is called before the first frame update
    void Start()
    {
        InitWindow();
    }
    private void InitMinuteItems(int infoIndex, SimpleLoopItem item)
    {
        int showInfo = infoIndex;
        item.txtValue.text = showInfo.ToString();
        item.toggle.SetIsOnWithoutNotify(showInfo == currMinute);
    }

    private void InitHourItems(int infoIndex, SimpleLoopItem item)
    {
        int showInfo = infoIndex;
        item.txtValue.text = showInfo.ToString();
        item.toggle.SetIsOnWithoutNotify(showInfo == currHour);
    }

    // Update is called once per frame
    void Update()
    {
        if (timestamp != 0) timeCount += Time.deltaTime;
        if (timeCount >= 1)
        {
            timeCount = 0;
            nowDateTime.AddSeconds(1);
        }
    }

    public void InitWindow(long _timestamp = 0)
    {
        currMonthCount = 0;
        currDateTime = DateTime.MinValue;

        timestamp = _timestamp <= 0 ? DatePickerUtil.GetCurrentTimeStampMilliseconds() : _timestamp;

        SetNewTimestamp(timestamp);
        hourList.InitData(24, InitHourItems, OnSelectHour);
        minuteList.InitData(60, InitMinuteItems, OnSelectMinute);

        //dropList.DoFadeOut(0);
        dropList.gameObject.SetActive(false);

        txtTimeShow.text = $"{currHour:d2}:{currMinute:d2}";
    }
    public void OnSelectHour(int infoIndex)
    {
        OnValueChange(infoIndex, currMinute);
        currHour = infoIndex;

    }
    public void OnSelectMinute(int infoIndex)
    {
        OnValueChange(currHour, infoIndex);
        currMinute = infoIndex;
    }
    public void OnClickConfirm()
    {
        DateTime result = currDateTime.AddHours(currHour).AddMinutes(currMinute);
        Debug.Log(result.ToString("g"));
        onConfirm?.Invoke(result);
    }
    public void OnClickTimePicker()
    {
        //dropList.DoFadeIn();
        hourList.ScrollToInfoIndex(currHour, EScrollSibling.Center);
        minuteList.ScrollToInfoIndex(currMinute, EScrollSibling.Center);
        dropList.gameObject.SetActive(true);
    }
    public void OnClickCloseTimePicker()
    {
        //dropList.DoFadeOut();
        dropList.gameObject.SetActive(false);
        txtTimeShow.text = $"{currHour:d2}:{currMinute:d2}";
    }
    private void SetNewTimestamp(long _timestamp)
    {
        timestamp = _timestamp;
        nowDateTime = DatePickerUtil.TimestampToDateTime(timestamp);
        currMonth = nowDateTime.Month;
        currYear = nowDateTime.Year;
        currHour = nowDateTime.Hour;
        currMinute = nowDateTime.Minute;

        RefreshDayRoot();
    }
    private void RefreshDayRoot()
    {
        if (btnDayItems.Count == 0)
        {
            for (int i = 0; i < dayRoot.childCount; i++)
            {
                Transform child = dayRoot.GetChild(i);
                BtnDayItem btnDayItem = child.GetComponent<BtnDayItem>();
                btnDayItem.SetClickAction(OnClickDay);
                btnDayItems.Add(btnDayItem);
            }
        }

        DateTime firstDayOfMonth = DatePickerUtil.GetFirstDayOfMonth(currYear, currMonth);
        int firstDayPos = DatePickerUtil.ConvertToNumericDayOfWeek(firstDayOfMonth);

        for (int i = 0; i < btnDayItems.Count; i++)
        {
            int index = i;
            int dayOffset = i - firstDayPos;
            BtnDayItem btnDayItem = btnDayItems[index];
            DateTime setDateTime = firstDayOfMonth.AddDays(dayOffset);
            btnDayItem.SetDateTime(setDateTime, currYear, currMonth);

            if (currDateTime == DateTime.MinValue)
            {
                bool isToday = setDateTime.Month == nowDateTime.Month && setDateTime.Day == nowDateTime.Day;
                if (isToday)
                {
                    currDateTime = setDateTime;
                    currSelectIndex = index;
                }
            }

            bool isSelectDay = setDateTime == currDateTime;
            if (isSelectDay) currSelectIndex = index;
            btnDayItem.RefreshUI(isSelectDay);
        }

        txtCurrDate.text = $"{currYear}-{currMonth}";
    }

    private void OnValueChange(DateTime newSelect)
    {
        if (onValueChange == null) return;
        if (currDateTime != newSelect)
        {
            DateTime newDateTime = newSelect.AddHours(currHour).AddMinutes(currMinute);
            onValueChange?.Invoke(newDateTime);
        }
    }
    private void OnValueChange(int newHour, int newMinute)
    {
        if (onValueChange == null) return;
        if (currHour != newHour || currMinute != newMinute)
        {
            DateTime newDateTime = currDateTime.AddHours(newHour).AddMinutes(newMinute);
            onValueChange?.Invoke(newDateTime);
        }
    }
    private void OnClickDay(int indexOnTable)
    {
        btnDayItems[currSelectIndex].RefreshUI(false);
        btnDayItems[indexOnTable].RefreshUI(true);
        currSelectIndex = indexOnTable;
        DateTime newSelect = btnDayItems[currSelectIndex].GetDateTime();
        OnValueChange(newSelect);
        currDateTime = newSelect;

        if (currDateTime.Year != currYear || currDateTime.Month != currMonth)
        {
            currYear = currDateTime.Year;
            currMonth = currDateTime.Month;
            currMonthCount = 0;
            RefreshDayRoot();
        }
    }

    public void OnClickLastMonth()
    {
        currMonthCount--;
        OnChangePage();
    }

    private void OnChangePage()
    {
        DatePickerUtil.CalculateMonthOffset(currDateTime.Year, currDateTime.Month, currMonthCount, out currYear, out currMonth);
        RefreshDayRoot();
    }

    public void OnClickNextMonth()
    {
        currMonthCount++;
        OnChangePage();
    }
}
