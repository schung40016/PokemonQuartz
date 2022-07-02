using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] Text statusText;
    [SerializeField] HPBar hpBar;
    [SerializeField] GameObject expBar;

    [SerializeField] Color psnColor;
    [SerializeField] Color parColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color frzColor;
    [SerializeField] Color slpColor;

    Pokemon _pokemon;
    Dictionary<ConditionID, Color> statusColors;

    // Controls lvl, pokemon name, and pokemon hp text.
    public void SetData(Pokemon pokemon)
    {
        _pokemon = pokemon;

        nameText.text = pokemon.Base.Name;
        SetLevel();
        hpBar.SetHP((float)pokemon.HP / pokemon.MaxHp);
        SetExp();

        statusColors = new Dictionary<ConditionID, Color>()
        {
            {ConditionID.psn, psnColor },
            {ConditionID.brn, brnColor },
            {ConditionID.par, parColor },
            {ConditionID.slp, slpColor },
            {ConditionID.frz, frzColor },
        };

        SetStatusText();
        _pokemon.OnStatusChanged += SetStatusText;
    }

    // Controls status text.
    void SetStatusText()
    {
        // Pokemon has no status condition.
        if(_pokemon.Status == null)
        {
            statusText.text = "";
        }
        // Set status.
        else
        {
            statusText.text = _pokemon.Status.Id.ToString().ToUpper();
            statusText.color = statusColors[_pokemon.Status.Id];
        }

    }

    // Set's level of the pokemon.
    public void SetLevel()
    {
        levelText.text = "Lvl " + _pokemon.Level;
    }

    // Updates the exp bar of the player hud.
    public void SetExp()
    {
        // Ensure that only the playerhud has the expbar.
        if (expBar == null)
        {
            return;
        }

        float normalizedExp = GetNormalizedExp();
        expBar.transform.localScale = new Vector3(normalizedExp, 1, 1);
    }

    // Animates our exp bar.
    public IEnumerator SetExpSmooth(bool reset = false)
    {
        // Ensure that only the playerhud has the expbar.
        if (expBar == null)
        {
            yield break;
        }

        if (reset == true)
        {
            expBar.transform.localScale = new Vector3(0, 1, 1);
        }

        float normalizedExp = GetNormalizedExp();
        yield return expBar.transform.DOScaleX(normalizedExp, 1.5f).WaitForCompletion();
    }


    float GetNormalizedExp()
    {
        int currLevelExp = _pokemon.Base.GetExpForLevel(_pokemon.Level);
        int nextLevelExp = _pokemon.Base.GetExpForLevel(_pokemon.Level + 1);

        float normalizedExp = (float)(_pokemon.Exp - currLevelExp) / (nextLevelExp - currLevelExp);
        return Mathf.Clamp01(normalizedExp);
    }

    // Updates pokemon's healthbar in hud.
    public IEnumerator UpdateHP()
    {
        if (_pokemon.HpChanged)
        {
            yield return hpBar.SetHPSmooth((float)_pokemon.HP / _pokemon.MaxHp);
            _pokemon.HpChanged = false;
        }
    }
}
