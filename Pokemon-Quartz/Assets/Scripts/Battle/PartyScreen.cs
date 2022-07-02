using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] Text messageText;

    PartyMemberUI[] memberSlots;

    //Enables us to temporarily store all pokemons from user for programing shortcut.
    List<Pokemon> pokemons;

    int selection = 0;

    public Pokemon SelectedMember => pokemons[selection];

    // Party screen can be called from different states like ActionSelect.
    public BattleState? CalledFrom { get; set; }

    //Initalizes all player's pokemon in party screen.
    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);
    }

    //Data handling for all 6 (or less) pokemon.
    public void SetPartyData(List<Pokemon> pokemons)
    {
        this.pokemons = pokemons;

        //Loop through data to acquire each pokemon.
        for( int i = 0; i < memberSlots.Length; i++ )
        {
            if( i < pokemons.Count )
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].SetData(pokemons[i]);
            }
            //Pokemon does not exist (player has less than 6 pokemons).
            else
            {
                memberSlots[i].gameObject.SetActive(false);
            }
        }

        UpdateMemberSelection(selection);

        messageText.text = "Choose a Pokemon";
    }

    // Handle party screen.
    public void HandleUpdate(Action onSelected, Action onBack)
    {
        var prevSelection = selection;

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++selection;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --selection;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selection += 1;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selection -= 1;
        }

        selection = Mathf.Clamp(selection, 0, pokemons.Count - 1);

        if (selection != prevSelection)
        {
            UpdateMemberSelection(selection);
        }

        // Player selects party screen.
        if (Input.GetKeyDown(KeyCode.Z))
        {
            onSelected?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            onBack?.Invoke();
        }
    }

    public void UpdateMemberSelection(int selectedMember)
    {
        for( int i = 0; i < pokemons.Count; i++ )
        {
            if( i == selectedMember )
            {
                memberSlots[i].SetSelected(true);
            }
            else
            {
                memberSlots[i].SetSelected(false);
            }
        }
    }

    public void SetMessageText(string message)
    {
        messageText.text = message;
    }
}
