using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : MonoBehaviour
{
    [SerializeField] string healFarewell = $"Yep, that should do it. Try to be careful next time.";
    [SerializeField] string notHealFarewell = $"Quit wasting my time then. Hurry up, go on now... I got Pokemon to heal.";

    public IEnumerator Heal(Transform player, Dialog dialog)
    {
        int selectedChoice = 0;

        yield return DialogManager.Instance.ShowDialog(dialog, 
            new List<string>() { "Yes", "No" }, 
            (choiceIndex) =>  selectedChoice = choiceIndex);

        // Selected yes.
        if (selectedChoice == 0)
        {
            yield return Fader.i.FadeIn(0.5f);

            var playerParty = player.GetComponent<PokemonParty>();
            playerParty.Pokemons.ForEach(p => p.Heal());
            playerParty.PartyUpdated();

            yield return Fader.i.FadeOut(0.5f);

            yield return DialogManager.Instance.ShowDialogText(healFarewell);
        }
        // Selected no.
        else if (selectedChoice == 1)
        {
            yield return DialogManager.Instance.ShowDialogText(notHealFarewell);
        }
    }
}
