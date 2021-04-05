using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PokemonParty : MonoBehaviour
{
    [SerializeField] List<Pokemon> pokemons;

    // Public getter to access player's pokemon list.
    public List<Pokemon> Pokemons
    {
        get
        {
            return pokemons;
        }
    }

    // Initializes all pokemon in player party for battle.
    private void Start()
    {
        foreach (var pokemon in pokemons)
        {
            pokemon.Init();
        }
    }

    // At start of battle, get non fainted pokemon.
    public Pokemon GetHealthyPokemon()
    {
        return pokemons.Where(x => x.HP > 0 ).FirstOrDefault();
    }

    // Adds pokemon into party after capture.
    public void AddPokemon(Pokemon newPokemon)
    {
        if ( pokemons.Count < 6)
        {
            pokemons.Add(newPokemon);
        }
        else
        {
            // Will add a PC later.
        }
    }
}
