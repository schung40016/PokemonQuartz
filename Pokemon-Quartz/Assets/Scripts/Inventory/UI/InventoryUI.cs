using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum InventoryUIState { ItemSelection, PartySelection, Busy }

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
    [SerializeField] GameObject scrollBar;

    [SerializeField] TextSelector cateSelector;

    Action<ItemBase> onItemUsed;

    Inventory inventory;
    RectTransform itemListRect;
    List<ItemSlotUI> slotUIList;

    int selectedItem = 0;
    int selectedCategory = 0;  

    InventoryUIState state;

    const int itemsInViewport = 8;

    private void Awake()
    {
        inventory = Inventory.GetInventory();
        itemListRect = itemList.GetComponent<RectTransform>();
    }

    private void Start()
    {
        UpdateItemList();

        inventory.OnUpdated += UpdateItemList;
    }

    void UpdateItemList()
    {
        // Clear all the existing items.
        foreach (Transform child in itemList.transform)
        {
            Destroy(child.gameObject);
        }

        slotUIList = new List<ItemSlotUI>();

        foreach (var itemSlot in inventory.GetSlotsByCategory(selectedCategory))
        {
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);
            slotUIObj.SetData(itemSlot);

            slotUIList.Add(slotUIObj);
        }
        UpdateItemSelection();
    }

    public void HandleUpdate(Action onBack,Action<ItemBase> onItemUsed=null)
    {
        this.onItemUsed = onItemUsed;

        if (state == InventoryUIState.ItemSelection)
        {
            int prevSelection = selectedItem;
            int prevCategory = selectedCategory;

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                ++selectedItem;
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                --selectedItem;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                ++selectedCategory;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                --selectedCategory;
            }

            if (selectedCategory > cateSelector.GetTextCount() - 1)
            {
                selectedCategory = 0;
            }
            else if (selectedCategory < 0)
            {
                selectedCategory = cateSelector.GetTextCount() - 1;
            }

            selectedItem = Mathf.Clamp(selectedItem, 0, inventory.GetSlotsByCategory(selectedCategory).Count - 1);           

            if (prevCategory != selectedCategory)
            {
                ResetSelection();
                cateSelector.UpdateSelection(selectedCategory);
                UpdateItemList();
            }
            else if (prevSelection != selectedItem)
            {
                UpdateItemSelection();
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                // TODO: Currently broke when user presses pokeball in HUD.
                ItemSelected();
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                onBack?.Invoke();
            }
        }
        else if (state == InventoryUIState.PartySelection)
        {
            if (hud != null)
            {
                hud.disableHudToggle = true;
                hud.SetCurrentSelection(1);
            }
            else
            {
                Action onSelected = () =>
                {
                    // Use the item on the selected pokemon.
                    StartCoroutine(UseItem());
                };

                Action onBackPartyScreen = () =>
                {
                    ClosePartyScreen();
                };

                partyScreen.HandleUpdate(onSelected, onBackPartyScreen);
            }
        }
    }

    // Resets the item UI, so that it refreshes with new category stuff.
    void ResetSelection()
    {
        selectedItem = 0;
        upArrow.gameObject.SetActive(false);
        downArrow.gameObject.SetActive(false);
        itemIcon.sprite = null;
        itemDesc.text = "";
    }

    public IEnumerator UseItem()
    {
        state = InventoryUIState.Busy;

        // Calling regular waitforseconds(1f) works but not dialogmanager.

        var usedItem = inventory.UseItem(selectedItem, partyScreen.SelectedMember, selectedCategory);

        if (usedItem != null)
        {
            if (!(usedItem is PokeballItem))
            {
                Debug.Log("hi");
                yield return DialogManager.Instance.ShowDialogText($"The player used {usedItem.Name}.");
            }

            onItemUsed?.Invoke(usedItem);
        }
        else
        {
            Debug.Log("hi");
            yield return DialogManager.Instance.ShowDialogText($"Nothing happened!");
        }

        ClosePartyScreen();
    }

    void UpdateItemSelection()
    {
        var slots = inventory.GetSlotsByCategory(selectedCategory);

        scrollBar.SetActive(true);

        selectedItem = Mathf.Clamp(selectedItem, 0, slots.Count - 1);

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

        if (slots.Count > 0)
        {
            var item = slots[selectedItem].Item;
            itemIcon.sprite = item.Icon;
            itemDesc.text = item.Description;
        }

        HandleScrolling();
    }

    void HandleScrolling()
    {
        if (slotUIList.Count <= itemsInViewport)
        {
            return;
        }

        // Specifically the height. Since ItemUiSlot is nactive when partyscreen is on, we can't use itemUISlot[0].Height. Had to use 12. 
        float scrollPos = Mathf.Clamp(selectedItem - itemsInViewport/2, 0, selectedItem) * 12;
        itemListRect.localPosition = new Vector2(itemListRect.localPosition.x, scrollPos);

        bool showUpArrow = selectedItem > itemsInViewport/2;
        upArrow.gameObject.SetActive(showUpArrow);
        bool showDownArrow = selectedItem+ itemsInViewport/2 < slotUIList.Count;
        downArrow.gameObject.SetActive(showDownArrow);
    }

    void OpenPartyScreen()
    {
        partyScreen.gameObject.SetActive(true);
        state = InventoryUIState.PartySelection;
    }

    public void ClosePartyScreen()
    {
        partyScreen.gameObject.SetActive(false);
        state = InventoryUIState.ItemSelection;
    }

    void ItemSelected()
    {
        if (selectedCategory == (int)ItemCategory.PokeBalls)
        {
            StartCoroutine(UseItem());
        }
        else
        {
            OpenPartyScreen();
        }
    }
}
