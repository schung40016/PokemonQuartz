using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Similar to dialogue box but for the sections.
public class TextSelector : MonoBehaviour
{
    [SerializeField] Color highlightColor;
    [SerializeField] GameObject sectionSelector;
    [SerializeField] List<Text> sectionTexts;

    //Indicates what action the player wants by turning words blue.
    public void UpdateSelection(int selectionAction)
    {
        for (int i = 0; i < sectionTexts.Count; i++)
        {
            //Selected
            if (i == selectionAction)
            {
                sectionTexts[i].color = highlightColor;
            }
            //Not selected
            else
            {
                sectionTexts[i].color = Color.green;
            }
        }
    }
}
