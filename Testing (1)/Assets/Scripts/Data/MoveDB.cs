using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveDB
{
    static Dictionary<string, MoveBase> moves;

    public static void Init()
    {
        moves = new Dictionary<string, MoveBase>();

        // Allows us to interact with the objects from the assets folder.
        var moveList = Resources.LoadAll<MoveBase>("");

        foreach (var move in moveList)
        {
            if (moves.ContainsKey(move.Name))
            {
                Debug.LogError("We found 2 pokemons with the same name: " + move.Name + ".");
                continue;
            }

            moves[move.Name] = move;
        }
    }

    public static MoveBase GetMoveByName(string name)
    {
        if (!moves.ContainsKey(name))
        {
            Debug.LogError($"Pokemon with the name {name} not found in the database");
            return null;
        }

        return moves[name];
    }
}
