using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerManager : MonoBehaviour {
    // List of referencable controllers, to reduce dependencies.
    public DateController dateController;
    public NatureController natureController;
    public ResourceController resourceController;
    public SaveGameController saveGameController;
    public EventController eventController;
    public FarmingController farmingController;
    public TaskController taskController;
    public BuildingController buildingController;
    public SkillsController skillsController;
    public SettingsController settingsController;
    public WeatherController weatherController;
    public GridController gridController;
    public StorageController storageController;
    public MapController mapController;
    public PathfindingController pathfindingController;
    public LoadingBarController loadingBarController;
    public EventQueueController eventQueueController;
    public NPCController nPCController;
}