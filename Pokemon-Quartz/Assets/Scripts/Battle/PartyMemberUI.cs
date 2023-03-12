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
    [SerializeField] Text messageText;

    Pokemon _pokemon;

    //Transfers pokemon data into party screen UI.
    public void Init(Pokemon pokemon)
    {
        _pokemon = pokemon;
        UpdateData();
        SetMessage("");
        _pokemon.OnHPChanged += UpdateData;
    }

    void UpdateData()
    {
        nameText.text = _pokemon.Base.Name;
        levelText.text = "Lvl " + _pokemon.Level;
        hpBar.SetHP((float)_pokemon.HP / _pokemon.MaxHp);
    }

    //Highlights user's cusor in party screen.
    public void SetSelected(bool selected)
    {
        if(selected)
        {
            nameText.color = GlobalSettings.i.HighlightedColor;
            pokeImage.sprite = _pokemon.Base.FrontSprite;                       // TO-DO: Change this to pip boy sprite later on.
        }
        else
        {
            nameText.color = GlobalSettings.i.RegColor;
        }
    }

    public void SetMessage(string message)
    {
        messageText.text = message;
    }
}
