 using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Allows classes to be visible through inspector.
[System.Serializable]
public class Pokemon
{
    //Allow pokemon party to be visible through inspector.
    [SerializeField] PokemonBase _base;
    [SerializeField] int level;

    // Constructor for creating copy pokemons.
    public Pokemon(PokemonBase pBase, int pLevel)
    {
        _base = pBase;
        level = pLevel;

        Init();
    }

    public PokemonBase Base
    {
        get
        {
            return _base;
        }   
    }
    public int Level
    {
        get
        {
            return level;
        }
    }

    // For exp.
    public int Exp { get; set; }

    // For return & setting the value of pokemon health.
    public int HP { get; set; }

    // For returning & setting the list of moves that a pokemon has.
    public List<Move> Moves { get; set; }

    public Move CurrentMove { get; set; }

    // For stat calculations.
    public Dictionary<Stat, int> Stats { get; private set; }

    // For stat boost moves.
    public Dictionary<Stat, int> StatBoosts { get; private set; }   //Note: Moves can only be boosted or decremented 6 times.

    // Holds condition.
    public Condition Status { get; private set; }

    // Keeps count of number of turn for sleep check.
    public int StatusTime { get; set; }

    // Holds volatile conditions like confusion.
    public Condition VolatileStatus { get; private set; }

    // Keeps count of number of turn for confusion check.
    public int VolatileStatusTime { get; set; }

    // Display status moves that either pokemon applies to each other.
    public Queue<string> StatusChanges { get; private set; }

    public event System.Action OnStatusChanged;
    public event System.Action OnHPChanged;

    public void Init()
    {
        HP = MaxHp;
        Moves = new List<Move>();

        // Checks whether a pokemon can learn a move based on its level.
        foreach ( var move in Base.LearnableMoves)
        {
            if( move.Level <= Level )
            {
                Moves.Add(new Move(move.Base));
            }
            //If the pokemon as more than 4 moves, the cannot get a 5th move.
            if( Moves.Count >= PokemonBase.MaximumNumOfMoves )
            {
                break;
            }    
        }

        Exp = Base.GetExpForLevel(Level); 

        CalculateStats();
        HP = MaxHp;

        StatusChanges = new Queue<string>();
        ResetStatBoost();
        Status = null;
        VolatileStatus = null;
    }

    public Pokemon(PokemonSaveData saveData)
    {
        _base = PokemonDB.GetPokemonByName(saveData.name);
        HP = saveData.hp;
        level = saveData.level;
        Exp = saveData.exp;

        if (saveData.statusId != null)
        {
            Status = ConditionsDB.Conditions[saveData.statusId.Value];
        }
        else
        {
            Status = null;
        }

        Moves = new List<Move>();

        // Generate moves
        Moves = saveData.moves.Select(s => new Move(s)).ToList();

        CalculateStats();
        StatusChanges = new Queue<string>();
        ResetStatBoost();
        VolatileStatus = null;
    }

    public PokemonSaveData GetSaveData()
    {
        var saveData = new PokemonSaveData()
        {
            name = Base.Name,
            hp = HP,
            level = Level,
            exp = Exp,
            statusId = Status?.Id,
            moves = Moves.Select(m => m.GetSaveData()).ToList()
        };

        return saveData;
    }

    // Stores & Calculates the value for each stat.
    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Defense, Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5);
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5);
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt((Base.SpDefense * Level) / 100f) + 5);
        Stats.Add(Stat.Speed, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5);

        MaxHp = Mathf.FloorToInt((Base.MaxHP * Level) / 100f) + 10 + Level;
    }

    // Resets stats to clear prior stat data.
    void ResetStatBoost()
    {
        StatBoosts = new Dictionary<Stat, int>()
        {
            {Stat.Attack, 0},
            {Stat.Defense, 0},
            {Stat.SpAttack, 0},
            {Stat.SpDefense, 0},
            {Stat.Speed, 0},
            {Stat.Accuracy, 0},
            {Stat.Evasion, 0}
        };
    }

    // Applies effect of stat boosts.
    int GetStat(Stat stat)
    {
        int statVal = Stats[stat];

        // Apply stat moves.
        int boost = StatBoosts[stat];
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        // Positie stat boost.
        if(boost >= 0)
        {
            statVal = Mathf.FloorToInt(statVal * boostValues[boost]);
        }
        else
        {
            statVal = Mathf.FloorToInt(statVal / boostValues[-boost]);
        }

        return statVal;
    }

    // Applies stat effects onto pokemon.
    public void ApplyBoosts(List<StatBoost> statBoosts)
    {
        // Loop through all status effects for application.
        foreach ( var statBoost in statBoosts )
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);

            // Detect if status move is used.
            if ( boost > 0)
            {
                StatusChanges.Enqueue($"{Base.Name}'s {stat} rose!");
            }
            else
            {
                StatusChanges.Enqueue($"{Base.Name}'s {stat} fell!");
            }
        }
    }

    // For leveling up the pokemon.
    public bool CheckForLevelUp()
    {
        if (Exp > Base.GetExpForLevel(level + 1))
        {
            ++level;
            return true;
        }
        return false;
    }

    // Returns list of learnable moves at current level.
    public LearnableMove GetLearnableMoveAtCurrentLevel()
    {
        return Base.LearnableMoves.Where(x => x.Level == level).FirstOrDefault(); // FirstOrDefault returns the first item or null if empty.
    }

    // Adds move into pokemon's move pool.
    public void LearnMove(LearnableMove moveToLearn)
    {
        if (Moves.Count > PokemonBase.MaximumNumOfMoves)
        {
            return;
        }

        Moves.Add(new Move(moveToLearn.Base));
    }

    public int Attack
    {
        get { return GetStat(Stat.Attack); }
    }

    public int Defense
    {
        get { return GetStat(Stat.Defense); }
    }

    public int SpAttack
    {
        get { return GetStat(Stat.SpAttack); }
    }

    public int SpDefense
    {
        get { return GetStat(Stat.SpDefense); }
    }

    public int Speed
    {
        get { return GetStat(Stat.Speed); }
    }

    public int MaxHp { get; private set; }

    public DamageDetails TakeDamage(Move move, Pokemon attacker)
    {
        float critical = 1f;

        //Critical hit calculations.
        if( Random.value * 100 <= 6.25f)
        {
            critical = 2f; //Double the damage.
        }

        //Type effectiveness.
        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

        var damageDetails = new DamageDetails()
        {
            TypeEffectiveness = type,
            Critical = critical,
            Fainted = false
        };

        //Detect whether pokemon uses spAttack/Attack and spDefense/defense.
        float attack = (move.Base.Category == MoveCategory.Special) ? attacker.SpAttack : attacker.Attack;
        float defense = (move.Base.Category == MoveCategory.Special) ? attacker.SpDefense : attacker.Defense;

        //Calculate total damage.
        float modifiers = Random.Range(0.85f, 1f) * type * critical;
        float a = (2 * attacker.Level + 10) / 250f;         //Level dependency on the attack.
        float d = a * move.Base.Power * ((float)attack / defense) + 2;     //Attack power and Attack stat of pokemon countered by Enemy pokemon's defense.
        int damage = Mathf.FloorToInt(d * modifiers);

        DecreaseHP(damage);

        //Enemy pokemon lives.
        return damageDetails;
    }

    //Deals damage to pokemon based on status.
    public void IncreaseHP(int amount)
    {
        HP = Mathf.Clamp(HP + amount, 0, MaxHp);
        OnHPChanged?.Invoke();
    }

    //Deals damage to pokemon based on status.
    public void DecreaseHP(int damage)
    {
        HP = Mathf.Clamp(HP - damage, 0, MaxHp);
        OnHPChanged?.Invoke();
    }

    //Sets Pokemon's status.
    public void SetStatus(ConditionID conditionId)
    {
        //Check whether pokemon has status or not.
        if(Status != null)
        {
            return;
        }

        //Set new status on pokemon.
        Status = ConditionsDB.Conditions[conditionId];

        Status?.OnStart?.Invoke(this);

        StatusChanges.Enqueue($"{Base.Name} {Status.StartMessage}");

        OnStatusChanged?.Invoke();
    }

    //Sets Pokemon's status.
    public void SetVolatileStatus(ConditionID conditionId)
    {
        //Check whether pokemon has status or not.
        if (VolatileStatus != null)
        {
            return;
        }

        //Set new status on pokemon.
        VolatileStatus = ConditionsDB.Conditions[conditionId];

        VolatileStatus?.OnStart?.Invoke(this);

        StatusChanges.Enqueue($"{Base.Name} {VolatileStatus.StartMessage}");

        OnStatusChanged?.Invoke();
    }

    //Automatically cures pokemon of the condition.
    public void CureStatus()
    {
        Status = null;
        OnStatusChanged?.Invoke();
    }

    //Automatically cures pokemon of the volatile condition.
    public void CureVolatileStatus()
    {
        VolatileStatus = null;
    }

    //Picks random move for the enemy Pokemon.
    public Move GetRandomMove()
    {
        //Get all the moves that has PP.
        var movesWithPP = Moves.Where(x => x.PP > 0).ToList();

        int r = Random.Range(0, movesWithPP.Count);
        return movesWithPP[r];
    }

    //Checks whether the pokemon can perform a move or not.
    public bool OnBeforeMove()
    {
        bool canPerformMove = true;
        
        //Checks for regular conditons.
        if(Status?.OnBeforeMove != null)
        {
            if( !Status.OnBeforeMove(this) )
            {
                canPerformMove = false;
            }
        }

        //Checks for volatile conditions
        if (VolatileStatus?.OnBeforeMove != null)
        {
            if (!VolatileStatus.OnBeforeMove(this))
            {
                canPerformMove = false;
            }
        }

        return canPerformMove;
    }

    public void OnAfterTurn()
    {
        Status?.OnAfterTurn?.Invoke(this); //The ? tells program to execute code on the right only if it is not null.
        VolatileStatus?.OnAfterTurn?.Invoke(this);
    }

    //Battle over reset pokemon stats.
    public void OnBattleOver()
    {
        VolatileStatus = null;
        ResetStatBoost();
    }
}

public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
}

[System.Serializable]
public class PokemonSaveData
{
    public string name;
    public int hp;
    public int level;
    public int exp;
    public ConditionID? statusId;
    public List<MoveSaveData> moves;
}