using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] Image pokeImage;
    [SerializeField] HPBar hpBar;

    Pokemon _pokemon;

    //Transfers pokemon data into party screen UI.
    public void SetData(Pokemon pokemon)
    {
        _pokemon = pokemon;

        nameText.text = pokemon.Base.Name;
        levelText.text = "Lvl " + pokemon.Level;
        hpBar.SetHP((float)pokemon.HP / pokemon.MaxHp);
    }

    //Highlights user's cusor in party screen.
    public void SetSelected(bool selected)
    {
        if(selected)
        {
            nameText.color = GlobalSettings.i.HighlightedColor;
            pokeImage.sprite = _pokemon.Base.FrontSprite;
        }
        else
        {
            nameText.color = GlobalSettings.i.RegColor;
        }
    }
}
