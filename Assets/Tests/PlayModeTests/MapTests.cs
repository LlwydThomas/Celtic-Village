using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.TestTools;
namespace Tests {

    public class MapTests : IPrebuildSetup {
        MapSaveData mapSaveData;
        [SetUp]
        public void Setup() {
            mapSaveData = new MapSaveData(2313, 43, 200, 200, 0.5f, 0.5f);
        }

        [UnityTest]
        [TestCase(50, 50, 5)]
        [TestCase(50, 50, 50)]
        [TestCase(0, 0, 0)]
        [TestCase(1000, -1000, 25)]
        public void MapBoundsCheckTest(int x, int y, int borderSize) {
            Vector2Int vector2Int = new Vector2Int(x, y);
            bool expected = x < mapSaveData.width / 2 - borderSize && y < mapSaveData.height / 2 - borderSize ? true : false;
            Assert.AreEqual(expected, MapFunctions.MapBoundsValueCheck(vector2Int, mapSaveData, borderSize));
        }

        [UnityTest]
        [TestCase(2, 0f, 1f, 0.51f)]
        [TestCase(2, 0f, 1f, 2f)]
        [TestCase(2, 0f, 1f, 0.99f)]
        [TestCase(4, 0.5f, 0.6f, 0.55f)]
        [TestCase(4, 0.5f, 0.6f, 0.61f)]
        public void DetermineFloraChoiceTest(int florasAvailable, float minSample, float maxSample, float sample) {
            List<FloraData> floraDatas = new List<FloraData>();
            for (int i = 0; i < florasAvailable; i++) {
                FloraData floraData = ScriptableObject.CreateInstance<FloraData>();
                floraData.ID = i + 1;
                floraDatas.Add(floraData);
            }
            float range = maxSample - minSample;
            float step = range / (float) florasAvailable;
            int expected = sample <= maxSample && sample >= minSample ? Mathf.FloorToInt((sample - minSample) / step) : -1;
            FloraData foundFlora = MapFunctions.DetermineFloraChoice(floraDatas, sample, minSample, maxSample, 0f);
            Assert.AreEqual(foundFlora != null, expected != -1);
            if (foundFlora != null) Assert.AreEqual(foundFlora, floraDatas[expected]);
        }

        [UnityTest]
        [TestCase(0.5f)]
        [TestCase(0.1f)]
        [TestCase(22f)]
        [TestCase(-4f)]
        public void DetermineMapTileIndex(float sample) {
            int index = MapFunctions.DetermineTileIndex(mapSaveData, sample);
            Assert.IsTrue(index >= 0 && index <= 3);
            Assert.AreEqual(sample < mapSaveData.waterDensity, index == 0);
        }

        [UnityTest]
        [TestCase(2, 2, false)]
        [TestCase(6, 4, true)]
        [TestCase(0, 0, false)]
        public void CheckIfTileOccupiedTest(int tileX, int tileY, bool expected) {
            Vector2 size = new Vector2(3, 3);
            List<Vector2> occupiedFloras = new List<Vector2>();
            for (int x = 0; x < 3; x++) {
                for (int y = 0; y < 3; y++) {
                    occupiedFloras.Add(new Vector2(x, y));
                }
            }
            Vector2 tileCentre = new Vector2(tileX, tileY);
            Assert.AreEqual(expected, MapFunctions.CheckIfTileOccupied(tileCentre, occupiedFloras, size) != null);
        }

        [UnityTest]
        [TestCase(3, 0.3f)]
        [TestCase(24, 0.24f)]
        [TestCase(952, 0.952f)]
        [TestCase(5632, 0.5632f)]
        public void SetSeedTest(int seed, float expected) {
            Assert.AreEqual(MapFunctions.HandleSeedInput(seed), expected);
        }
    }
}