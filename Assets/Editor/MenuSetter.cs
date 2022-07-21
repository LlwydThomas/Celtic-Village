using UnityEditor;
using UnityEngine;
static class ScirptableObjectItems {

    [MenuItem("Assets/Create/ScriptableObjects/Structure Data", false, 1)]
    static void StructureData() {
        var asset = ScriptableObject.CreateInstance<StructureData>();

        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        path += "/StructureData-.asset";

        ProjectWindowUtil.CreateAsset(asset, path);
    }

    [MenuItem("Assets/Create/ScriptableObjects/Animal Data", false, 1)]
    static void AnimalData() {
        var asset = ScriptableObject.CreateInstance<AnimalData>();
        /*code to preconfigure your asset*/

        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        path += "/AnimalData-.asset";

        ProjectWindowUtil.CreateAsset(asset, path);
    }

    [MenuItem("Assets/Create/ScriptableObjects/Resource Data", false, 1)]
    static void ResourceData() {
        var asset = ScriptableObject.CreateInstance<ResourceData>();
        /*code to preconfigure your asset*/

        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        path += "/ResourceData-.asset";

        ProjectWindowUtil.CreateAsset(asset, path);
    }

    [MenuItem("Assets/Create/ScriptableObjects/Flora Data", false, 1)]
    static void FloraData() {
        var asset = ScriptableObject.CreateInstance<FloraData>();
        /*code to preconfigure your asset*/

        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        path += "/FloraData-.asset";

        ProjectWindowUtil.CreateAsset(asset, path);
    }

    [MenuItem("Assets/Create/ScriptableObjects/Purpose Data", false, 1)]
    static void PurposeData() {
        var asset = ScriptableObject.CreateInstance<PurposeData>();
        /*code to preconfigure your asset*/

        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        path += "/PurposeData-.asset";

        ProjectWindowUtil.CreateAsset(asset, path);
    }

    [MenuItem("Assets/Create/ScriptableObjects/Trait Data", false, 1)]
    static void TraitData() {
        var asset = ScriptableObject.CreateInstance<TraitData>();
        /*code to preconfigure your asset*/

        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        path += "/TraitData-.asset";

        ProjectWindowUtil.CreateAsset(asset, path);
    }

    [MenuItem("Assets/Create/ScriptableObjects/Tribe Data", false, 1)]
    static void TribeData() {
        var asset = ScriptableObject.CreateInstance<TribeData>();
        /*code to preconfigure your asset*/

        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        path += "/TribeData-.asset";

        ProjectWindowUtil.CreateAsset(asset, path);
    }

    [MenuItem("Assets/Create/ScriptableObjects/Farming Data", false, 1)]
    static void FarmingData() {
        var asset = ScriptableObject.CreateInstance<FarmingData>();
        /*code to preconfigure your asset*/

        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        path += "/FarmingData-.asset";

        ProjectWindowUtil.CreateAsset(asset, path);
    }

    [MenuItem("Assets/Create/ScriptableObjects/Task Data", false, 1)]
    static void TaskData() {
        var asset = ScriptableObject.CreateInstance<TaskData>();
        /*code to preconfigure your asset*/

        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        path += "/TaskData-.asset";

        ProjectWindowUtil.CreateAsset(asset, path);
    }

    [MenuItem("Assets/Create/ScriptableObjects/Season Data", false, 1)]
    static void SeasonData() {
        var asset = ScriptableObject.CreateInstance<SeasonData>();
        /*code to preconfigure your asset*/

        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        path += "/SeasonData-.asset";

        ProjectWindowUtil.CreateAsset(asset, path);
    }

    [MenuItem("Assets/Create/ScriptableObjects/Scheduled Event", false, 1)]
    static void ScheduledEvent() {
        var asset = ScriptableObject.CreateInstance<ScheduledEvent>();
        /*code to preconfigure your asset*/

        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        path += "/ScheduledEvent-.asset";

        ProjectWindowUtil.CreateAsset(asset, path);
    }

    [MenuItem("Assets/Create/ScriptableObjects/Crafting Data", false, 1)]
    static void CraftingData() {
        var asset = ScriptableObject.CreateInstance<CraftingData>();
        /*code to preconfigure your asset*/

        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        path += "/CraftingData-.asset";

        ProjectWindowUtil.CreateAsset(asset, path);
    }

    [MenuItem("Assets/Create/ScriptableObjects/Fish Data", false, 1)]
    static void FishData() {
        var asset = ScriptableObject.CreateInstance<FishData>();
        /*code to preconfigure your asset*/

        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        path += "/FishData-.asset";

        ProjectWindowUtil.CreateAsset(asset, path);
    }

    [MenuItem("Assets/Create/ScriptableObjects/DifficultySettings Data", false, 1)]
    static void DifficultySettings() {
        var asset = ScriptableObject.CreateInstance<DifficultySettings>();
        /*code to preconfigure your asset*/

        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        path += "/DifficultySettings-.asset";

        ProjectWindowUtil.CreateAsset(asset, path);
    }
}

// Menu initialiser for data lists.
static class ScirptableObjectLists {

    [MenuItem("Assets/Create/ScriptableObjectList/StructureData List", false, 1)]
    static void StructureData() {
        var asset = ScriptableObject.CreateInstance<StructureDataList>();
        /*code to preconfigure your asset*/

        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        path += "/StructureDataList.asset";

        ProjectWindowUtil.CreateAsset(asset, path);
    }

    [MenuItem("Assets/Create/ScriptableObjectList/Animal List", false, 1)]
    static void AnimalData() {
        var asset = ScriptableObject.CreateInstance<AnimalDataList>();
        /*code to preconfigure your asset*/

        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        path += "/AnimalList.asset";

        ProjectWindowUtil.CreateAsset(asset, path);
    }

    [MenuItem("Assets/Create/ScriptableObjectList/ResourceData List", false, 1)]
    static void ResourceData() {
        var asset = ScriptableObject.CreateInstance<ResourceDataList>();
        /*code to preconfigure your asset*/

        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        path += "/ResourceDataList.asset";

        ProjectWindowUtil.CreateAsset(asset, path);
    }

    [MenuItem("Assets/Create/ScriptableObjectList/FloraData List", false, 1)]
    static void FloraData() {
        var asset = ScriptableObject.CreateInstance<FloraDataList>();
        /*code to preconfigure your asset*/

        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        path += "/FloraDataList.asset";

        ProjectWindowUtil.CreateAsset(asset, path);
    }

    [MenuItem("Assets/Create/ScriptableObjectList/PurposeData List", false, 1)]
    static void PurposeData() {
        var asset = ScriptableObject.CreateInstance<PurposeDataList>();
        /*code to preconfigure your asset*/

        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        path += "/PurposeDataList.asset";

        ProjectWindowUtil.CreateAsset(asset, path);
    }

    [MenuItem("Assets/Create/ScriptableObjectList/TraitData List", false, 1)]
    static void TraitData() {
        var asset = ScriptableObject.CreateInstance<TraitDataList>();
        /*code to preconfigure your asset*/

        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        path += "/TraitDataList.asset";

        ProjectWindowUtil.CreateAsset(asset, path);
    }

    [MenuItem("Assets/Create/ScriptableObjectList/FarmingData List", false, 1)]
    static void FarmingData() {
        var asset = ScriptableObject.CreateInstance<FarmingDataList>();
        /*code to preconfigure your asset*/

        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        path += "/FarmingDataList.asset";

        ProjectWindowUtil.CreateAsset(asset, path);
    }

    [MenuItem("Assets/Create/ScriptableObjectList/TaskData List", false, 1)]
    static void TaskData() {
        var asset = ScriptableObject.CreateInstance<TaskDataList>();
        /*code to preconfigure your asset*/

        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        path += "/TaskDataList.asset";

        ProjectWindowUtil.CreateAsset(asset, path);
    }

    [MenuItem("Assets/Create/ScriptableObjectList/SeasonData List", false, 1)]
    static void SeasonData() {
        var asset = ScriptableObject.CreateInstance<SeasonDataList>();
        /*code to preconfigure your asset*/

        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        path += "/SeasonDataList.asset";

        ProjectWindowUtil.CreateAsset(asset, path);
    }

    [MenuItem("Assets/Create/ScriptableObjectList/ScheduledEvent List", false, 1)]
    static void ScheduledEvent() {
        var asset = ScriptableObject.CreateInstance<ScheduledEventList>();
        /*code to preconfigure your asset*/

        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        path += "/ScheduledEventList.asset";

        ProjectWindowUtil.CreateAsset(asset, path);
    }

    [MenuItem("Assets/Create/ScriptableObjectList/CraftingData List", false, 1)]
    static void CraftingData() {
        var asset = ScriptableObject.CreateInstance<CraftingDataList>();
        /*code to preconfigure your asset*/
        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        path += "/CraftingDataList.asset";

        ProjectWindowUtil.CreateAsset(asset, path);
    }

    [MenuItem("Assets/Create/ScriptableObjectList/FishData List", false, 1)]
    static void FishData() {
        var asset = ScriptableObject.CreateInstance<FishDataList>();
        /*code to preconfigure your asset*/

        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        path += "/FishDataList.asset";

        ProjectWindowUtil.CreateAsset(asset, path);
    }

    [MenuItem("Assets/Create/ScriptableObjectList/DifficultySettings List", false, 1)]
    static void DifficultySettings() {
        var asset = ScriptableObject.CreateInstance<DifficultySettingsList>();
        /*code to preconfigure your asset*/

        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        path += "/DifficultySettingsList.asset";

        ProjectWindowUtil.CreateAsset(asset, path);
    }
}

public class uGUITools : MonoBehaviour {
    [MenuItem("uGUI/Anchors to Corners %[")]
    static void AnchorsToCorners() {
        RectTransform t = Selection.activeTransform as RectTransform;
        RectTransform pt = Selection.activeTransform.parent as RectTransform;

        if (t == null || pt == null) return;

        Vector2 newAnchorsMin = new Vector2(t.anchorMin.x + t.offsetMin.x / pt.rect.width,
            t.anchorMin.y + t.offsetMin.y / pt.rect.height);
        Vector2 newAnchorsMax = new Vector2(t.anchorMax.x + t.offsetMax.x / pt.rect.width,
            t.anchorMax.y + t.offsetMax.y / pt.rect.height);

        t.anchorMin = newAnchorsMin;
        t.anchorMax = newAnchorsMax;
        t.offsetMin = t.offsetMax = new Vector2(0, 0);
    }

    [MenuItem("uGUI/Corners to Anchors %]")]
    static void CornersToAnchors() {
        RectTransform t = Selection.activeTransform as RectTransform;

        if (t == null) return;

        t.offsetMin = t.offsetMax = new Vector2(0, 0);
    }
}