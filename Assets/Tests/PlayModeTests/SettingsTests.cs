using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.TestTools;
namespace Tests {

    public class SettingsTests : IPrebuildSetup {
        // A Test behaves as an ordinary method
        private GameObject settingsObject;
        private SettingsController settingsController;
        [SetUp]
        public void Setup() {
            PlayerPrefs.SetString("Language", "English");
            settingsObject = new GameObject();
            settingsController = settingsObject.AddComponent<SettingsController>();
            GameObject eventController = new GameObject();
            eventController.AddComponent<EventController>();
        }

        [UnityTest]
        [TestCase("English")]
        [TestCase("Cymraeg")]
        public void TestLanguageChange(string language) {
            string expected = "";
            switch (language) {
                case "English":
                    expected = "Current Task";
                    break;
                case "Cymraeg":
                    expected = "Tasg Gyfredol";
                    break;
            }
            PlayerPrefs.SetString("Language", language);
            settingsController.StringTrans = SettingsFunctions.SetLanguage(PlayerPrefs.GetString("Language", "Cymraeg"));
            Assert.Greater(settingsController.StringTrans.Count, 0);
            Assert.AreEqual(settingsController.TranslateString("CurrentTask"), expected);
        }

        [UnityTest]
        [TestCase(0, true)]
        [TestCase(1, true)]
        [TestCase(4, true)]
        [TestCase(4, false)]
        public void PawnNameReturnTest(int count, bool unique) {
            int expected = count;
            List<string> pawnNames = PawnFunctions.ReturnPawnNames(count, unique);
            Assert.AreEqual(expected, pawnNames.Count);
            if (unique) {
                foreach (string name in pawnNames) {
                    Assert.AreEqual(pawnNames.FindAll(x => x == name).Count, 1);
                }
            }
        }

        [UnityTest]
        public IEnumerator SaveFileReturnTest() {
            List<SaveGameItem> saveGames = SaveFunctions.ReturnSaveFiles(PlayerPrefs.GetString("saveLocation"), "date");
            Assert.IsNotNull(saveGames);
            System.DateTime date = saveGames[0].dateTime;

            // Test to see if they have been ordered correctly.
            for (int i = 1; i < saveGames.Count; i++) {
                System.DateTime newDate = saveGames[i].dateTime;
                if (newDate > date) Assert.Fail();
                date = newDate;
            }
            yield return null;
        }

        [UnityTest]
        [TestCase("Quit", "Cau'r Gêm")]
        [TestCase("SeedText", "Hedyn Map")]
        [TestCase("NameText", "Enw'r Pentref")]
        [TestCase("Food", "Bwyd")]

        public void TranslateTMPItemsTest(string variable, string translation) {
            PlayerPrefs.SetString("Language", "Cymraeg");
            settingsController.StringTrans = SettingsFunctions.SetLanguage("Cymraeg");
            GameObject textObj = new GameObject();
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.SetText(variable);
            SettingsFunctions.TranslateTMPItems(settingsController, text);
            Assert.AreEqual(text.text, translation);
        }

        [UnityTest]

        public IEnumerator DateTimeSerialize() {
            long current = System.DateTime.Now.ToFileTime();
            Debug.Log(current);
            System.DateTime currentConverted = System.DateTime.FromFileTime(current);
            Debug.Log(currentConverted);
            yield return null;
        }

    }
}