using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonDB
{
    static Dictionary<string, PokemonBase> pokemons;

    public static void Init()
    {
        pokemons = new Dictionary<string, PokemonBase>();

        // Allows us to interact with the objects from the assets folder.
        var pokemonArray = Resources.LoadAll<PokemonBase>("");

        foreach (var pokemon in pokemonArray)
        {
            if (pokemons.ContainsKey(pokemon.Name))
            {
                Debug.LogError("We found 2 pokemons with the same name: " + pokemon.Name + ".");
                continue;
            }

            pokemons[pokemon.Name] = pokemon;
        }
    }

    public static PokemonBase GetPokemonByName(string name)
    {
        if (!pokemons.ContainsKey(name))
        {
            Debug.LogError($"Pokemon with the name {name} not found in the database");
            return null;
        }

        return pokemons[name];
    }
}
