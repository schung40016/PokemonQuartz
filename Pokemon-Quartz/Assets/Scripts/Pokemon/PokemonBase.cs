using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon/Create new Pokemon")]

public class PokemonBase : ScriptableObject
{
    [SerializeField] string name;

    [TextArea]
    [SerializeField] string description;

    //Visual representation.
    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;

    //Types.
    [SerializeField] PokemonType type1;
    [SerializeField] PokemonType type2;

    //Base stats.
    [SerializeField] int maxHP;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;

    // Used for exp gain calculations.
    [SerializeField] int expYield;
    [SerializeField] GrowthRate growthRate;

    [SerializeField] int catchRate = 255;

    [SerializeField] List<LearnableMove> learnableMoves;
    [SerializeField] List<MoveBase> learnableByItems;

    [SerializeField] List<Evolution> evolutions;

    public static int MaximumNumOfMoves { get; set; } = 4;

    public List<MoveBase> LearnableByItems => learnableByItems;

    public List<Evolution> Evolutions => evolutions;

    public int GetExpForLevel(int level)
    {
        if (growthRate == GrowthRate.Fast)
        {
            return 4 * (level * level * level) / 5;
        }
        else if (growthRate == GrowthRate.MediumFast)
        {
            return level * level * level;
        }
        else if (growthRate == GrowthRate.MediumSlow)
        {
            return 6 * (level * level * level) / 5 - 15 * (level * level) + 100 * level - 140;
        }
        else if (growthRate == GrowthRate.Slow)
        {
            return 5 * (level * level * level) / 4;
        }
        else if (growthRate == GrowthRate.Fluctuating)
        {
            return GetFluctuating(level);
        }
        return -1;
    }

    public int GetFluctuating(int level)
    {
        if (level <= 15)
        {
            return Mathf.FloorToInt(Mathf.Pow(level, 3) * ((Mathf.Floor((level + 1) / 3) + 24) / 50));
        }
        else if (level >= 15 && level <= 36)
        {
            return Mathf.FloorToInt(Mathf.Pow(level, 3) * ((level + 14) / 50));
        }
        else
        {
            return Mathf.FloorToInt(Mathf.Pow(level, 3) * ((Mathf.Floor(level / 2) + 32) / 50));
        }
    }

    public string Name
    {
        get { return name; }
    }

    public string Description
    {
        get { return description; }
    }

    public Sprite FrontSprite
    {
        get { return frontSprite; }
    }

    public Sprite BackSprite
    {
        get { return backSprite; }
    }

    public PokemonType Type1
    {
        get { return type1; }
    }

    public PokemonType Type2
    {
        get { return type2; }
    }

    public int MaxHP
    {
        get { return maxHP; }
    }

    public int Attack
    {
        get { return attack; }
    }

    public int Defense
    {
        get { return defense; }
    }
       
    public int SpAttack
    {
        get { return spAttack;  }
    }

    public int SpDefense
    {
        get { return spDefense; }
    }

    public int Speed
    {
        get { return speed;  }
    }

    public List<LearnableMove> LearnableMoves
    {
        get { return learnableMoves; }
    }

    public int CatchRate => catchRate;  // Property that only works if the variable needs a getter.

    public int ExpYield => expYield;

    public GrowthRate GrowthRate => growthRate;
}

[System.Serializable]
public class LearnableMove
{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;

    //Changed - 7/12/2020
    public MoveBase Base
    {
        get { return moveBase; }
    }

    public int Level
    {
        get { return level; }
    }
}

[System.Serializable]
public class Evolution
{
    [SerializeField] PokemonBase evolvesInto;
    [SerializeField] int requiredLevel;
    [SerializeField] EvolutionItem requiredItem;

    public PokemonBase EvolesInto => evolvesInto;
    public int RequiredLevel => requiredLevel;
    public EvolutionItem RequiredItem => requiredItem;
}

public enum PokemonType
{
    None,
    Normal, 
    Fire, 
    Water,
    Electric,
    Grass,
    Ice,
    Fighting,
    Poison,
    Ground,
    Flying,
    Psychic,
    Bug,
    Rock,
    Ghost,
    Dragon,
    Dark,
    Steel,
    Fairy
}

public enum GrowthRate
{
    Fast, MediumFast, MediumSlow, Slow, Fluctuating 
}

public enum Stat
{
    Attack, Defense, SpAttack, SpDefense, Speed, Accuracy, Evasion
    //Note: Accuracy and Evasion aren't actual stats, they're there to boost moveAccuracy.
}

//Class holds all values for the weaknesses.
public class TypeChart
{
    static float[][] chart =
    {
        //Defenses-->        NOR   FIR   WAT   ELE   GRA   ICE   FIG   POI   GRO   FLY   PSY   BUG   ROC   GHO   DRA   DAR   STE   FAI
        /*NOR*/ new float[] { 1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f, 0.5f,   0f,   1f,   1f, 0.5f,   1f },
        /*FIR*/ new float[] { 1f, 0.5f, 0.5f,   1f,   2f,   2f,   1f,   1f,   1f,   1f,   1f,   2f, 0.5f,   1f, 0.5f,   1f,   2f,   1f },
        /*WAT*/ new float[] { 1f,   2f, 0.5f,   1f, 0.5f,   1f,   1f,   1f,   2f,   1f,   1f,   1f,   2f,   1f, 0.5f,   1f,   1f,   1f },
        /*ELE*/ new float[] { 1f,   1f,   2f, 0.5f, 0.5f,   1f,   1f,   1f,   0f,   2f,   1f,   1f,   1f,   1f, 0.5f,   1f,   1f,   1f },
        /*GRA*/ new float[] { 1f, 0.5f,   2f,   1f, 0.5f,   1f,   1f, 0.5f,   2f, 0.5f,   1f, 0.5f,   2f,   1f, 0.5f,   1f, 0.5f,   1f },
        /*ICE*/ new float[] { 1f, 0.5f, 0.5f,   1f,   2f, 0.5f,   1f,   1f,   2f,   2f,   1f,   1f,   1f,   1f,   2f,   1f, 0.5f,   1f },
        /*FIG*/ new float[] { 2f,   1f,   1f,   1f,   1f,   2f,   1f, 0.5f,   1f, 0.5f, 0.5f, 0.5f,   2f,   0f,   1f,   2f,   2f, 0.5f },
        /*POI*/ new float[] { 1f,   1f,   1f,   1f,   2f,   1f,   1f, 0.5f, 0.5f,   1f,   1f,   1f, 0.5f, 0.5f,   1f,   1f,   0f,   2f },
        /*GRO*/ new float[] { 1f,   2f,   1f,   2f, 0.5f,   1f,   1f,   2f,   1f,   0f,   1f, 0.5f,   2f,   1f,   1f,   1f,   2f,   1f },
        /*FLY*/ new float[] { 1f,   1f,   1f, 0.5f,   2f,   1f,   2f,   1f,   1f,   1f,   1f,   2f, 0.5f,   1f,   1f,   1f, 0.5f,   1f },
        /*PSY*/ new float[] { 1f,   1f,   1f,   1f,   1f,   1f,   2f,   2f,   1f,   1f, 0.5f,   1f,   1f,   1f,   1f,   0f, 0.5f,   1f },
        /*BUG*/ new float[] { 1f, 0.5f,   1f,   1f,   2f,   1f, 0.5f, 0.5f,   1f, 0.5f,   2f,   1f,   1f, 0.5f,   1f,   2f, 0.5f, 0.5f },
        /*ROC*/ new float[] { 1f,   2f,   1f,   1f,   1f,   2f, 0.5f,   1f, 0.5f,   2f,   1f,   2f,   1f,   1f,   1f,   1f, 0.5f,   1f },
        /*GHO*/ new float[] { 0f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   2f,   1f,   1f,   2f,   1f, 0.5f,   1f,   1f },
        /*DRA*/ new float[] { 1f, 0.5f,   1f,   1f,   2f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   2f,   1f, 0.5f,   0f },
        /*DAR*/ new float[] { 1f,   1f,   1f,   1f,   1f,   1f, 0.5f,   1f,   1f,   1f,   2f,   1f,   1f,   2f,   1f, 0.5f,   1f, 0.5f },
        /*STE*/ new float[] { 1f, 0.5f, 0.5f, 0.5f,   1f,   2f,   1f,   1f,   1f,   1f,   1f,   1f,   2f,   1f,   1f,   1f, 0.5f,   2f },
        /*FAI*/ new float[] { 1f, 0.5f,   1f,   1f,   1f,   1f,   2f, 0.5f,   1f,   1f,   1f,   1f,   1f,   1f,   2f,   2f, 0.5f,   1f }
    };

    public static float GetEffectiveness(PokemonType attackType, PokemonType defenseType)
    {
        //If either pokemon on field is none.
        if( attackType == PokemonType.None || defenseType == PokemonType.None)
        {
            return 1;
        }

        //Subtract by one because of row and enum value difference.
        int row = (int)attackType - 1;

        int col = (int)defenseType - 1;

        return chart[row][col];
    }
}