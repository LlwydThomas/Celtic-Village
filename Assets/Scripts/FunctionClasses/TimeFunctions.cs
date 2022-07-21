using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TimeFunctions {
    // Start is called before the first frame update
    public static DateTimeObject ConvertDateTimeObject(float rawTime, TimeModel timeModel) {
        int yearsConverted = Mathf.FloorToInt(rawTime / (float) timeModel.yearLength) + 1;
        float yearsRemainder = rawTime % (float) timeModel.yearLength;
        int monthsConverted = Mathf.FloorToInt(yearsRemainder / (float) timeModel.monthLength) + 1;
        float monthsRemainder = yearsRemainder % (float) timeModel.monthLength;
        int daysConverted = Mathf.FloorToInt(monthsRemainder / (float) timeModel.dayLength) + 1;
        float daysRemainder = monthsRemainder % (float) timeModel.dayLength;
        int hours = Mathf.FloorToInt(daysRemainder / 60f);
        float hoursRemainder = daysRemainder % (float) 60;
        int minutes = Mathf.FloorToInt(hoursRemainder);
        float minutesRemainder = hoursRemainder % 60;
        //Debug.Log("Raw time converted to " + yearsConverted + " years, " + monthsConverted + " months, " + daysConverted + " days, " + hours + " hours, " + minutes + " mins.");
        // Return the calculated values through the DateTimeObject class.
        return new DateTimeObject(rawTime, daysConverted, monthsConverted, yearsConverted, hours, minutes);
    }
    public static bool CompareDateTimes(DateTimeObject dateTime1, DateTimeObject dateTime2) {
        Debug.Log("DTC - Comparing hours: " + dateTime1.hours + ", days: " + dateTime1.days + ", months: " + dateTime1.months + ", years: " + dateTime1.years + " /to/ " + "hours: " + dateTime2.hours + ", days: " + dateTime2.days + ", months: " + dateTime2.months + ", years: " + dateTime2.years);

        if (dateTime1.days == dateTime2.days && dateTime1.months == dateTime2.months && dateTime1.years == dateTime2.years) return true;
        else return false;
    }
    public static float LightIntensityDeduction(float minimumIntensity, float maximumIntesity, int morningEndHour, int eveningEndHour, int _hours, float _minutes) {
        float nightScale;
        float pointInNight;
        float nightMidPoint = (24f - (float) eveningEndHour + (float) morningEndHour) / 2f;

        float intensityRange = (maximumIntesity - minimumIntensity);
        if (_hours <= morningEndHour) pointInNight = (float) (24 - eveningEndHour) + _hours + (_minutes / 60);
        else pointInNight = (float) (_hours - eveningEndHour) + (_minutes / 60);
        //Debug.Log("TF - NMP: " + nightMidPoint + ", PIN: " + pointInNight);
        //Debug.Log(pointInNight);
        if (pointInNight <= nightMidPoint) nightScale = (nightMidPoint - pointInNight) / nightMidPoint;
        else nightScale = (pointInNight - nightMidPoint) / nightMidPoint;
        return Mathf.Clamp(minimumIntensity + (nightScale * intensityRange), minimumIntensity, maximumIntesity);
    }

    public static float TemperatureDeduction(float baseTemp, float minTemp, float maxTemp, int morningEndHour, int eveningStartHour, int _hours, float _minutes) {
        float temperatureScale;
        float temperatureRange = maxTemp - minTemp;
        float pointInDay;
        float dayMidPoint = 12;
        if (_hours <= dayMidPoint) pointInDay = _hours + (_minutes / 60);
        else pointInDay = (float) 24 - _hours - (_minutes / 60);
        float tempInterval = temperatureRange / dayMidPoint;
        //Debug.Log(pointInNight);
        return Mathf.Clamp(tempInterval * pointInDay, minTemp, maxTemp);
    }

    public static float DateAddition(TimeModel timeModel, int hours = 0, int days = 0, int months = 0, int years = 0) {
        float additiveDate = (hours * 60) + (days * timeModel.dayLength) + (months * timeModel.monthLength) + (years * timeModel.yearLength);
        return timeModel.rawTime + additiveDate;
    }

    public static int DeduceSeason(TimeModel timeModel, int month) {
        if (month < 0 || month > timeModel.monthsInYear) return -1;
        int yearLength = timeModel.monthsInYear;
        int interval = yearLength / 4;
        Debug.Log("TIF - Division Check: " + 7200 / 1440);
        Debug.Log("TIF - Months in year " + yearLength + ", interval of " + interval);
        Debug.Log("TIF - Day Length: " + timeModel.dayLength + ", Month Length: " + timeModel.monthLength + ", Days in Month " + timeModel.daysInMonth + ", Months in year " + timeModel.monthsInYear);
        int season;
        if (month >= interval) {
            if (month >= interval * 2) {
                if (month >= interval * 3) {
                    if (month >= interval * 4) {
                        season = 4;
                    } else season = 3;
                } else season = 2;
            } else season = 1;
        } else season = -1;
        Debug.Log("TIF - current month " + month + ", currentInt = " + interval + " returns season of " + season);
        return season;
    }

    public static void InitialiseTimeModel(TimeModel timeModel, int _yearLength, int _monthLength, int _dayLength, float _rawTime) {
        timeModel.dayLength = _dayLength;
        timeModel.monthLength = _monthLength;
        timeModel.yearLength = _yearLength;
        timeModel.daysInMonth = _monthLength / _dayLength;
        timeModel.monthsInYear = _yearLength / _monthLength;
        timeModel.rawTime = _rawTime;
    }

    public static string GetSeasonString(int seasonIndex) {
        switch (seasonIndex) {
            case 1:
                return "Winter";
            case 2:
                return "Spring";
            case 3:
                return "Summer";
            case 4:
                return "Autumn";
            default:
                return "";
        }
    }

    public static DailyWeatherData CalculateWeatherData(SeasonData season, DateTimeObject currentDateTime, float tempFluctuation) {
        Debug.Log("WHC - Passes the compare date times check.");
        if (season != null) {
            DailyWeatherData dailyWeather;
            bool precipitaion, rainfallQueued;
            precipitaion = Random.Range(0.0f, 0.1f) <= season.chanceOfPrecipiation ? true : false;
            int precipitationLength = precipitaion ? Random.Range(1, 10) : 0;
            float averageTemperature = ((float) (season.maxTemp + season.minTemp) / 2f) + Random.Range(-tempFluctuation, tempFluctuation);
            dailyWeather = new DailyWeatherData(season, currentDateTime, precipitaion, precipitationLength, averageTemperature);
            dailyWeather.rainfallQueued = false;
            return dailyWeather;
        } else return null;
    }
}