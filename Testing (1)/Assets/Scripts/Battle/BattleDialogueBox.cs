using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleDialogueBox : MonoBehaviour
{
    [SerializeField] int lettersPerSecond;
    [SerializeField] Text dialogText;
    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject moveSelector;
    [SerializeField] GameObject moveDetails;
    [SerializeField] GameObject choiceBox;

    [SerializeField] List<Text> actionTexts;
    [SerializeField] List<Text> moveTexts;

    [SerializeField] Text ppText;
    [SerializeField] Text typeText;

    [SerializeField] Text yesText;
    [SerializeField] Text noText;

    Color highlightColor;
    Color regColor;

    private void Start()
    {
        highlightColor = GlobalSettings.i.HighlightedColor;
        regColor = GlobalSettings.i.RegColor;
    }

    public void SetDialog(string dialog)
    {
        dialogText.text = dialog;
    }

    //Coroutine to animate letters one by one.
    public IEnumerator TypeDialog(string dialog)
    {
        //To begin animation we want to start with no letters. Then add as we animate.
        dialogText.text = "";
        
        //Adds each letter to the sentence a single time.
        foreach (var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }

        yield return new WaitForSeconds(1f);
    }

    //Following methods (3), enables the class to let player choose.
    public void EnableDialogText(bool enabled)
    {
        dialogText.enabled = enabled;
    }

    public void EnableActionSelector(bool enabled)
    {
        actionSelector.SetActive(enabled);
    }

    public void EnableChoiceBox(bool enabled)
    {
        choiceBox.SetActive(enabled);
    }

    public void EnableMoveSelector(bool enabled)
    {
        moveSelector.SetActive(enabled);
        moveDetails.SetActive(enabled);
    }

    // Indicates what action the player wants by turning words blue.
    public void UpdateActionSelection(int selectionAction)
    {
        for( int i = 0; i < actionTexts.Count; i++)
        {
            if( i == selectionAction)
            {
                actionTexts[i].color = highlightColor;
            }    
            else
            {
                actionTexts[i].color = regColor;
            }
        }
    }

    // Indicates what move the player wants by turning words blue.
    public void UpdateMoveSelection(int selectedMove, Move move)
    {
        for( int i = 0; i < moveTexts.Count; i++)
        {
            if( i == selectedMove)
            {
                moveTexts[i].color = highlightColor;
            }
            else
            {
                moveTexts[i].color = regColor;
            }
        }

        ppText.text = $"PP {move.PP}/{move.Base.PP}";
        typeText.text = move.Base.Type.ToString();

        if ( move.PP == 0 )
        {
            ppText.color = Color.red;
        }
        else if ( move.PP > 0 && move.PP < move.PP/2)
        {
            ppText.color = Color.yellow;
        }
        else
        {
            ppText.color = Color.black;
        }
    }

    //Create move names for the battle sequence.
    public void SetMoveNames(List<Move> moves)
    {
        for( int i = 0; i < moveTexts.Count; i++)
        {
            if(i < moves.Count)
            {
                moveTexts[i].text = moves[i].Base.Name;
            }    
            else
            {
                moveTexts[i].text = "-";
            }
        }
    }

    //Indicates what action the player wants by turning words blue.
    public void UpdateChoiceBox(bool yesSelected)
    {
        if(yesSelected)
        {
            yesText.color = highlightColor;
            noText.color = regColor;
        }
        else
        {
            yesText.color = regColor;
            noText.color = highlightColor;
        }
    }
}
