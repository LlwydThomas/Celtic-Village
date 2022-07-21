using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelManager : MonoBehaviour {
    // Start is called before the first frame update
    public NatureModel natureModel = new NatureModel();
    public GridModel gridModel = new GridModel();
    public FarmingModel farmingModel = new FarmingModel();
    public TimeModel timeModel = new TimeModel();
    [SerializeField]
    public BuildingModel buildingModel = new BuildingModel();
    public MapDataModel mapDataModel = new MapDataModel();
    public SkillModel skillModel = new SkillModel();
    public WeatherModel weatherModel = new WeatherModel();
    public TaskModel taskModel = new TaskModel();
    public ResourceModel resourceModel = new ResourceModel();
    public NPCModel nPCModel = new NPCModel();
    public ScheduledEventModel scheduledEventModel = new ScheduledEventModel();
}