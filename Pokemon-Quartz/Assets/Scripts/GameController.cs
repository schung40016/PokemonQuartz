using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { FreeRoam, Battle, HUD, Dialog, Cutscene, Paused, Menu, Evolution}

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    [SerializeField] HUD hud;
    [SerializeField] PartyScreen partyScreen;

    GameState state;
    GameState prevState;
    GameState stateBeforeEvolution;

    public SceneDetails CurrentScene { get; private set; }
    public SceneDetails PrevScene { get; private set; }

    MenuController menuController;

    public static GameController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        menuController = GetComponent<MenuController>();

        /*
         * Disables mouse.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        */

        // Initialize Pokemon database.
        PokemonDB.Init();
        MoveDB.Init();
        ConditionsDB.Init();
        ItemDB.Init();
        QuestDB.Init();
    }

    private void Start()
    {
        
        battleSystem.OnBattleOver += EndBattle;
        playerController.ShowHud += DisplayHud;
        hud.QuittingHud += QuitHud;

        partyScreen.Init();

        DialogManager.Instance.OnShowDialog += () =>
        {
            prevState = state;
            state = GameState.Dialog;
        };

        DialogManager.Instance.OnDialogFinished += () =>
        {
            if(state == GameState.Dialog)
            {
                state = prevState;
            }
        };

        menuController.onBack += () =>
        {
            state = GameState.FreeRoam;
        };

        menuController.onMenuSelected += OnMenuSelected;

        EvolutionManager.i.OnStartEvolution += () =>
        {
            stateBeforeEvolution = state;
            state = GameState.Evolution;
        };
        EvolutionManager.i.OnCompleteEvolution += () =>
        {
            partyScreen.SetPartyData();
            state = stateBeforeEvolution;
        };
    }

    // keep the game paused while swithcing over to next scene
    public void PauseGame(bool pause) 
    {
        if (pause)
        {
            prevState = state;
            state = GameState.Paused;
        }
        else 
        {
            state = prevState;
        }
    }

    public void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<PokemonParty>();
        var wildPokemon = CurrentScene.GetComponent<MapArea>().GetRandomWildPokemon();

        var wildPokemonCopy = new Pokemon(wildPokemon.Base, wildPokemon.Level);

        battleSystem.StartBattle(playerParty, wildPokemonCopy);
    }

    TrainerController trainer;

    public void StartTrainerBattle(TrainerController trainer)
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        this.trainer = trainer;
        var playerParty = playerController.GetComponent<PokemonParty>();
        var trainerParty = trainer.GetComponent<PokemonParty>();

        battleSystem.StartTrainerBattle(playerParty, trainerParty);
    }

    public void OnEntersTrainerView(TrainerController trainer)
    {
        state = GameState.Cutscene;
        StartCoroutine(trainer.TriggerTrainerBattle(playerController));
    }

    void EndBattle(bool won)
    {
        if(trainer != null && won)
        {
            trainer.BattleLost();
            trainer = null;
        }

        partyScreen.SetPartyData();

        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);

       var playerParty = playerController.GetComponent<PokemonParty>();
        StartCoroutine(playerParty.CheckForEvolutions());
    }

    //Displays hud.
    void DisplayHud()
    {
        state = GameState.HUD;
        battleSystem.gameObject.SetActive(false);
        hud.gameObject.SetActive(true);
    }

    //Get ouf of hud.
    void QuitHud()
    {
        state = GameState.FreeRoam;
        hud.gameObject.SetActive(false);
        hud.audioSwitch = true;
    }

    //(Dis/En)ables either the player controller or battle system.
    private void Update()
    {
        if (state == GameState.FreeRoam)
        {
            playerController.HandleUpdate();

            // Initialize game menu.
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                menuController.OpenMenu();
                state = GameState.Menu;
            }
        }
        else if (state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
        else if (state == GameState.HUD)
        {
            hud.HandleUpdate();
        }
        else if (state == GameState.Dialog)
        {
            DialogManager.Instance.HandleUpdate();
        }
        else if (state == GameState.Menu)
        {
            menuController.HandleUpdate();
        }
    }

    public void SetCurrentScene(SceneDetails currScene) 
    {
        PrevScene = CurrentScene;
        CurrentScene = currScene;
    }

    void OnMenuSelected(int selectedItem)
    {
        if (selectedItem == 0)
        {
            // Resume game.
        }
        else if (selectedItem == 1)
        {
            SavingSystem.i.Save("saveSlot1");
        }
        else if (selectedItem == 2)
        {
            SavingSystem.i.Load("saveSlot1");
        }
        else if (selectedItem == 3)
        {
            // Quit Game.
            Application.Quit();
        }

        state = GameState.FreeRoam;
    }

    public PlayerController GetPlayerController()
    {
        return playerController;
    }

    public GameState State => state;

}
