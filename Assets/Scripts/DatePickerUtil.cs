using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class DatePickerUtil
{
    // 输入一个时间戳（毫秒），返回对应的DateTime对象
    public static DateTime TimestampToDateTime(long timestampMillis)
    {
        // Unix起始时间（1970年1月1日）的DateTime表示
        DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // 添加时间戳毫秒数
        DateTime dateTime = epoch.AddMilliseconds(timestampMillis);

        // 转换为东八区时间（北京时间）
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
        // 计算总月份
        int totalMonths = year * 12 + month - 1;

        // 加上偏移量
        totalMonths += monthOffset;

        // 计算结果年份和月份
        resultYear = totalMonths / 12;
        resultMonth = totalMonths % 12 + 1; // 加 1 是因为月份从 1 开始计数
    }
    public static DateTime GetFirstDayOfMonth(int year, int month)
    {
        // 构造指定年份和月份的第一天的 DateTime 对象
        DateTime firstDayOfMonth = new DateTime(year, month, 1);

        return firstDayOfMonth;
    }
    public static int ConvertToNumericDayOfWeek(DateTime date)
    {
        // 获取日期的星期几
        DayOfWeek dayOfWeek = date.DayOfWeek;

        // 将星期日转换为 0，星期一转换为 1，以此类推
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
                return -1; // 如果出现未知情况，返回 -1 表示错误
        }
    }

}
