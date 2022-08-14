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
    //[SerializeField] GameController gameController;
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
    [SerializeField] GameObject bagSect;
    [SerializeField] GameObject questSect;
    [SerializeField] GameObject dexSect;
    [SerializeField] GameObject text;

    //Handles quitting the hud procedures.
    public event Action QuittingHud;

    //Tracks what section the player is on right now.
    int currentSection;

    // Checks whether the the pkayer wants to use an item on a mon.
    public bool disableHudToggle = false;
    int enableSelection = 0;
    private int selectedItem = 0;

    public GameObject activeSection;

    private void Awake()
    {
        //Get Image and original position of the image.
        //image = GetComponent<Image>();
        originalPos = image.transform.localPosition;
        StartAnimation();

        //Start section is always stats.
        activeSection = statSect;
    }

    public void StartAnimation()
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
        SectionSelector();

        QuitHud();
    }

    //Quit the HUD.
    public void QuitHud()
    {
        if (Input.GetKeyDown(KeyCode.Space) && (inventoryUI.GetInventoryState() != InventoryUIState.Busy || inventoryUI.GetInventoryState() != InventoryUIState.MoveToForget))
        {
            image.transform.DOLocalMoveY(590f, 1f); // Og: 590f.
            QuittingHud();
        }
    }

    //Handles section selection.
    public void SectionSelector()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow) && !disableHudToggle)
        {
            audio.PlayOneShot(selectSound);
            ++currentSection;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && !disableHudToggle)
        {
            audio.PlayOneShot(selectSound);
            --currentSection;
        }

        currentSection = Mathf.Clamp(currentSection, 0, 5);

        if (!disableHudToggle)
        {
            sectBox.UpdateSelection(currentSection);
        }

        //Player selected stats.
        if (currentSection == 0)
        {
            StationSelected(statSect);
        }
        //Player selected pokemon.
        else if (currentSection == 1)
        {
            Action onSelected;

            if (disableHudToggle && (inventoryUI.GetInventoryState() != InventoryUIState.Busy && inventoryUI.GetInventoryState() != InventoryUIState.MoveToForget))
            {
                EnableSection(1);

                // Player came from the bag section.
                onSelected = () =>
                {
                    StartCoroutine(inventoryUI.UseItem());
                };
            }
            else
            {
                // Open Pokemon summary.
                onSelected = () =>
                {
                };
            }

            Action onBack = () =>
            {
                // TODO: Tell the program whether the onBack is from regular or from inventoryUI.
                if (disableHudToggle)
                {
                    inventoryUI.ClosePartyScreen();
                    currentSection = 2;
                    disableHudToggle = false;
                }
                partySect.gameObject.SetActive(false);
            };

            StationSelected(partySect.gameObject);
            if (inventoryUI.GetInventoryState() == InventoryUIState.MoveToForget)
            {
                inventoryUI.HandleUpdate(null);
            }
            else
            {
                partySect.HandleUpdate(onSelected, onBack);
            }
        }
        //Player selected bag.
        else if (currentSection == 2)
        {
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                // Allows player to traverse to other sections within items.
                disableHudToggle = true;
                EnableSection(2);
            }

            Action onBack = () =>
            {
                bagSect.SetActive(false);
                disableHudToggle = false;
            };

            StationSelected(bagSect.gameObject); 

            inventoryUI.HandleUpdate(onBack);
        }
        else if (currentSection == 3)
        {
            StationSelected(questSect);
        }
        //Player selected radio.
        else if (currentSection == 4)
        {
            StationSelected(radioSect);
            radioSect.GetComponent<StationSelector>().HandleUpdate();
        }
        //Player select pokedex.
        else if (currentSection == 5)
        {
            StationSelected(dexSect);
        }
    }

    void StationSelected(GameObject section)
    {
        if (activeSection != section)
        {
            activeSection.SetActive(false);
            activeSection = section;
            activeSection.SetActive(true);
        }
    }

    public void SetCurrentSelection(int value)
    {
        currentSection = value;
    }

    public void EnableSection(int sect)
    {
        enableSelection = sect;
        sectBox.GreyOut(enableSelection);
    }
}

