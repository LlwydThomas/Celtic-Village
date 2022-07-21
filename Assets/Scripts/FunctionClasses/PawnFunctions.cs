using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public static class PawnFunctions {
    // Start is called before the first frame update

    private static List<string> vowels = new List<string>() { "A", "E", "I", "O", "U", "W", "Y" };
    public static List<Pawn> DefaultPawnListReturn(List<SkillData> skillDatas, int defLimit) {
        List<Pawn> defPawnList = new List<Pawn>();
        for (int i = 0; i < defLimit; i++) {
            PawnStatus defPawnStatus = new PawnStatus(1, 1);
            //defPawnList.Add(new Pawn("Donjon Mugabe", defPawnStatus, i));
        }

        return defPawnList;
    }

    public static Pawn CreateNewPawn(string name) {
        if (name == null || name.Length == 0) return null;
        Pawn newPawn = new Pawn(name, 21, RandomiseColours(4), new PawnStatus(1, 1), -1);
        return newPawn;
    }

    public static Color[] RandomiseColours(int count) {
        Color[] returnColours = new Color[count];
        returnColours[0] = GeneralFunctions.RandomColours(1, false) [0];
        Color[] additionalColors = GeneralFunctions.RandomColours(count - 1, true);
        for (int i = 1; i <= count - 1; i++) {
            returnColours[i] = additionalColors[i - 1];
        }
        return returnColours;
    }

    public static List<Pawn> IncreasePawnStatusLevels(List<Pawn> pawnList, int[] hungerIncrease, int[] tirednessIncrease, bool triggerEvent) {
        Debug.Log("AdjustingStatusLevels");
        foreach (Pawn pawn in pawnList) {
            // Debug.Log (pawn.id);
            // First elements in increase arrarys = passive deduction.
            PawnStatus status = pawn.pawnStatus;
            string pawnStatusDebug = "Hunger: " + status.hungerLevel + ", Tiredness: " + status.tirednessLevel;
            switch (status.currentStatus) {
                case PawnStatus.CurrentStatus.Sleeping:
                    float bedBonus;
                    if (pawn.sleepingNode != null) bedBonus = 3f;
                    else bedBonus = 1.5f;
                    status.hungerLevel = BindStatusValues(status.hungerLevel, -(hungerIncrease[0] * status.hungerModifier) / 2);
                    status.tirednessLevel = BindStatusValues(status.tirednessLevel, (tirednessIncrease[1] * status.tiredModifier * bedBonus));
                    break;
                case PawnStatus.CurrentStatus.Eating:
                    status.hungerLevel = BindStatusValues(status.hungerLevel, 0);
                    status.tirednessLevel = BindStatusValues(status.tirednessLevel, -(tirednessIncrease[0] * status.tiredModifier));
                    break;
                default:
                    status.hungerLevel = BindStatusValues(status.hungerLevel, -(hungerIncrease[0] * status.hungerModifier));
                    status.tirednessLevel = BindStatusValues(status.tirednessLevel, -(tirednessIncrease[0] * status.tiredModifier));
                    break;
            }
            pawnStatusDebug += "; newHunger: " + status.hungerLevel + ", newTired: " + status.tirednessLevel;
            //Debug.Log(pawnStatusDebug);
        }

        if (triggerEvent) EventController.TriggerEvent("pawnStatusChange");
        return pawnList;
    }

    public static float FindVillagerDeathChance(Pawn pawn) {
        float exponent = (float) (pawn.age - 50f) / 8f;
        return (Mathf.Exp(exponent)) / 8f - 0.01f;
    }

    public static string RandomPawnName() {
        List<string> pawnNames = ReturnPawnNames(2, true);
        if (pawnNames != null) {
            string connector = vowels.Contains(pawnNames[1].Substring(0, 1)) ? "Ab" : "Ap";
            return pawnNames[0] + " " + connector + " " + pawnNames[1];
        } else return null;
    }

    public static List<string> ReturnPawnNames(int count, bool unique) {
        string nameFile = Application.streamingAssetsPath + "/Strings/PawnNames.json";
        List<string> returnList = new List<string>();
        // Read the text from directly from the Json file.
        StreamReader inp_stm = new StreamReader(nameFile);
        string content = inp_stm.ReadToEnd();
        //if (content.Length > 0) Debug.Log("File has been read, but not converted");

        NameList container = JsonUtility.FromJson<NameList>(content);
        Debug.Log("Total names retrieved: " + container.stringList.Count);
        int whileLimit = container.stringList.Count;
        while (returnList.Count < count) {
            int index = Random.Range(0, container.stringList.Count);
            if (container.stringList[index] != null) {
                string name = container.stringList[index];
                if (unique) {
                    if (!returnList.Contains(name)) returnList.Add(name);
                } else returnList.Add(name);
            }
            whileLimit -= 1;
            if (whileLimit <= 0) return null;
        }
        return returnList;
    }

    public static int BindStatusValues(int currentLevel, float change) {
        int changeInt = Mathf.FloorToInt(change);
        if (currentLevel + changeInt >= 0) {
            if (currentLevel + changeInt <= 100) {
                return currentLevel + changeInt;
            } else return 100;
        } else return 0;
    }

    public static int ApplyHealthChange(PawnStatus pawnStatus, int change) {
        int resultantHealth = pawnStatus.totalHealth + change;
        resultantHealth = Mathf.Clamp(resultantHealth, 0, 100);
        pawnStatus.totalHealth = resultantHealth;
        return pawnStatus.totalHealth;
    }

    public static List<ResourceTransaction> StorageDumpTransactionReturn(List<StorageContainer> totalStorages, StorageContainer pawnStorage) {
        List<ResourceTransaction> transactions = new List<ResourceTransaction>();
        foreach (InstantiatedResource resource in pawnStorage.inventory) {
            int countRemaining = resource.count - resource.reserved;
            List<InstantiatedResource> existingStorage = StorageFunctions.StoragesWithResource(totalStorages, resource.resourceData);
            Debug.Log("PTS - Attempting to dump " + resource.resourceData.resourceName);
            // If an existing storage already contains this resource, attempt to merge the pawn's storage with the existing storage. 
            if (existingStorage != null) {
                foreach (InstantiatedResource existing in existingStorage) {
                    Debug.Log("PTS - Existing resource of type " + existing.resourceData.resourceName + " found in storage container " + existing.storageContainerID);
                    StorageContainer storageContainer = existing.storageContainer;
                    int maxCount = Mathf.FloorToInt((storageContainer.weightCapacity - storageContainer.weightFill) / existing.resourceData.weightPerItem);
                    int change = maxCount <= countRemaining ? maxCount : countRemaining;
                    ResourceTransaction resourceTransaction = new ResourceTransaction(existing, change);
                    transactions.Add(resourceTransaction);
                    countRemaining -= change;
                    if (countRemaining <= 0) break;
                }
            }

            // If the pawn still has outstanding resources, find the emptiest available storages and try to offload all the resources into these storages.
            List<StorageContainer> emptiestStorages = StorageFunctions.FindEmptiestStorages(totalStorages);
            foreach (StorageContainer storageContainer1 in emptiestStorages) {
                int maxCount = Mathf.FloorToInt((storageContainer1.weightCapacity - storageContainer1.weightFill) / resource.resourceData.weightPerItem);
                if (maxCount < 1) continue;
                int change = maxCount <= countRemaining ? maxCount : countRemaining;
                InstantiatedResource relevantResource = storageContainer1.inventory.Find(x => x.resourceData == resource.resourceData);
                if (relevantResource == null) {
                    relevantResource = new InstantiatedResource(resource.resourceData, 0);
                    relevantResource.storageContainerID = storageContainer1.id;
                    storageContainer1.inventory.Add(relevantResource);
                }

                ResourceTransaction resourceTransaction = new ResourceTransaction(relevantResource, change);
                transactions.Add(resourceTransaction);
                countRemaining -= change;
                if (countRemaining <= 0) break;
            }
        }

        return transactions;
    }
}