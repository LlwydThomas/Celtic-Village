using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherModel {
    // Start is called before the first frame update
    public SeasonData currentSeason;

    public float currentTemperature;
    public List<SeasonData> seasonDatas = new List<SeasonData>();
    public Dictionary<int, SeasonData> seasonDataLookup = new Dictionary<int, SeasonData>();

    public int daysSinceRain = 0;

}