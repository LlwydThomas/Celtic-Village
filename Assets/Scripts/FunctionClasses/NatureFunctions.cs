using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NatureFunctions {
    // Start is called before the first frame update
    public static int[] DetermineFloraGrowthSeasons(FloraData flora) {
        bool[] seasons = flora.growthSeasons;
        int firstSeason = -1;
        bool lastSeasonFound = false;
        bool firstSeasonFound = false;
        int lastSeason = -1;
        int count = 0;
        for (int i = 0; i < seasons.Length; i++) {
            if (seasons[i]) {
                count++;
                if (firstSeason == -1) firstSeason = i;
                if (!lastSeasonFound) {
                    lastSeason = i;
                } else {
                    if (!firstSeasonFound) {
                        firstSeason = i;
                        firstSeasonFound = true;
                    }
                }
            } else {
                //firstSeason = -1;
                if (lastSeason != -1) {
                    lastSeasonFound = true;
                }
            }
        }

        if (count == 4) {
            Debug.Log("IGM - Year round my guy" +
                " for " +
                flora.uniqueType);
            return new int[] {-1, -1 };
        } else return new int[] { firstSeason, lastSeason };

    }

    public static GameObject PrefabSelectionFromStage(FloraData floraData, FloraItem.Stage stage) {
        switch (stage) {
            case FloraItem.Stage.Seedling:
                return floraData.seedlingPrefab;
            case FloraItem.Stage.Growing:
                return floraData.growingPrefab;
            default:
                return floraData.maturePrefab;
        }
    }

    public static float CalculateFloraHealthChange(FloraItem flora, int[] healthChangeMatrix, SeasonData currentSeason, int lastRain) {
        if (healthChangeMatrix.Length != 3) {
            Debug.LogError("NF - Health change matrix of incorrect length.");
            return -1;
        }
        FloraData floraData = flora.floraData;
        float healthChange = 0f;

        // Three factors can affect a flora's health: being out of season, being infected or being without water.
        healthChange += !floraData.growthSeasons[currentSeason.id - 1] ? healthChangeMatrix[0] : 0;
        healthChange += flora.infected ? healthChangeMatrix[1] : 0;
        healthChange += lastRain > 3 ? healthChangeMatrix[2] : 0;
        return healthChange;

    }

    public static void SwapFloraPrefab(FloraItem flora, GameObject prefab = null) {
        if (prefab == null) prefab = PrefabSelectionFromStage(flora.floraData, flora.floraStage);
        GameObject.Destroy(flora.prefabLocation);
        GameObject newPrefab = GameObject.Instantiate(prefab, flora.gameObject.transform);
        flora.prefabLocation = newPrefab;
    }

    public static FloraItem.Stage DetermineFloraStage(FloraItem floraItem) {
        if (floraItem.growthPercentage < 100) {
            if (floraItem.growthPercentage < 30) {
                floraItem.floraStage = FloraItem.Stage.Seedling;
            } else {
                floraItem.floraStage = FloraItem.Stage.Growing;
            }
        } else {
            floraItem.floraStage = FloraItem.Stage.Mature;
        }
        return floraItem.floraStage;
    }

}