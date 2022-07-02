using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Condition
{
    //Name of condition.
    public string Name { get; set; }

    //Export ID's to other classes.
    public ConditionID Id { get; set; }

    //Description of the condition.
    public string Description { get; set; }

    //Tells hud what message to display for certain conditions.
    public string StartMessage { get; set; }

    //For sleep calculations.
    public Action<Pokemon> OnStart { get; set; }

    //For SLP, FRZ, and PAR conditions. Checks and stops pokemon from moving.
    //We use Func rather than Action because Func can return a value.
    public Func<Pokemon, bool> OnBeforeMove { get; set; } 

    //For BRN and POI conditions. Checks to damage pokemon if conditioned.
    public Action<Pokemon> OnAfterTurn { get; set; }
}
