using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;

    [SerializeField] Text categoryText;
    [SerializeField] Image itemIcon;
    [SerializeField] Text itemDesc;

    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;

    int selectedItem;
    const int itemsInViewport = 8;
    RectTransform itemListRect;

    List<ItemBase> availableItems;
    List<ItemSlotUI> slotUIList;
    Action<ItemBase> onItemSelected;
    Action onBack;

    private void Awake()
    {
        itemListRect = itemList.GetComponent<RectTransform>();
    }

    public void Show(List<ItemBase> availableItems, Action<ItemBase> onItemSelected, Action onBack)
    {
        this.availableItems = availableItems;
        this.onItemSelected = onItemSelected;
        this.onBack = onBack;

        gameObject.SetActive(true);
        UpdateItemList();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void HandleUpdate()
    {
        var prevSelection = selectedItem;

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ++selectedItem; 
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            --selectedItem;
        }

        selectedItem = Mathf.Clamp(selectedItem, 0, availableItems.Count);

        if (selectedItem != prevSelection)
        {
            UpdateItemSelection();
        }

        if(Input.GetKeyDown(KeyCode.Z))
        {
            onItemSelected?.Invoke(availableItems[selectedItem]);
        }
        else if(Input.GetKeyDown(KeyCode.X))
        {
            onBack?.Invoke();
        }
    }

    void UpdateItemList()
    {
        // Clear all the existing items.
        foreach (Transform child in itemList.transform)
        {
            Destroy(child.gameObject);
        }

        slotUIList = new List<ItemSlotUI>();

        foreach (var item in availableItems)
        {
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);
            slotUIObj.SetNameAndPrice(item);

            slotUIList.Add(slotUIObj);
        }
        UpdateItemSelection();
    }

    void UpdateItemSelection()
    {
        //scrollBar.SetActive(true);

        selectedItem = Mathf.Clamp(selectedItem, 0, availableItems.Count - 1);

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

        if (availableItems.Count > 0)
        {
            itemIcon.color = Color.white;
            var item = availableItems[selectedItem];
            itemIcon.sprite = item.Icon;
            itemDesc.text = item.Description;
        }
        else
        {
            itemIcon.color = Color.black;
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
}
