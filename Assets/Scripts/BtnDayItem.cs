using System;
using UnityEngine;
using UnityEngine.UI;

public class BtnDayItem : MonoBehaviour
{
    public Button button;
    public Text txtDay;
    public Transform currSelectDayTag;
    public Color currMonthColor;
    public Color otherMonthColor;
    public Color selectColor;

    private Action<int> onClickAction;
    private DateTime dateTime;
    private int currYear;
    private int currMonth;

    // Start is called before the first frame update
    void Start()
    {
        if (button == null) button = GetComponent<Button>();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        onClickAction?.Invoke(transform.GetSiblingIndex());
    }
    public DateTime GetDateTime()
    {
        return dateTime;
    }
    public string GetDateTimeStr(string format)
    {
        return dateTime.ToString(format);
    }
    public void SetDateTime(DateTime _dateTime, int _currYear, int _currMonth)
    {
        dateTime = _dateTime;
        currMonth = _currMonth;
        currYear = _currYear;
    }
    public void SetClickAction(Action<int> _action)
    {
        onClickAction = _action;
    }
    public void RefreshUI(bool isSelect)
    {
        txtDay.text = dateTime.Day.ToString();
        bool isOtherMonth = dateTime.Month != currMonth || dateTime.Year != currYear;
        txtDay.color = isSelect ? selectColor : isOtherMonth ? otherMonthColor : currMonthColor;
        currSelectDayTag.gameObject.SetActive(isSelect);
    }
}
