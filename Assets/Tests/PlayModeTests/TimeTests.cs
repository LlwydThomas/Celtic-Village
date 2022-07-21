using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.TestTools;
namespace Tests {

    public class TimeTests : IPrebuildSetup {
        TimeModel timeModel;
        [SetUp]
        public void Setup() {
            timeModel = new TimeModel();
            TimeFunctions.InitialiseTimeModel(timeModel, 1440 * 4 * 5, 1440 * 5, 1440, 0);
        }

        [UnityTest]
        [TestCase(1, 10)]
        [TestCase(4, 6)]
        [TestCase(10, 18)]

        public void LightIntensityTest(int morningEnd, int eveningEnd) {
            float lightIntesity = TimeFunctions.LightIntensityDeduction(0, 1, morningEnd, eveningEnd, morningEnd, 0);
            float lightIntesity2 = TimeFunctions.LightIntensityDeduction(0, 1, morningEnd, eveningEnd, eveningEnd, 0);
            int midDay = Mathf.FloorToInt(((float) eveningEnd + (float) morningEnd) / 2);
            float lightIntesity3 = TimeFunctions.LightIntensityDeduction(0, 1, morningEnd, eveningEnd, midDay, 0);
            Assert.IsTrue(lightIntesity > 0 && lightIntesity < 1);
            Assert.IsTrue(lightIntesity2 > 0 && lightIntesity < 1);
            Assert.IsTrue(lightIntesity3 > 0 && lightIntesity < 1);
            Assert.Greater(lightIntesity3, lightIntesity);
            Assert.Greater(lightIntesity3, lightIntesity2);
        }

        [UnityTest]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(6)]

        public void DeduceSeasonTest(int month) {
            int expected = -1;
            switch (month) {
                case 1:
                    expected = 1;
                    break;
                case 2:
                    expected = 2;
                    break;
                case 3:
                    expected = 3;
                    break;
                case 4:
                    expected = 4;
                    break;
            }
            int value = TimeFunctions.DeduceSeason(timeModel, month);
            Assert.AreEqual(expected, value);
        }

        [UnityTest]
        [TestCase(10, 25, 3123, 4.5f)]
        [TestCase(-10, -10, 1, 1f)]
        [TestCase(20, 60, 3123, 100f)]
        public void DailyWeatherCalculationTest(int minTemp, int maxTemp, float rawTime, float tempFluctuation) {
            SeasonData season = ScriptableObject.CreateInstance<SeasonData>();
            season.chanceOfPrecipiation = 0.5f;
            season.minTemp = minTemp;
            season.maxTemp = maxTemp;
            DateTimeObject dateTime = TimeFunctions.ConvertDateTimeObject(rawTime, timeModel);
            DailyWeatherData dailyWeather = TimeFunctions.CalculateWeatherData(season, dateTime, tempFluctuation);
            Assert.AreEqual(dailyWeather.precipitaion, dailyWeather.precipitationLength > 0);
            Assert.IsTrue(dailyWeather.averageTemperature > minTemp - tempFluctuation && dailyWeather.averageTemperature < maxTemp + tempFluctuation);
        }

    }
}