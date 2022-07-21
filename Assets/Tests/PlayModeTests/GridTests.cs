using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.TestTools;
namespace Tests {

    public class GridTests : IPrebuildSetup {
        // A Test behaves as an ordinary method
        GridModel gridModel;

        [SetUp]
        public void Setup() {
            //TimeObject = GameObject.Instantiate (Resources.Load<GameObject> ("ControllerPrefabs/MainWorld"));
            gridModel = new GridModel();
            gridModel.gridList = new int[50, 50];
            gridModel.cellSize = 1f;
        }

        [UnityTest]
        [TestCase(1, 1, 100, 100, 1)]
        [TestCase(-100, 1, 10, 10, 2)]
        [TestCase(15, 15, 30, 30, 2)]
        public void CheckCoordinatesWithRestrainsTest(int x, int y, int sizeX, int sizeY, int scale) {
            int[, ] size = new int[sizeX, sizeY];
            bool expected = (x >= -sizeX / scale && x < sizeX / scale && y >= -sizeY / scale && y < sizeY / scale);
            bool result = GridFunctions.CheckCoordsWithRestraints(x, y, size, scale);
            Assert.AreEqual(expected, result);
        }

        [UnityTest]
        [TestCase(5, 5, 8)]
        [TestCase(0, 0, 3)]
        [TestCase(20, 20, 0)]
        public void ReturnNeighboursTest(int nodeX, int nodeY, int expectedNeighbours) {
            List<Node> nodeList = new List<Node>();
            Dictionary<Vector2, Node> nodeBank = new Dictionary<Vector2, Node>();
            int id = 1;
            for (int x = 0; x < 10; x++) {
                for (int y = 0; y < 10; y++) {
                    id++;
                    Vector3 position = new Vector3(x, y, 1);
                    Vector2 vector2 = new Vector2(x, y);
                    Node node = new Node(id, true, false, position, x, y, null, null);
                    nodeBank.Add(vector2, node);
                }
            }

            Vector2 nodePosition = new Vector2(nodeX, nodeY);
            if (nodeBank.ContainsKey(nodePosition)) {
                Node currentNode = nodeBank[nodePosition];
                List<Node> returnList = GridFunctions.ReturnNeighbours(currentNode, 1, gridModel.gridList, nodeBank);
                Assert.AreEqual(returnList.Count, expectedNeighbours);
            } else Assert.True(true);

        }

        [UnityTest]
        [TestCase(5, 5, 2)]
        [TestCase(100, 100, 5)]
        [TestCase(10, 10, 6)]

        public void NodesWithinRadiusTest(int nodeX, int nodeY, int radius) {

            List<Node> nodeList = new List<Node>();
            Dictionary<Vector2, Node> nodeBank = new Dictionary<Vector2, Node>();
            Vector3 nodePosition = new Vector3(nodeX, nodeY, 1);
            int expected = 0;
            int id = 1;
            for (int x = 0; x < 10; x++) {
                for (int y = 0; y < 10; y++) {
                    id++;
                    Vector3 position = new Vector3(x, y, 1);
                    Vector2 vector2 = new Vector2(x, y);
                    Node node = new Node(id, true, false, position, x, y, null, null);
                    nodeBank.Add(vector2, node);
                    if (Vector3.Distance(position, nodePosition) < radius) expected += 1;
                }
            }

            List<Node> returnList = GridFunctions.NodesWithinRadius(nodeBank, gridModel.cellSize, nodePosition, radius);
            Assert.AreEqual(returnList.Count, expected);
        }

    }
}