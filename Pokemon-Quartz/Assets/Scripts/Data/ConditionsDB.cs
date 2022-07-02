using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB : MonoBehaviour
{ 
    public static void Init()
    {
        foreach( var kvp in Conditions)
        {
            var conditionId = kvp.Key;
            var condition = kvp.Value;

            condition.Id = conditionId;
        }
    }

    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        {
            ConditionID.psn,
            new Condition()
            {
                Name = "Poison",
                StartMessage = "feels a bit woozy!",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.UpdateHP(pokemon.MaxHp / 8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} hurt itself due to poison.");
                }
            }
        },
        {
            ConditionID.brn,
            new Condition()
            {
                Name = "Burn",
                StartMessage = "has been burned!",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.UpdateHP(pokemon.MaxHp / 16);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} taken damage from its burn.");
                }
            }
        },
        {
            ConditionID.par,
            new Condition()
            {
                Name = "Paralyze",
                StartMessage = "has been paralyzed!",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    //Return true = pokemon can move, else it false if it cannot move.
                    //Paralyzed.
                    if (Random.Range(1,5) == 1)
                    {
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is paralyzed; it can't move!");
                        return false;
                    }
                    //Not paralyzed.
                    return true;
                }
            }
        },
        {
            ConditionID.frz,
            new Condition()
            {
                Name = "Freeze",
                StartMessage = "has been frozen!",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    //Return false because pokemon cannot move if frozen
                    //Paralyzed.
                    if (Random.Range(1,5) == 1)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is not frozen anymore!");
                        return true;
                    }
                    //Frozen.
                    return false;
                }
            }
        },
        {
            ConditionID.slp,
            new Condition()
            {
                Name = "Sleep",
                StartMessage = "has been been put to sleep!",
                OnStart = (Pokemon pokemon) =>
                {
                    //Sleeps for 1-3 turns.
                    pokemon.StatusTime = Random.Range(1,4);
                    Debug.Log($"Will be asleep for {pokemon.StatusTime} moves");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    //False = sleeping, else not asleep.
                    if( pokemon.StatusTime <= 0 )
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} woke up!");
                        return true;
                    }

                    //Decrement how many more turns the sleep lasts.
                    pokemon.StatusTime--;
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is asleep!");
                    
                    //Sleeping.
                    return false;
                }
            }
        },
        //Volatile Status
        {
            ConditionID.confusion,
            new Condition()
            {
                Name = "Confusion",
                StartMessage = " is confused!",
                OnStart = (Pokemon pokemon) =>
                {
                    //Confused for 1-4 turns.
                    pokemon.VolatileStatusTime = Random.Range(1, 5);
                    Debug.Log($"Will be confused for {pokemon.VolatileStatusTime} moves");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    //False = sleeping, else not asleep.
                    if( pokemon.VolatileStatusTime <= 0 )
                    {
                        pokemon.CureVolatileStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is not confused anymore!");
                        return true;
                    }

                    //Decrement how many more turns the sleep lasts.
                    pokemon.VolatileStatusTime--;

                    //50% pokemon can perform a move.
                    if( Random.Range(1, 3) == 1)
                    {
                        return true;
                    }

                    //Else hurt by confusion
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is confused!");
                    pokemon.UpdateHP(pokemon.MaxHp / 8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} hurt itself in confusion!");
                    return false;
                }
            }
        }
    };

    public static float GetStatusBonus(Condition condition)
    {
        if( condition == null)
        {
            return 1f;
        }
        else if( condition.Id == ConditionID.slp || condition.Id == ConditionID.frz)
        {
            return 2f;
        }
        else if (condition.Id == ConditionID.par || condition.Id == ConditionID.brn || condition.Id == ConditionID.psn)
        {
            return 1.5f;
        }

        return 1f;
    }
}

public enum ConditionID
{
    none, psn, brn, slp, par, frz, confusion
}