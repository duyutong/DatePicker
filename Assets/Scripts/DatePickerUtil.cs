using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class DatePickerUtil
{
    // ����һ��ʱ��������룩�����ض�Ӧ��DateTime����
    public static DateTime TimestampToDateTime(long timestampMillis)
    {
        // Unix��ʼʱ�䣨1970��1��1�գ���DateTime��ʾ
        DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // ���ʱ���������
        DateTime dateTime = epoch.AddMilliseconds(timestampMillis);

        // ת��Ϊ������ʱ�䣨����ʱ�䣩
        dateTime = dateTime.ToLocalTime();

        return dateTime;
    }
    public static long GetCurrentTimeStampMilliseconds()
    {
        DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        long timeStampMilliseconds = (long)(DateTime.UtcNow - epochStart).TotalMilliseconds;
        return timeStampMilliseconds;
    }
    public static void CalculateMonthOffset(int year, int month, int monthOffset, out int resultYear, out int resultMonth)
    {
        // �������·�
        int totalMonths = year * 12 + month - 1;

        // ����ƫ����
        totalMonths += monthOffset;

        // ��������ݺ��·�
        resultYear = totalMonths / 12;
        resultMonth = totalMonths % 12 + 1; // �� 1 ����Ϊ�·ݴ� 1 ��ʼ����
    }
    public static DateTime GetFirstDayOfMonth(int year, int month)
    {
        // ����ָ����ݺ��·ݵĵ�һ��� DateTime ����
        DateTime firstDayOfMonth = new DateTime(year, month, 1);

        return firstDayOfMonth;
    }
    public static int ConvertToNumericDayOfWeek(DateTime date)
    {
        // ��ȡ���ڵ����ڼ�
        DayOfWeek dayOfWeek = date.DayOfWeek;

        // ��������ת��Ϊ 0������һת��Ϊ 1���Դ�����
        switch (dayOfWeek)
        {
            case DayOfWeek.Sunday:
                return 0;
            case DayOfWeek.Monday:
                return 1;
            case DayOfWeek.Tuesday:
                return 2;
            case DayOfWeek.Wednesday:
                return 3;
            case DayOfWeek.Thursday:
                return 4;
            case DayOfWeek.Friday:
                return 5;
            case DayOfWeek.Saturday:
                return 6;
            default:
                return -1; // �������δ֪��������� -1 ��ʾ����
        }
    }

}
