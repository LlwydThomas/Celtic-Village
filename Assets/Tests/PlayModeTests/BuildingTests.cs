using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.TestTools;
namespace Tests {

    public class BuildingTests : IPrebuildSetup {
        GridModel gridModel;

        [SetUp]
        public void Setup() {
            //TimeObject = GameObject.Instantiate (Resources.Load<GameObject> ("ControllerPrefabs/MainWorld"));
            gridModel = new GridModel();
            gridModel.gridList = new int[50, 50];
            gridModel.cellSize = 1f;
        }

        [UnityTest]
        [TestCase(QualityTier.I)]
        [TestCase(QualityTier.II)]
        [TestCase(QualityTier.III)]
        public void LessOrEqualToQualityTierTest(QualityTier qualityTier) {
            int expected = (int) qualityTier + 1;
            List<QualityTier> qualityTiers = BuildingFunctions.LessOrEqualToQualityTier(qualityTier);
            Assert.AreEqual(qualityTiers.Count, expected);
        }

        [UnityTest]
        [TestCase(QualityTier.I)]
        [TestCase(QualityTier.II)]
        [TestCase(QualityTier.III)]
        public void FindFlorasOfQualityTest(QualityTier qualityTier) {
            List<FloraData> floraDatas = new List<FloraData>();
            for (int i = 0; i < 5; i++) {
                floraDatas.Add(ScriptableObject.CreateInstance<FloraData>());
                floraDatas[i].qualityTier = i < 3 ? QualityTier.I : QualityTier.III;
            }
            int expected = floraDatas.FindAll(x => (int) x.qualityTier <= (int) qualityTier).Count;
            List<FloraData> floras = BuildingFunctions.FlorasOfQuality(floraDatas, qualityTier);
            Assert.AreEqual(floras.Count, expected);
        }

        [UnityTest]
        [TestCase(5, 1, 1)]
        [TestCase(10, 0, 0)]
        [TestCase(2, 10, 10)]
        public void BuildingNodesCalculatorTest(int radiusSize, int rectSizeX, int rectSizeY) {
            PurposeData purposeData = ScriptableObject.CreateInstance<PurposeData>();
            purposeData.radius = radiusSize;
            StructureData structureData = ScriptableObject.CreateInstance<StructureData>();
            structureData.radiusMultiplier = 1f;
            Build build = new Build("Test", Vector3.zero, new GameObject(), structureData, purposeData, 1);
            GameObject referenceObject = new GameObject();
            BuildingReferences buildingReferences = referenceObject.AddComponent<BuildingReferences>();
            build.buildingReferences = buildingReferences;
            GameObject workerRect = new GameObject();
            buildingReferences.workerNodeTransform = workerRect.AddComponent<RectTransform>();
            buildingReferences.workerNodeTransform.sizeDelta = new Vector2(rectSizeX, rectSizeY);
            GameObject internalRect = new GameObject();
            buildingReferences.internalNodeRectTransform = internalRect.AddComponent<RectTransform>();
            buildingReferences.internalNodeRectTransform.sizeDelta = new Vector2(rectSizeX, rectSizeY);
            int expectedRadiusNodes = 0;
            List<Node> nodeList = new List<Node>();
            Dictionary<Vector2, Node> nodeBank = new Dictionary<Vector2, Node>();
            int id = 1;
            for (int x = -50; x < 50; x++) {
                for (int y = -50; y < 50; y++) {
                    id++;
                    Vector3 position = new Vector3(x, y, 0);
                    Vector2 vector2 = new Vector2(x, y);
                    Node node = new Node(id, true, false, position, x, y, null, null);
                    nodeBank.Add(vector2, node);
                    if (Vector3.Distance(build.worldPosition, position) < radiusSize) expectedRadiusNodes += 1;
                }
            }

            BuildingFunctions.BuildingNodeCalculator(build, nodeBank, gridModel.cellSize);
            Assert.AreEqual(build.workerNodes.Count, rectSizeY * rectSizeX);
            Assert.AreEqual(build.internalNodes.Count, rectSizeX * rectSizeY);
            Assert.AreEqual(build.availableNodes.Count, expectedRadiusNodes);
        }

    }
}