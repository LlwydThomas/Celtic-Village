using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.TestTools;
namespace Tests {

    public class AITests : IPrebuildSetup {
        // A Test behaves as an ordinary method
        private GameObject TimeObject;
        private GameObject rigidObject;
        private Rigidbody2D testBody;
        GridModel gridModel;
        TimeModel timeModel;

        [SetUp]
        public void Setup() {
            //TimeObject = GameObject.Instantiate (Resources.Load<GameObject> ("ControllerPrefabs/MainWorld"));
            gridModel = new GridModel();
            gridModel.gridList = new int[50, 50];
            gridModel.cellSize = 1f;

            timeModel = new TimeModel();
            TimeFunctions.InitialiseTimeModel(timeModel, 1440 * 4 * 5, 1440 * 5, 1440, 0);
        }

        [UnityTest]

        public IEnumerator TestAIMovement() {
            // Insntiate a game object, and add a rigidbody component to it.
            rigidObject = new GameObject();
            rigidObject.transform.position = new Vector3(0f, 0f, 0f);
            testBody = rigidObject.AddComponent<Rigidbody2D>();
            // Reference the AiFunctions class, and define a target position.
            Vector3 targetPosition = new Vector3(1, 1, 0);
            Debug.Log(targetPosition);

            // Run the move towards function, and compare the position of the rigidbody with the target position. If they are equal, the test has passed.

            AiFunctions.MoveTowards(testBody, targetPosition, 100f, 1f);
            yield return new WaitForSeconds(4f);
            Assert.AreEqual(testBody.transform.position, targetPosition);
        }

        [UnityTest]
        [TestCase(1, 1, 40, 40, false)]
        [TestCase(22, 5, 35, 5, false)]
        [TestCase(1, 1, 50, 50, false)]
        [TestCase(1, 1, 2, 2, true)]
        [TestCase(5, 5, 24, 5, true)]
        public void FindRouteTest(int startX, int startY, int endX, int endY, bool possible) {
            // Insntiate a game object, and add a rigidbody component to it.
            List<Node> nodeList = new List<Node>();
            Dictionary<Vector2, Node> nodeBank = new Dictionary<Vector2, Node>();
            int id = 1;
            // Create a grid of nodes with a line of unwalkables down the middle.
            for (int x = 0; x < 50; x++) {
                for (int y = 0; y < 50; y++) {
                    id++;
                    Vector3 worldPos = new Vector3(x, y, 1);
                    Vector2 nodeBankPos = new Vector2(x, y);
                    Node node = new Node(id, true, false, worldPos, x, y, null, null);
                    if (x == 25) node.walkable = false;
                    nodeList.Add(node);
                    nodeBank.Add(nodeBankPos, node);
                }
            }
            Vector2 startVect = new Vector2(startX, startY);
            Vector2 endVect = new Vector2(endX, endY);
            if (nodeBank.ContainsKey(startVect) && nodeBank.ContainsKey(endVect)) {
                Node startNode = nodeBank[startVect];
                Node endNode = nodeBank[endVect];
                List<Node> foundRoute = AiFunctions.FindRoute(startNode, endNode, nodeBank, gridModel, 1);
                string debugString = "AIT - Found route:";
                foreach (Node node in foundRoute) {
                    debugString += " " + node.worldPosition;
                }
                debugString += ".";
                Debug.Log(debugString);
                Assert.AreEqual(foundRoute.Count > 0, possible);
            }

        }

        [UnityTest]
        [TestCase(1, 1)]
        [TestCase(3, 2)]
        [TestCase(10, 5)]
        [TestCase(0, 10)]
        [TestCase(5, 20)]
        public void FindEventChanceTest(int daysSince, int minForEvent) {
            float baseChance = 0.5f;
            float offsetDivisor = 0.1f;
            DateTimeObject dateTimeObject = TimeFunctions.ConvertDateTimeObject(daysSince * 1445, timeModel);
            Debug.Log("Date time: " + dateTimeObject.rawTime + ", days: " + dateTimeObject.days);
            int difference = daysSince - minForEvent;
            float expected = daysSince >= minForEvent ? baseChance + (offsetDivisor * difference) : -1f;
            expected = Mathf.Clamp(expected, -1f, 1f);
            float actual = AiFunctions.CalculateProbabilityOfEvent(dateTimeObject, minForEvent, offsetDivisor, baseChance, timeModel.dayLength);
            Assert.AreEqual(expected, actual);
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.

    }
}