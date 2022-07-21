using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public static class GeneralFunctions {
    // Start is called before the first frame update
    public static Color greenSwatch = new Color(0.08f, 0.34f, 0.075f);
    public static Color blackBackground = new Color(0, 0, 0, 0.4f);
    public static TextMesh GenerateWorldText(Transform parent, string text, Vector3 localPosition, int fontSize, Color lliw, TextAnchor textAnchor, TextAlignment textAlignment, int sortingOrder) {
        //Generate a text object in world space
        GameObject textObject = new GameObject("WorldText", typeof(TextMesh));
        Transform transform = textObject.transform;
        transform.SetParent(parent, false);
        transform.localPosition = localPosition;
        TextMesh textMesh = textObject.GetComponent<TextMesh>();
        textMesh.anchor = textAnchor;
        textMesh.alignment = textAlignment;
        textMesh.text = text;
        textMesh.fontSize = fontSize;
        textMesh.color = lliw;
        textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
        return textMesh;
    }

    public static GameObject GenerateEmptyCollider(Vector3 size, Vector3 position) {
        GameObject prefab = Resources.Load<GameObject>("EmptyCollider");
        GameObject returnObj = GameObject.Instantiate(prefab, position, Quaternion.identity);
        returnObj.transform.localScale = size;
        return returnObj;
    }

    public static LineRenderer DrawConnectingLine(Vector3 start, Vector3 end, Material material, Transform parent) {
        GameObject myLine = new GameObject();
        myLine.transform.parent = parent;
        myLine.transform.position = start;

        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = material;
        lr.startColor = Color.white;
        lr.endColor = Color.white;
        lr.startWidth = 0.2f;
        lr.endWidth = 0.2f;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.sortingLayerName = "AlwaysOnTop";
        return lr;
    }

    public static Color[] RandomColours(int count, bool random) {
        Color[] possibleColours = new Color[] {
            new Color(0.82f, 0.64f, 0.43f, 1),
            new Color(0.284f, 0.213f, 0.0f, 1),
        };
        Color[] returnColours = new Color[count];
        for (int i = 0; i < count; i++) {
            Color randColour;
            if (random) randColour = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
            else randColour = possibleColours[Random.Range(0, possibleColours.Length)];
            returnColours[i] = randColour;
        }
        return returnColours;
    }

    /***************************************************************************************
     *    Title: C# Fisher-Yates Shuffle: Generic Method
     *    Author: TheDeveloperBlog
     *    Date Accessed: 13/11/2021
     *    Code version: 1.0
     *    Availability: https://thedeveloperblog.com/fisher-yates-shuffle
     *
     ***************************************************************************************/

    public static List<T> FisherYatesShuffle<T>(List<T> list, bool copy = true) {
        List<T> copiedList;
        if (copy) copiedList = new List<T>(list);
        else copiedList = list;

        System.Random random = new System.Random();
        int listCount = list.Count;
        for (int i = 0; i < listCount; i++) {
            int r = i + (int) (random.NextDouble() * (listCount - i));
            T t = copiedList[r];
            copiedList[r] = copiedList[i];
            copiedList[i] = t;
        }
        return copiedList;
    }

    // End Citation.

    public static float[] NormaliseProbabilities(float[] floatArray) {
        float[] returns = new float[floatArray.Length];
        float totalPercentage = 0;
        foreach (float val in floatArray) {
            totalPercentage += val;
        }
        int totalAvailableEvents = returns.Length;
        float chanceFloor = 0;

        for (int i = 0; i < floatArray.Length; i++) {
            float offsetPercentage = floatArray[i] / totalPercentage;
            returns[i] = offsetPercentage;
        }
        return returns;
    }

    public static int PickRandomValueFromChanceArray(float[] baseProbabilities, float random) {
        int count = baseProbabilities.Length;
        float[] chanceArray = NormaliseProbabilities(baseProbabilities);
        float totalProbability = 0;
        for (int i = 0; i < chanceArray.Length; i++) {
            float currentProbability = chanceArray[i];
            Debug.Log("Random: " + random + " current: " + currentProbability + " total: " + totalProbability);
            if (random >= totalProbability && random <= totalProbability + currentProbability) {
                return i;
            }
            totalProbability += currentProbability;
        }
        return -1;
    }

    public static bool CompareConditions(ComparisonValue comparisonValue, int requiredValue, int currentValue) {
        switch (comparisonValue) {
            case ComparisonValue.Equal:
                if (requiredValue == currentValue) return true;
                break;
            case ComparisonValue.Less:
                if (requiredValue >= currentValue) return true;
                break;
            case ComparisonValue.Greater:
                if (requiredValue <= currentValue) return true;
                break;
        }
        return false;
    }

    public static List<RequiredResources> CopyResourceList(List<RequiredResources> listToCopy) {
        List<RequiredResources> returnList = new List<RequiredResources>();
        foreach (RequiredResources res in listToCopy) {
            returnList.Add(new RequiredResources(res.resource, res.count));
        }

        return returnList;
    }

    public static List<RequiredResources> CopyResourceList(List<InstantiatedResource> resources) {
        List<RequiredResources> returnList = new List<RequiredResources>();
        foreach (InstantiatedResource resource in resources) {
            returnList.Add(new RequiredResources(resource.resourceData, resource.count));
        }
        return returnList;
    }

    public static void SetExpansionSize(ExpansionButtonView expansionButtonView, int count, float itemSize, float sizeOverride = -1) {
        // Size of expansion button is set to the height of the button itself and the size of the resultant list.
        float size = 70 + 5 + (count * (itemSize + 5));
        Debug.Log("DRV - Setting expansion size to " + size + " due to res count of " + count);
        if (sizeOverride != -1) size = 70;
        expansionButtonView.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
    }

    public static void ResizeExpansionButton(ExpansionButtonView expansionButtonView, int count, float itemSize) {
        if (expansionButtonView == null) return;
        if (expansionButtonView.resultantList.activeSelf) {
            SetExpansionSize(expansionButtonView, count, itemSize);
        } else SetExpansionSize(expansionButtonView, count, itemSize, 90f);
    }

    public static void SetContentHeight(RectTransform rect, float size, RectTransform[] forceRebuild) {
        if (forceRebuild != null)
            foreach (RectTransform game in forceRebuild) LayoutRebuilder.ForceRebuildLayoutImmediate(game);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
        //rect.LeanSetLocalPosY((-size / 2) - 5);
    }

    public static void FormatWorldIcon(GameObject prefab, Vector3 position, Transform parent, Sprite icon) {
        GameObject iconObject = GameObject.Instantiate(prefab, position, Quaternion.identity, parent);
        iconObject.GetComponent<SpriteRenderer>().sprite = icon;
    }

    public static List<RequiredResources> CalculatePotentialResources(List<ProbableRequiredResource> potentialRes) {
        List<RequiredResources> requiredResources = new List<RequiredResources>();
        foreach (ProbableRequiredResource probable in potentialRes) {
            float randomRoll = Random.Range(0f, 1f);
            if (randomRoll < probable.probability) requiredResources.Add(probable.requiredResource);
        }
        return requiredResources;
    }

    public static string DescribeCurrentTask(TaskGroup task, SettingsController settings, TaskData taskData) {
        if (task == null || taskData == null) return settings.TranslateString("NoCurrentTask");
        string translateString = settings.TranslateString(taskData.taskDescription);
        string insertArgs = null;
        switch (taskData.ID) {
            case 1:
                // Flora Destruction.
                TaskGroup.FloraDestructionTask floraDestructionTask = task as TaskGroup.FloraDestructionTask;
                insertArgs = settings.TranslateString(floraDestructionTask.flora.uniqueType);
                break;
            case 9:
            case 2:
                // Flora Creation Base.
                TaskGroup.FloraCreationTask floraCreationTask = task as TaskGroup.FloraCreationTask;
                insertArgs = settings.TranslateString(floraCreationTask.flora.uniqueType);
                break;
            case 3:
                // Eating
                TaskGroup.ConsumeFoodTask consumeFoodTask = task as TaskGroup.ConsumeFoodTask;
                insertArgs = settings.TranslateString(consumeFoodTask.foodItem.resourceName);
                break;
            case 4:
                //sleeping
                break;
            case 5:
                //mining
                TaskGroup.MiningTask miningTask = task as TaskGroup.MiningTask;
                insertArgs = settings.TranslateString(miningTask.miningOutput.resourceName);
                break;
            case 6:
                //transferInvetory.
                TaskGroup.TransferInventoryTask transferInventoryTask = task as TaskGroup.TransferInventoryTask;
                ResourceData movingItem = transferInventoryTask.transferredResource;
                insertArgs = settings.TranslateString(movingItem.resourceName);
                break;
            case 7:
                //crafting
                TaskGroup.CraftingTask craftingTask = task as TaskGroup.CraftingTask;
                ResourceData mainOutput = craftingTask.recipeData.outputs[0].resource;
                insertArgs = settings.TranslateString(mainOutput.resourceName);
                break;
            case 8:
                //fishing
                TaskGroup.FishingTask fishingTask = task as TaskGroup.FishingTask;
                insertArgs = settings.TranslateString(fishingTask.currentFish.fishName);
                break;
            case 10:
                //slay animal
                TaskGroup.SlayAnimalTask slayAnimal = task as TaskGroup.SlayAnimalTask;
                insertArgs = settings.TranslateString(slayAnimal.targetAnimal.animalData.uniqueName);
                break;
        }

        if (insertArgs != null) {
            return string.Format(translateString, insertArgs);
        } else return translateString;
    }

    public static void SetExpansionsMaxFont(List<ExpansionButtonView> expansions) {
        List<TextMeshProUGUI> texts = new List<TextMeshProUGUI>();
        foreach (ExpansionButtonView expansion in expansions) texts.Add(expansion.description);
        float maxFont = FindMinFont(texts);
        foreach (ExpansionButtonView expansion1 in expansions) expansion1.description.fontSizeMax = maxFont;
    }

    public static void SetExpansionsMaxFont(List<ResourceDisplayItem> resourceDisplays) {
        List<ExpansionButtonView> expansions = new List<ExpansionButtonView>();
        foreach (ResourceDisplayItem resource in resourceDisplays) expansions.Add(resource.expansionButton);
        SetExpansionsMaxFont(expansions);
    }

    public static float FindMinFont(List<TextMeshProUGUI> textItems) {
        float minFont = 10000f;
        foreach (TextMeshProUGUI text in textItems) minFont = text.fontSize < minFont ? text.fontSize : minFont;
        Debug.Log("GF - Min Font found: " + minFont);
        return minFont;
    }
    // End of citation.
}