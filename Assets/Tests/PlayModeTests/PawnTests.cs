using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.TestTools;
namespace Tests {

    public class PawnTests : IPrebuildSetup {
        // A Test behaves as an ordinary method
        List<SkillData> skillDatas = new List<SkillData>();
        [SetUp]
        public void Setup() {
            //pawnList.Add(new Pawn("test", skillLevelled, pawnStatus, 0));
        }

        [UnityTest]
        public IEnumerator CreatePawnAndAmendStatusLevels() {
            // Determine the vales prior to the functions invocation.
            int h1, t1, h2, t2, h3, t3, h4, t4;
            Pawn newPawn = PawnFunctions.CreateNewPawn("Llywelyn");
            Assert.IsNotNull(newPawn);
            List<Pawn> pawnList = new List<Pawn>() { newPawn };
            // Store the original values;
            h1 = newPawn.pawnStatus.hungerLevel;
            t1 = newPawn.pawnStatus.tirednessLevel;
            newPawn.pawnStatus.currentStatus = PawnStatus.CurrentStatus.Working;
            // Invoke the function and determine the new values
            int[] hungerAmends = new int[] { 2, 2 };
            PawnFunctions.IncreasePawnStatusLevels(pawnList, hungerAmends, hungerAmends, false);
            h2 = newPawn.pawnStatus.hungerLevel;
            t2 = newPawn.pawnStatus.tirednessLevel;

            newPawn.pawnStatus.currentStatus = PawnStatus.CurrentStatus.Sleeping;
            // Invoke the function and determine the new values
            PawnFunctions.IncreasePawnStatusLevels(pawnList, hungerAmends, hungerAmends, false);
            h3 = newPawn.pawnStatus.hungerLevel;
            t3 = newPawn.pawnStatus.tirednessLevel;

            Debug.Log("H1: " + h1 + ", H2: " + h2 + ", H3: " + h3 + "; T1: " + t1 + ", T2: " + t2 + ", T3: " + t3);
            // Compare the values
            Assert.Less(h2, h1);
            Assert.Less(t2, t1);
            Assert.Greater(t3, t2);
            Assert.LessOrEqual(h3, h2);
            yield return new WaitForSeconds(1f);
        }

        [UnityTest]
        public IEnumerator PawnColourTest() {
            Color[] colourArray = PawnFunctions.RandomiseColours(5);
            yield return new WaitForSeconds(0.5f);
            Assert.AreEqual(5, colourArray.Length);
        }

        [UnityTest]
        [TestCase("Cymeraf")]
        [TestCase("")]
        [TestCase("Tymer")]

        public void CreatePawnTest(string name) {
            Pawn newPawn = PawnFunctions.CreateNewPawn(name);
            bool expected = name.Length > 0 ? true : false;
            Assert.AreEqual(newPawn != null, expected);
            if (newPawn != null) {
                Assert.IsNotNull(newPawn.pawnStatus);
                Assert.AreEqual(newPawn.pawnColours.Length, 4);
                Assert.AreEqual(newPawn.name, name);
            }
        }

        [UnityTest]
        [TestCase(15)]
        [TestCase(40)]
        [TestCase(100)]
        [TestCase(140)]
        public void AgePawnTest(int age) {
            Pawn pawn = PawnFunctions.CreateNewPawn("AgeTest");
            float deathChance;
            pawn.age = age;
            deathChance = PawnFunctions.FindVillagerDeathChance(pawn);
            Assert.AreEqual(age <= 102, deathChance < 100);
        }
        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.

    }
}