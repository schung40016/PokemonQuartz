using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, PartyScreen, AboutToUse, MoveToForget, BattleOver, Bag }

public enum BattleAction { Move, SwitchPokemon, UseItem, Run }

public class BattleSystem : MonoBehaviour
{
    public AudioSource selectSound;
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogueBox dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;
    [SerializeField] GameObject pokeballSprite;
    [SerializeField] MoveSelectionUI moveSelectionUI;
    [SerializeField] InventoryUI inventoryUI;

    public event Action<bool> OnBattleOver;

    // Handles what screen to display during battle or whose turn it is.
    BattleState state;

    // Handles the selection cursor for user during battle.
    int currentAction;
    int currentMove;
    bool aboutToUseChoice = true;

    // Stores pokemon party for battling.
    PokemonParty playerParty;
    PokemonParty trainerParty;

    // Stores the wild pokemon encountered.
    Pokemon wildPokemon;

    bool isTrainerBattle = false;
    PlayerController player;
    TrainerController trainer;

    int escapeAttempts;
    MoveBase moveToLearn;

    // Start wild battle.
    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        player = playerParty.GetComponent<PlayerController>();
        isTrainerBattle = false;
        
        StartCoroutine( SetUpBattle() );
    }

    // Starts trainer battle.
    public void StartTrainerBattle(PokemonParty playerParty, PokemonParty trainerParty)
    {
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;
        isTrainerBattle = true;

        player = playerParty.GetComponent<PlayerController>();
        trainer = trainerParty.GetComponent<TrainerController>();       // <-- Not retrieving pokemons

        StartCoroutine(SetUpBattle());
    }
     
    // Coroutine function for animating and setting up battle huds.
    public IEnumerator SetUpBattle()
    {
        playerUnit.Clear();
        enemyUnit.Clear();

        if(!isTrainerBattle)
        {
            // Wild battle.
            playerUnit.Setup(playerParty.GetHealthyPokemon());
            enemyUnit.Setup(wildPokemon);

            // Passes the user's pokemon's move to the program.
            dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);

            // Initialize code to type in the text one by one.
            yield return dialogBox.TypeDialog($"A wild {enemyUnit.Pokemon.Base.Name} appeared.");
            // Wait for 1 second for the animation to finish.
            yield return new WaitForSeconds(1f);
        }
        else
        {
            // Trainer battle.

            // Display the trainer/player sprites.
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);

            playerImage.sprite = player.Sprite;
            trainerImage.sprite = trainer.Sprite;

            yield return dialogBox.TypeDialog($"{trainer.Name} wants to battle!");

            // Send out both party's first pokemon.
            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var enemyPokemon = trainerParty.GetHealthyPokemon();

            if(enemyUnit == null)
            {
                Debug.Log("failure");
            }
            enemyUnit.Setup(enemyPokemon);            
            yield return dialogBox.TypeDialog($"{trainer.Name} sends out {enemyPokemon.Base.Name}");

            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerPokemon = playerParty.GetHealthyPokemon();
            playerUnit.Setup(playerPokemon);
            yield return dialogBox.TypeDialog($"Go {playerPokemon.Base.name}!");

            // Pass moves for player party.
            dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
        }

        // Initialize escape counter for escaping.
        escapeAttempts = 0;

        // Initalize party tab for player.
        partyScreen.Init();

        ActionSelection();
    }

    // End battle.
    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.Pokemons.ForEach(p => p.OnBattleOver());
        playerUnit.Hud.ClearData();
        enemyUnit.Hud.ClearData();
        OnBattleOver(won);
    }


    // Phase where play can either run or attack.
    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.SetDialog("Select an action.");
        dialogBox.EnableActionSelector(true);
    }

    void OpenBag()
    {
        state = BattleState.Bag; 
        inventoryUI.gameObject.SetActive(true);
    }

    // Displays player's party during battle.
    void OpenPartyScreen()
    {
        partyScreen.CalledFrom = state;

        state = BattleState.PartyScreen;
        partyScreen.gameObject.SetActive(true);
    }

    // Allows player to choose an action.
    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator AboutToUse(Pokemon newPokemon)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"{trainer.Name} is about to use {newPokemon.Base.Name}. Do you wish to change pokemons?");
        state = BattleState.AboutToUse;
        dialogBox.EnableChoiceBox(true);
    }

    // Handles the move selection ui menu.
    IEnumerator ChooseMoveToForget(Pokemon pokemon, MoveBase newMove)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"Choose a move you wish to forget");
        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(pokemon.Moves.Select(x => x.Base).ToList(), newMove);
        moveToLearn = newMove;

        state = BattleState.MoveToForget;
    }

    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.RunningTurn;
        if(playerAction == BattleAction.Move)
        {
            // Gets move that is highlighted blue.
            playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[currentMove];
            
            // Decide move for enemy.
            enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.GetRandomMove();

            int playerMovePriority = playerUnit.Pokemon.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Pokemon.CurrentMove.Base.Priority;

            // Check who moves first!
            // Check priority first.
            bool playerGoesFirst = true;

            if( enemyMovePriority > playerMovePriority )
            {
                playerGoesFirst = false;
            }
            else if ( enemyMovePriority == playerMovePriority )
            {
                playerGoesFirst = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;
            }

            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;

            var secondPokemon = secondUnit.Pokemon;

            // First turn.
            yield return RunMove(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
            yield return RunAfterTurn(firstUnit);

            // Check if the battle is over.
            if(state == BattleState.BattleOver)
            {
                yield break;
            }

            if( secondPokemon.HP > 0)
            {
                // Second turn.
                yield return RunMove(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
                yield return RunAfterTurn(secondUnit);

                // Check if the battle is over.
                if (state == BattleState.BattleOver)
                {
                    yield break;
                }
            }
        }
        else
        {
            // Player switches pokemon.
            if (playerAction == BattleAction.SwitchPokemon)
            {
                var selectedPokemon = partyScreen.SelectedMember;
                state = BattleState.Busy;
                yield return SwitchPokemon(selectedPokemon);
            }
            else if (playerAction == BattleAction.UseItem)
            {
                // This is handled from item screen, so do nothing and skip to enemy move.
                dialogBox.EnableActionSelector(false);
            }
            else if (playerAction == BattleAction.Run)
            {
                yield return TryToEscape();
            }

            var enemyMove = enemyUnit.Pokemon.GetRandomMove();

            // Second turn.
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);

            if (state == BattleState.BattleOver)
            {
                yield break;
            }
        }

        if( state != BattleState.BattleOver)
        {
            ActionSelection();
        }
    }

    // Handles attack
    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        // Check whether the pokemon can move or not.
        bool canRunMove = sourceUnit.Pokemon.OnBeforeMove();
        if( !canRunMove )
        {
            yield return ShowStatusChanges(sourceUnit.Pokemon);

            yield return sourceUnit.Hud.WaitForHPUpdate(); 

            yield break;
        }
        // Display condition through hud.
        yield return ShowStatusChanges(sourceUnit.Pokemon);

        move.PP--;

        yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.name} used {move.Base.name}");

        if (CheckIfMoveHits(move, sourceUnit.Pokemon, targetUnit.Pokemon))
        {
            sourceUnit.PlayAttackAnimation();
            yield return new WaitForSeconds(1f);

            targetUnit.PlayHitAnimation();

            // Check if status condition.
            if (move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Pokemon, targetUnit.Pokemon, move.Base.Target);
            }
            else
            {
                var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
                yield return targetUnit.Hud.WaitForHPUpdate();
                yield return ShowDamageDetails(damageDetails);
            }

            // Handles if the moves has secondary effects.
            if(move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && targetUnit.Pokemon.HP > 0)
            {
                foreach (var secondary in move.Base.Secondaries )
                {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if( rnd <= secondary.Chance)
                    {
                        yield return RunMoveEffects(secondary, sourceUnit.Pokemon, targetUnit.Pokemon, secondary.Target);
                    }
                }
            }

            if (targetUnit.Pokemon.HP <= 0)
            {
                yield return HandlePokemonFainted(targetUnit);
            }
        }
        else
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.name}'s missed its attack!");
        }
    }

    IEnumerator RunMoveEffects(MoveEffects effects, Pokemon source, Pokemon target, MoveTarget moveTarget)
    {
        // Stat boosting.
        if (effects.Boosts != null)
        {
            if (moveTarget == MoveTarget.Self)
            {
                source.ApplyBoosts(effects.Boosts);
            }
            else
            {
                target.ApplyBoosts(effects.Boosts);
            }
        }
        // Status conditioning.
        if( effects.Status != ConditionID.none)
        {
            target.SetStatus(effects.Status);
        }

        // Volatile Status conditioning.
        if (effects.VolatileStatus != ConditionID.none)
        {
            target.SetVolatileStatus(effects.VolatileStatus);
        }

        // Display status messages.
        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    // Executes status.
    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        if(state == BattleState.BattleOver)
        {
            yield break;
        }

        yield return new WaitUntil(() => state == BattleState.RunningTurn);

        sourceUnit.Pokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Pokemon);
        yield return sourceUnit.Hud.WaitForHPUpdate();
        if (sourceUnit.Pokemon.HP <= 0)
        {
            yield return HandlePokemonFainted(sourceUnit);
            yield return new WaitUntil(() => state == BattleState.RunningTurn);
        }
    }

    //Checks if pokemon move hits.
    bool CheckIfMoveHits(Move move, Pokemon source, Pokemon target)
    {
        // If source pokemon has a always hitting move.
        if( move.Base.AlwaysHits)
        {
            return true;
        }

        float moveAccuracy = move.Base.Accuracy;

        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = source.StatBoosts[Stat.Evasion];

        // Keeps track of stat numbs for accuracy and evasion.
        var boostValues = new float[] { 1f, 4f/3f, 5f/3f, 2f, 7f/3f, 8f/3f, 3f };

        // Boost accuracy.
        if( accuracy > 0)
        {
            moveAccuracy *= boostValues[accuracy];
        }
        // Decrease accuracy.
        else
        {
            moveAccuracy /= boostValues[-accuracy];
        }

        // Boost evasion.
        if (evasion > 0)
        {
            moveAccuracy *= boostValues[evasion];
        }
        // Decrease evasion.
        else
        {
            moveAccuracy /= boostValues[-evasion];
        }

        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
    }

    // For handling pokemon deaths and calculate exp gain.
    IEnumerator HandlePokemonFainted(BattleUnit faintedUnit)
    {
        yield return dialogBox.TypeDialog($"{faintedUnit.Pokemon.Base.name} fainted.");
        faintedUnit.PlayFaintAnimation();
        yield return new WaitForSeconds(2f);

        if (!faintedUnit.IsPlayerUnit)
        {
            // Exp Gain
            int expYield = faintedUnit.Pokemon.Base.ExpYield;
            int enemyLevel = faintedUnit.Pokemon.Level;
            float trainerBonus = (isTrainerBattle) ? 1.5f : 1f;

            int expGain = Mathf.FloorToInt((expYield * enemyLevel * trainerBonus) / 7);
            playerUnit.Pokemon.Exp += expGain;
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} gained {expGain} exp!");
            yield return playerUnit.Hud.SetExpSmooth();

            // Check level up, use while loop in cases of multiple level ups.
            while (playerUnit.Pokemon.CheckForLevelUp())
            {
                playerUnit.Hud.SetLevel(); 
                yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} grew to level {playerUnit.Pokemon.Level}");

                // Try to learn a new move.
                var newMove = playerUnit.Pokemon.GetLearnableMoveAtCurrentLevel();

                if (newMove != null)
                {
                    if (playerUnit.Pokemon.Moves.Count < PokemonBase.MaximumNumOfMoves)
                    {
                        playerUnit.Pokemon.LearnMove(newMove);
                        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} learned {newMove.Base.Name}");
                        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
                    }
                    else
                    {
                        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} is trying to learn {newMove.Base.Name}");
                        yield return dialogBox.TypeDialog($"But it can not now more than {PokemonBase.MaximumNumOfMoves} moves");
                        yield return ChooseMoveToForget(playerUnit.Pokemon, newMove.Base);
                        yield return new WaitUntil(() => state != BattleState.MoveToForget);
                        yield return new WaitForSeconds(2f);
                    }
                }

                // Take care of surplus exp.
                yield return playerUnit.Hud.SetExpSmooth(true);
            }

            yield return new WaitForSeconds(1f);
        }

        // See if target pokemon fainted.
        CheckForBattleOver(faintedUnit);
    }

    // Shows status changes.
    IEnumerator ShowStatusChanges(Pokemon pokemon)
    {
        while(pokemon.StatusChanges.Count > 0)
        {
            var message = pokemon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    // Checks which pokemon fainted.
    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        // Player fainted.
        if( faintedUnit.IsPlayerUnit )
        {
            // Used for getting other pokemon from player party.
            var nextPokemon = playerParty.GetHealthyPokemon();

            // Check whether player has any pokemon left.
            if (nextPokemon != null)
            {
                OpenPartyScreen();
            }
            // Else end battle if all pokemons are dead.
            else
            {
                BattleOver(false);
            }
        }
        // Enemy fainted.
        else
        {
            if(!isTrainerBattle)
            {
                BattleOver(true);
            }
            else
            {
                var nextPokemon = trainerParty.GetHealthyPokemon();
                if(nextPokemon != null)
                {
                    // Send out the next pokemon.
                    StartCoroutine(AboutToUse(nextPokemon));
                }
                else
                {
                    BattleOver(true);
                }
            }
        }
    }

    // Displays in dialogue box what happened.
    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if(damageDetails.Critical > 1f)
        {
            yield return dialogBox.TypeDialog("A critical hit!");
        }    

        if(damageDetails.TypeEffectiveness > 1f)
        {
            yield return dialogBox.TypeDialog("It was super effective!");
        }
        else if(damageDetails.TypeEffectiveness < 1f)
        {
            yield return dialogBox.TypeDialog("It was not very effective.");
        }
    }

    public void HandleUpdate()
    {
        if ( state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        }
        else if (state == BattleState.Bag)
        {
            Action onBack = () =>
            {
                inventoryUI.gameObject.SetActive(false);
                state = BattleState.ActionSelection;
            };

            Action<ItemBase> onItemUsed = (ItemBase usedItem) =>
            {
                StartCoroutine(OnItemUsed(usedItem));
            };

            inventoryUI.HandleUpdate(onBack, onItemUsed);
        }
        else if (state == BattleState.AboutToUse)
        {
            HandleAboutToUse();
        }
        else if (state == BattleState.MoveToForget)
        {
            Action<int> onMoveSelected = (moveIndex) =>
            {
                moveSelectionUI.gameObject.SetActive(false);
                
                // Player does't want to learn the new move.
                if (moveIndex == PokemonBase.MaximumNumOfMoves)
                {
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.name} did not learn {moveToLearn.Name}"));
                }
                // Pokemon forget and learn a new move.
                else
                {
                    var selectedMove = playerUnit.Pokemon.Moves[moveIndex].Base;
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.name} forgot {selectedMove.Name} and learned {moveToLearn.Name}"));
                    playerUnit.Pokemon.Moves[moveIndex] = new Move(moveToLearn);
                }

                moveToLearn = null;
                state = BattleState.RunningTurn;
            };

            moveSelectionUI.HandleMoveSelection(onMoveSelected);
        }
    }

    // Handles all arrow key movement from player.
    void HandleActionSelection()
    {
        if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentAction;
        }
        else if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentAction;
        }
        else if(Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentAction += 2;
        }
        else if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentAction -= 2;
        }

        currentAction = Mathf.Clamp(currentAction, 0, 3);

        dialogBox.UpdateActionSelection(currentAction);

        if(Input.GetKeyDown(KeyCode.Z))
        {
            // Player selected fight.
            if (currentAction == 0)
            {
                selectSound.Play();
                MoveSelection();
            }
            // Player selected bag.
            else if(currentAction == 1)
            {
                OpenBag();
            }
            // Player selected Pokemon.
            else if (currentAction == 2)
            {
                OpenPartyScreen();
            }
            // Player selected run.
            else if (currentAction == 3)
            {
                StartCoroutine(RunTurns(BattleAction.Run));
            }
        }
    }

    // Player control for move screen.
    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentMove += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentMove -= 2;
        }

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Pokemon.Moves.Count - 1);

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        if(Input.GetKeyDown(KeyCode.Z))
        {
            var move = playerUnit.Pokemon.Moves[currentMove];
            if( move.PP == 0 )
            {
                return;
            }

            selectSound.Play();
            dialogBox.EnableMoveSelector(false);        // Stop player from doing something so that game could load.
            dialogBox.EnableDialogText(true);           // Loads text.
            StartCoroutine(RunTurns(BattleAction.Move));
        }

        if(Input.GetKeyDown(KeyCode.X))
        {
            selectSound.Play();
            dialogBox.EnableMoveSelector(false);        // Stop player from doing something so that game could load.
            dialogBox.EnableDialogText(true);           // Loads text.
            ActionSelection();
        }
    }

    // Handle party screen.
    void HandlePartySelection()
    {
        Action onSelected = () =>
        {
            var selectedMember = partyScreen.SelectedMember;

            // Disable player from selecting fainted pokemon.
            if (selectedMember.HP <= 0)
            {
                partyScreen.SetMessageText("Less you got a revive, you can't use a fainted pokemon.");
                return;
            }
            if (selectedMember == playerUnit.Pokemon)
            {
                partyScreen.SetMessageText("You're going to send out the same pokemon?");
                return;
            }

            // Disable party screen.
            partyScreen.gameObject.SetActive(false);

            if (partyScreen.CalledFrom == BattleState.ActionSelection)
            {
                StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
            }
            else
            {
                state = BattleState.Busy;

                // Show pokemon switch.
                bool isTrainerAboutToUse = partyScreen.CalledFrom == BattleState.AboutToUse;
                StartCoroutine(SwitchPokemon(selectedMember, isTrainerAboutToUse));
            }

            partyScreen.CalledFrom = null;
        };

        Action onBack = () =>
        {
            if (playerUnit.Pokemon.HP <= 0)
            {
                partyScreen.SetMessageText("You have to chooose a pokemon to continue");
                return;
            }

            partyScreen.gameObject.SetActive(false);
            if (partyScreen.CalledFrom == BattleState.AboutToUse)
            {
                StartCoroutine(SendNextTrainerPokemon());
            }
            else
            {
                ActionSelection();
            }

            partyScreen.CalledFrom = null;
        };

        partyScreen.HandleUpdate(onSelected, onBack);
    }

    // Handles if player wishes to switch pokemon or not after defeating one of the trainer's pokemon.
    void HandleAboutToUse()
    {
        if(Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            aboutToUseChoice = !aboutToUseChoice;
        }

        dialogBox.UpdateChoiceBox(aboutToUseChoice);

        if(Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableChoiceBox(false);
            if(aboutToUseChoice == true)
            {
                // Yes switch.
                OpenPartyScreen();
            }
            else
            {
                // No switch.
                StartCoroutine(SendNextTrainerPokemon());
            }
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableChoiceBox(false);
            StartCoroutine(SendNextTrainerPokemon());
        }
    }

    // Handles animation for calling back and inputting new pokemon.
    IEnumerator SwitchPokemon(Pokemon newPokemon, bool isTrainerAboutToUse = false)
    {
        // Execute only if pokemon is still alive.
        if(playerUnit.Pokemon.HP > 0)
        {
            // Recall pokemon.
            yield return dialogBox.TypeDialog($"Come back {playerUnit.Pokemon.Base.Name}");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }

        // Set new pokemon information.
        playerUnit.Setup(newPokemon);

        // Passes the user's pokemon's move to the program.
        dialogBox.SetMoveNames(newPokemon.Moves);

        yield return dialogBox.TypeDialog($"Go {newPokemon.Base.name}!");

        if (isTrainerAboutToUse)
        {
            StartCoroutine(SendNextTrainerPokemon());
        }
        else
        {
            state = BattleState.RunningTurn;
        }
    }

    IEnumerator SendNextTrainerPokemon()
    {
        state = BattleState.Busy;

        var nextPokemon = trainerParty.GetHealthyPokemon();
        enemyUnit.Setup(nextPokemon);
        yield return dialogBox.TypeDialog($"{trainer.Name} sends out {nextPokemon.Base.name}!");

        state = BattleState.RunningTurn;
    }

    IEnumerator OnItemUsed(ItemBase usedItem)
    {
        state = BattleState.Busy;
        inventoryUI.gameObject.SetActive(false);

        if (usedItem is PokeballItem)
        {
            yield return ThrowPokeball((PokeballItem)usedItem);
        }

        StartCoroutine(RunTurns(BattleAction.UseItem));
    }

    // Handles catching a pokemon.
    IEnumerator ThrowPokeball(PokeballItem pokeballItem)
    {
        state = BattleState.Busy;

        if(isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"Stealing pokemon is illegal in BrightLand city. Better to not do that.");
            state = BattleState.RunningTurn;
            yield break;
        }

        yield return dialogBox.TypeDialog($"{player.Name} used a {pokeballItem.Name}.");

        var pokeballObj = Instantiate(pokeballSprite, playerUnit.transform.position - new Vector3(2, 0), Quaternion.identity);        // Use quaternion.identity to not give an object a rotation.
        var pokeball = pokeballObj.GetComponent<SpriteRenderer>();
        pokeball.sprite = pokeballItem.Icon;

        // Pokeball capture animations.
        yield return pokeball.transform.DOJump(enemyUnit.transform.position + new Vector3(0, 2), 2f, 1, 1f).WaitForCompletion();
        yield return enemyUnit.PlayCaptureAnimation();
        pokeball.transform.DOMoveY(enemyUnit.transform.position.y - 1f, 0.5f).WaitForCompletion();

        //Pokeball shake animation.
        int shakeCount = TryToCatchPokemon(enemyUnit.Pokemon, pokeballItem);

        for ( int i = 0; i < Mathf.Min(shakeCount, 3); ++i )    // Mathf.min ensures the ball shakes three times no matter what.
        {
            yield return new WaitForSeconds(0.5f);
            yield return pokeball.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.8f).WaitForCompletion();
        }

        if (shakeCount == 4)
        {
            // Pokemon is caught.
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} was caught!!!");
            yield return pokeball.DOFade(0, 1.5f).WaitForCompletion();

            playerParty.AddPokemon(enemyUnit.Pokemon);
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} has been added to your party.");

            Destroy(pokeball);
            BattleOver(true);
        }
        else
        {
            // Pokemon broke out.
            yield return new WaitForSeconds(1f);
            pokeball.DOFade(0, 0.2f);
            yield return enemyUnit.PlayBreakOutAnimation();

            if( shakeCount < 2)
            {
                yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} broke free.");
            }
            else
            {
                yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} broke free. Damn, that was so close!");
            }
            Destroy(pokeball);
            state = BattleState.RunningTurn;
        }
    }

    // Handles if wild pokemon will be caught or not.
    int TryToCatchPokemon(Pokemon pokemon, PokeballItem pokeballItem)
    {
        float a = (3 * pokemon.MaxHp - 2 * pokemon.HP) * pokemon.Base.CatchRate * pokeballItem.CatchRateModifier * ConditionsDB.GetStatusBonus(pokemon.Status) / (3 * pokemon.MaxHp);
    
        if( a >= 255)
        {
            return 4;
        }

        //Determine how many times the pokeball should shake if not caught.
        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

        int shakeCount = 0;
        while (shakeCount < 4)
        {
            if (UnityEngine.Random.Range(0, 65535) >= b)
                break;
            ++shakeCount;
        }

        return shakeCount;
    }

    IEnumerator TryToEscape()
    {
        state = BattleState.Busy;
        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"Running is not an option in a pokemon duel.");
            state = BattleState.RunningTurn;
            yield break;
        }

        ++escapeAttempts;

        int playerSpeed = playerUnit.Pokemon.Speed;
        int enemySpeed = enemyUnit.Pokemon.Speed;

        if (enemySpeed < playerSpeed)
        {
            yield return dialogBox.TypeDialog($"Ran away successful!");
            BattleOver(true);
        }
        else
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * escapeAttempts;
            f = f % 256;
        
            if (UnityEngine.Random.Range(0, 256) < f)
            {
                yield return dialogBox.TypeDialog($"Ran away successful!");
                BattleOver(true);
            }
            else
            {
                yield return dialogBox.TypeDialog($"You failed at escaping.");
                state = BattleState.RunningTurn;
            }
        }
    }
}
