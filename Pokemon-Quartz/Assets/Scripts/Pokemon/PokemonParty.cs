using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class PokemonParty : MonoBehaviour
{
    [SerializeField] List<Pokemon> pokemons;

    public event Action OnUpdated;

    // Public getter to access player's pokemon list.
    public List<Pokemon> Pokemons
    {
        get
        {
            return pokemons;
        }
        set
        {
            pokemons = value;

            OnUpdated?.Invoke();
        }
    }

    // Initializes all pokemon in player party for battle.
    private void Awake()
    {
        foreach (var pokemon in pokemons)
        {
            pokemon.Init();
        }
    }

    private void Start()
    {
    }

    // At start of battle, get non fainted pokemon.
    public Pokemon GetHealthyPokemon()
    {
        return pokemons.Where(x => x.HP > 0).FirstOrDefault();
    }

    // Adds pokemon into party after capture.
    public void AddPokemon(Pokemon newPokemon)
    {
        if ( pokemons.Count < 6)
        {
            pokemons.Add(newPokemon);
            OnUpdated?.Invoke();
        }
        else
        {
            // Will add a PC later.
        }
    }

    public bool CheckForEvolutions()
    {
        return pokemons.Any(p => p.CheckForEvolution() != null);
    }

    public IEnumerator RunEvolutions()
    {
        foreach (var pokemon in pokemons)
        {
            var evolution = pokemon.CheckForEvolution();
            if (evolution != null)
            {
                yield return EvolutionManager.i.Evolve(pokemon, evolution);
            }
        }
    }

    public void PartyUpdated()
    {
        OnUpdated?.Invoke();
    }

    public static PokemonParty GetPlayerParty()
    {
        return FindObjectOfType<PlayerController>().GetComponent<PokemonParty>();
    }
}
