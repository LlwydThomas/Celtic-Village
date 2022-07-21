using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeModel {
    public int dayLength, monthLength, yearLength;
    public int daysInMonth, monthsInYear;
    public float rawTime;
    public float speed;
}

[System.Serializable]
public class DateTimeObject {
    public int days, hours, minutes;
    public int months;
    public int years;
    public float rawTime;
    public float hoursAndMinutes;

    public DateTimeObject(float _rawTime, int _days, int _months, int _years, int _hours, int _minutes) {
        days = _days;
        months = _months;
        years = _years;
        rawTime = _rawTime;
        hours = _hours;
        minutes = _minutes;
        hoursAndMinutes = minutes + (hours * 60);
    }
}