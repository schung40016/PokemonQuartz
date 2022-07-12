﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum InventoryrUIState { ItemSelection, PartySelection, Busy }

public class InventoryUI : MonoBehaviour
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;

    [SerializeField] Image itemIcon;
    [SerializeField] Text itemDesc;

    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;

    [SerializeField] PartyScreen partyScreen;
    [SerializeField] HUD hud;

    Inventory inventory;
    RectTransform itemListRect;
    List<ItemSlotUI> slotUIList;

    int selectedItem = 0;

    InventoryrUIState state;

    const int itemsInViewport = 8;

    private void Awake()
    {
        inventory = Inventory.GetInventory();
        itemListRect = itemList.GetComponent<RectTransform>();
    }

    private void Start()
    {
        UpdateItemList();   
    }

    void UpdateItemList()
    {
        // Clear all the existing items.
        foreach (Transform child in itemList.transform)
        {
            Destroy(child.gameObject);
        }

        slotUIList = new List<ItemSlotUI>();

        foreach (var itemSlot in inventory.Slots)
        {
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);
            slotUIObj.SetData(itemSlot);

            slotUIList.Add(slotUIObj);
        }

        UpdateItemSelection();
    }

    public void HandleUpdate(Action onBack)
    {
        if (state == InventoryrUIState.ItemSelection)
        {
            int prevSelection = selectedItem;

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                ++selectedItem;
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                --selectedItem;
            }

            selectedItem = Mathf.Clamp(selectedItem, 0, inventory.Slots.Count - 1);

            if (prevSelection != selectedItem)
            {
                UpdateItemSelection();
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                OpenPartyScreen();
            }
        }
        else if (state == InventoryrUIState.PartySelection)
        {
            /*
            Action onSelected = () =>
            {
                // Use the item on the selected pokemon.
                // Tell HUD's currentSelection to become partyScreen.
                hud.SetCurrentSelection(1); 
            };

            Action onBackPartyScreen = () =>
            {
                hud.SetCurrentSelection(2);
                ClosePartyScreen();
            };

            partyScreen.HandleUpdate(onSelected, onBackPartyScreen);
            */

            hud.useItemOnMon = true;
            hud.SetCurrentSelection(1);
        }
    }

    void UpdateItemSelection()
    {
        for (int i = 0; i < slotUIList.Count; i++)
        {
            if (i == selectedItem)
            {
                slotUIList[i].NameText.color = GlobalSettings.i.HighlightedColor;
            }
            else
            {
                slotUIList[i].NameText.color = GlobalSettings.i.RegColor;
            }
        }

        var item = inventory.Slots[selectedItem].Item;
        itemIcon.sprite = item.Icon;
        itemDesc.text = item.Description;

        HandleScrolling();
    }

    void HandleScrolling()
    {
        if (slotUIList.Count <= itemsInViewport)
        {
            return;
        }

        float scrollPos = Mathf.Clamp(selectedItem - itemsInViewport/2, 0, selectedItem) * slotUIList[0].Height;
        itemListRect.localPosition = new Vector2(itemListRect.localPosition.x, scrollPos);

        bool showUpArrow = selectedItem > itemsInViewport/2;
        upArrow.gameObject.SetActive(showUpArrow);
        bool showDownArrow = selectedItem+ itemsInViewport/2 < slotUIList.Count;
        downArrow.gameObject.SetActive(showDownArrow);
    }

    void OpenPartyScreen()
    {
        state = InventoryrUIState.PartySelection;
    }

    public void ClosePartyScreen()
    {
        state = InventoryrUIState.ItemSelection;
    }
}
