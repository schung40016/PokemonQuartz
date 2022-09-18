using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestUI : MonoBehaviour
{
    [SerializeField] GameObject quests;
    [SerializeField] QuestSlotUI questSlotUI;

    [SerializeField] Image questIcon;
    [SerializeField] Text questDesc;

    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;

    [SerializeField] HUD hud;
    [SerializeField] GameObject scrollBar;

    QuestList questList;
    RectTransform questListRect;
    List<QuestSlotUI> slotUIList;

    int selectedQuest = 0;
    int selectedCategory = 0;

    const int questsInViewport = 8;

    private void Awake()
    {
        questList = QuestList.GetQuestList();
        questListRect = quests.GetComponent<RectTransform>();
    }

    private void Start()
    {
        UpdateQuestList();

        questList.OnUpdated += UpdateQuestList;
    }


    // Updates the backend info with all the current quests.
    void UpdateQuestList()
    {
        // Clear all the existing items.
        foreach (Transform child in quests.transform)
        {
            Destroy(child.gameObject);
        }

        slotUIList = new List<QuestSlotUI>();

        foreach (var questSlot in questList.GetQuests())
        {
            var slotUIObj = Instantiate(questSlotUI, quests.transform);
            slotUIObj.NameText.enabled = true;
            slotUIObj.CompletionText.enabled = true;
            slotUIObj.SetQuestData(questSlot);

            slotUIList.Add(slotUIObj);
        }

        UpdateQuestSelection();
    }

    public void HandleUpdate()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ++selectedQuest;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            --selectedQuest;
        }

        selectedQuest = Mathf.Clamp(selectedQuest, 0, questList.GetQuests().Count - 1);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            // TODO: Select quest.
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            // TODO: Deselect quest.
        }
    }


    // Updates the UI of the quest list.
    void UpdateQuestSelection()
    {
        var slots = questList.GetQuests();

        scrollBar.SetActive(true);

        selectedQuest = Mathf.Clamp(selectedQuest, 0, slots.Count - 1);

        for (int i = 0; i < slotUIList.Count; i++)
        {
            if (i == selectedQuest)
            {
                slotUIList[i].NameText.color = GlobalSettings.i.HighlightedColor;
            }
            else
            {
                slotUIList[i].NameText.color = GlobalSettings.i.RegColor;
            }
        }

        Debug.Log(slots.Count);

        if (slots.Count > 0)
        {
            var quest = slots[selectedQuest].Base;
            //questIcon.sprite = quest; //Create an image attribute for QuestBase"

            questDesc.text = quest.Desc;
        }

        HandleScrolling();
    }

    void HandleScrolling()
    {
        if (slotUIList.Count <= questsInViewport)
        {
            return;
        }

        // Specifically the height. Since ItemUiSlot is nactive when partyscreen is on, we can't use itemUISlot[0].Height. Had to use 12. 
        float scrollPos = Mathf.Clamp(selectedQuest - questsInViewport / 2, 0, selectedQuest) * 12;
        questListRect.localPosition = new Vector2(questListRect.localPosition.x, scrollPos);

        bool showUpArrow = selectedQuest > questsInViewport / 2;
        upArrow.gameObject.SetActive(showUpArrow);
        bool showDownArrow = selectedQuest + questsInViewport / 2 < slotUIList.Count;
        downArrow.gameObject.SetActive(showDownArrow);
    }

    // Select the quest.
    IEnumerator ItemSelected()
    {
        return null;
    }
}