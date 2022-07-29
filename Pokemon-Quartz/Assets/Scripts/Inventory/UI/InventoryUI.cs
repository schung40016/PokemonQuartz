using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum InventoryUIState { ItemSelection, PartySelection, Busy, MoveToForget }

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
    [SerializeField] MoveSelectionUI moveSelectionUI;

    [SerializeField] TextSelector cateSelector;

    Action<ItemBase> onItemUsed;

    Inventory inventory;
    RectTransform itemListRect;
    List<ItemSlotUI> slotUIList;

    int selectedItem = 0;
    int selectedCategory = 0;
    MoveBase moveToLearn;

    InventoryUIState state;

    const int itemsInViewport = 8;

    public InventoryUIState GetInventoryState()
    {
        return state;
    }

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
                StartCoroutine(ItemSelected());
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
        else if (state == InventoryUIState.MoveToForget)
        {
            Action<int> onMoveSelected = (int moveIndex) =>
            {
                StartCoroutine(OnMoveToForgetSelected(moveIndex));
            };

            moveSelectionUI.HandleMoveSelection(onMoveSelected);
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

        yield return HandleTmItems();

        var usedItem = inventory.UseItem(selectedItem, partyScreen.SelectedMember, selectedCategory);

        Debug.Log(usedItem != null);

        if (usedItem != null)
        {
            if (usedItem is RecoveryItem)
            {
                yield return DialogManager.Instance.ShowDialogText($"The player used {usedItem.Name}.");
            }

            onItemUsed?.Invoke(usedItem);
        }
        else
        {   
            if (selectedCategory == (int)ItemCategory.Items)
            {
                yield return DialogManager.Instance.ShowDialogText($"Nothing happened!");
            }
        }

        ClosePartyScreen();
    }

    IEnumerator HandleTmItems()
    {
        var tmItem = inventory.GetItem(selectedItem, selectedCategory) as TmItem;
        
        if (tmItem == null)
        {
            yield break;
        }

        var pokemon = partyScreen.SelectedMember;

        if (pokemon.HasMove(tmItem.Move))
        {
            // BUG: TM still gets removed when pokemon already has move.
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} already knows {tmItem.Move.Name}.");
            yield break;
        }

        if (tmItem.CanBeTaught(pokemon))
        {
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} can't learn {tmItem.Move.Name}.");
            yield break;
        }

        if (pokemon.Moves.Count < PokemonBase.MaximumNumOfMoves)
        {
            pokemon.LearnMove(tmItem.Move);
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} learned {tmItem.Move.Name}.");
        }
        else
        {
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} is trying to learn {tmItem.Move.Name}.");
            yield return DialogManager.Instance.ShowDialogText($"But it cannot learn more than {PokemonBase.MaximumNumOfMoves} moves.");
            yield return ChooseMoveToForget(pokemon, tmItem.Move);
            yield return new WaitUntil(() => state != InventoryUIState.MoveToForget);
        }
    }

    // Handles the move selection ui menu.
    IEnumerator ChooseMoveToForget(Pokemon pokemon, MoveBase newMove)
    {
        state = InventoryUIState.MoveToForget;
        yield return DialogManager.Instance.ShowDialogText($"Choose a move you wish to forget", true, false);
        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(pokemon.Moves.Select(x => x.Base).ToList(), newMove);
        moveToLearn = newMove;

        state = InventoryUIState.MoveToForget;
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

        partyScreen.CLearMemberSlotMessages();

        hud.SetCurrentSelection(2);
        hud.disableHudToggle = false;
    }

    IEnumerator ItemSelected()
    {
        state = InventoryUIState.Busy;

        var item = inventory.GetItem(selectedItem, selectedCategory);

        if (GameController.Instance.State == GameState.Battle)
        {
            // In Battle.
            if (!item.CanUseInBattle)
            {
                yield return DialogManager.Instance.ShowDialogText($"This item cannot be used in battle!");
                state = InventoryUIState.ItemSelection;
                yield break;
            }
        }
        else
        {
            // Outside battle.
            if (!item.CanUseOutsideBattle)
            {
                yield return DialogManager.Instance.ShowDialogText($"This item cannot be used outside battle!");
                state = InventoryUIState.ItemSelection;
                yield break;
            }
        }

        if (selectedCategory == (int)ItemCategory.PokeBalls)
        {
            StartCoroutine(UseItem());
        }
        else
        {
            OpenPartyScreen();

            if(item is TmItem)
            {
                // Show if the TM is usable.
                partyScreen.ShowIfTmIsUsable(item as TmItem);
            }
        }
    }

    IEnumerator OnMoveToForgetSelected(int moveIndex)
    {
        var pokemon = partyScreen.SelectedMember;

        DialogManager.Instance.CloseDialog();

        moveSelectionUI.gameObject.SetActive(false);

        // Player does't want to learn the new move.
        if (moveIndex == PokemonBase.MaximumNumOfMoves)
        {
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.name} did not learn {moveToLearn.Name}");
        }
        // Pokemon forget and learn a new move.
        else
        {
            var selectedMove = pokemon.Moves[moveIndex].Base;
            Debug.Log(pokemon.Base.Name);
            Debug.Log(selectedMove.Name);
            Debug.Log(moveToLearn.Name);
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.name} forgot {selectedMove.Name} and learned {moveToLearn.Name}");
            pokemon.Moves[moveIndex] = new Move(moveToLearn);
        }

        moveToLearn = null;
        state = InventoryUIState.ItemSelection;
    }
}
