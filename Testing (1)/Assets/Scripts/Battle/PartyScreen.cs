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

        messageText.text = "Choose a Pokemon";
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
