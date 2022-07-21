using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewManager : MonoBehaviour {
    public BuildingDialogueView buildingDialogueView;
    public ObjectSelectView objectSelectView;
    public SpawnPrefabView spawnPrefabView;
    public InGameSaveLoad inGameSaveLoad;
    public CommandInteraction commandInteraction;
    public ConfirmationView confirmationView;
    public DisplayPawnView displayPawnView;
    public DisplayResourcesView displayResourcesView;

    public FarmingDialogueView farmingDialogueView;
    public FPSTrack fPSTrack;
    public GeneralCommandsView generalCommandsView;
    public GeneralCompiler generalCompiler;
    public HandleSideMenu handleSideMenu;
    public IGMView iGMView;
    public InventoryDisplayView inventoryDisplayView;
    public KeyboardShortcuts keyboardShortcuts;
    public MapInputHandler mapInputHandler;
    public NewGameCompiler newGameCompiler;

    public ObjectInfoView objectInfoView;
    public ParticleView particleView;
    public PawnDisplayHandler pawnDisplayHandler;
    public PawnListDisplay pawnListDisplay;
    public RectSelectAlt rectSelectAlt;
    public SettingsCompiler settingsCompiler;
    public SideMenuButtonsView sideMenuButtonsView;
    public ToolTipHandler toolTipHandler;
    public DateDisplayView dateDisplayView;
    public TradingItemView tradingItemView;
    public TradingDialogueView tradingDialogueView;
    public WeatherDisplayView weatherDisplayView;
    public WorkerPawnView workerPawnView;

    public WelcomeDialogueView welcomeDialogueView;

}