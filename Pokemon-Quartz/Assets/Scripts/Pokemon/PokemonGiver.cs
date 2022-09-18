using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonGiver : MonoBehaviour, ISavable
{
    [SerializeField] Pokemon pokemonToGive;
    [SerializeField] Dialog dialog;

    bool used = false;

    public IEnumerator GivePokemon(PlayerController player)
    {
        yield return DialogManager.Instance.ShowDialog(dialog);

        pokemonToGive.Init();
        player.GetComponent<PokemonParty>().AddPokemon(pokemonToGive);

        used = true;

        string dialogText = $"You received {pokemonToGive.Base.Name}.";

        yield return DialogManager.Instance.ShowDialogText(dialogText);
    }

    public bool CanBeGiven()
    {
        return pokemonToGive != null && !used;
    }

    public object CaptureState()
    {
        return used;
    }

    public void RestoreState(object state)
    {
        used = (bool)state;
    }
}
