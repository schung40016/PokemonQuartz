using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class HUD : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] TextSelector sectBox;
    [SerializeField] GameController gameController;
    private Vector3 originalPos;

    //Handles audio
    public AudioClip openMenu;
    public AudioClip selectSound;
    public AudioSource audio;

    public bool audioSwitch = true;

    //Store all gameobject section types.
    [SerializeField] GameObject statSect;
    [SerializeField] GameObject radioSect;
    [SerializeField] PartyScreen partySect;
    [SerializeField] InventoryUI inventoryUI;

    //Handles quitting the hud procedures.
    public event Action QuittingHud;

    //Tracks what section the player is on right now.
    int currentSection;

    // Checks whether the the pkayer wants to use an item on a mon.
    public bool useItemOnMon = false;

    public GameObject activeSection;

    private void Awake()
    {
        //Get Image and original position of the image.
        //image = GetComponent<Image>();
        originalPos = image.transform.localPosition;
        startAnimation();

        //Start section is always stats.
        activeSection = statSect;
    }

    public void startAnimation()
    { 
        image.transform.localPosition = new Vector3(originalPos.x, 590f);       
    }

    // Update is called once per frame
    public void HandleUpdate()
    {
        image.transform.DOLocalMoveY(0, 1f);  //originalPos.y
        if (audioSwitch)
        {
            audio.PlayOneShot(openMenu);
            audioSwitch = false;    
        }

        //Allow player to select the categories.
        sectionSelector();

        quitHud();
    }

    //Quit the HUD.
    public void quitHud()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            image.transform.DOLocalMoveY(590f, 1f); // Og: 590f.
            QuittingHud();
        }
    }

    //Handles section selection.
    public void sectionSelector()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow) && !useItemOnMon)
        {
            audio.PlayOneShot(selectSound);
            ++currentSection;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && !useItemOnMon)
        {
            audio.PlayOneShot(selectSound);
            --currentSection;
        }

        currentSection = Mathf.Clamp(currentSection, 0, 4);

        if (useItemOnMon) 
        {
            sectBox.GreyOut(1);
        }
        else
        {
            sectBox.UpdateSelection(currentSection);
        }

        //Player selected stats.
        if (currentSection == 0)
        {
            stationSelected(statSect);
        }
        //Player selected pokemon.
        else if (currentSection == 1)
        {
            Action onSelected = () =>
            {
                // TODO: Add summary screen
                if (useItemOnMon)
                {
                    sectBox.GreyOut(1);
                }
                else
                {

                }
            };

            Action onBack = () =>
            {
                // TODO: Tell the program whether the onBack is from regular or from inentoryUI.
                if (useItemOnMon)
                {
                    inventoryUI.ClosePartyScreen();
                    currentSection = 2;
                    useItemOnMon = false;
                }

                partySect.gameObject.SetActive(false);
            };

            stationSelected(partySect.gameObject);
            partySect.HandleUpdate(onSelected, onBack);
        }
        //Player selected bag.
        else if (currentSection == 2)
        {
            Action onBack = () =>
            {
                inventoryUI.gameObject.SetActive(false);
            };

            stationSelected(inventoryUI.gameObject);
            inventoryUI.HandleUpdate(onBack);
        }
        //Player selected radio.
        else if (currentSection == 3)
        {
            stationSelected(radioSect);
            radioSect.GetComponent<StationSelector>().HandleUpdate();
        }
        //Player select pokedex.
        else if (currentSection == 4)
        {
            activeSection.SetActive(false);
        }
    }

    void stationSelected(GameObject section)
    {
        activeSection.SetActive(false);
        activeSection = section;
        activeSection.SetActive(true);
    }

    public void SetCurrentSelection(int value)
    {
        currentSection = value;
    }
}
