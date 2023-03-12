using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShopState { Menu, Buying, Selling, Busy }

public class ShopController : MonoBehaviour
{
    [SerializeField] Vector2 shopCameraOffset;
    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] ShopUI shopUI;
    [SerializeField] WalletUI walletUI;
    [SerializeField] CountSelectorUI countSelectorUI;
    Inventory inventory;

    public event Action OnStart;
    public event Action OnFinish;
    Merchant merchant;

    ShopState state;

    public static ShopController i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    private void Start()
    {
        inventory = Inventory.GetInventory();
    }

    public IEnumerator StartTrading(Merchant merchant)
    {
        this.merchant = merchant;

        OnStart?.Invoke();

        yield return StartMenuState();
    }

    IEnumerator StartMenuState()
    {
        state = ShopState.Menu;
        int selectedChoice = 0;

        yield return DialogManager.Instance.ShowDialogText("What are you looking to buy?",
            waitForInput: false,
            choices: new List<string> { "Buy", "Sell", "Quit" },
            onChoiceSelected: choiceIndex => selectedChoice = choiceIndex);

        // Buy
        if (selectedChoice == 0)
        {
            yield return GameController.Instance.MoveCamera(shopCameraOffset);
            walletUI.Show();
            shopUI.Show(merchant.AvailableItems, (item) => StartCoroutine(BuyItem(item)), () => StartCoroutine(OnBackFromBuying()));

            state = ShopState.Buying;
        }
        // Sell
        else if (selectedChoice == 1)
        {
            state = ShopState.Selling;
            inventoryUI.gameObject.SetActive(true);
        }
        // Quit
        else if (selectedChoice == 2)
        {
            OnFinish?.Invoke();
            yield break;
        }
    }

    public void HandleUpdate()
    {
        if (state == ShopState.Selling)
        {
            inventoryUI.HandleUpdate(OnBackFromSelling, (selectedItem) => StartCoroutine(SellItem(selectedItem)));
        }
        else if (state == ShopState.Buying)
        {
            shopUI.HandleUpdate();
        }
    }

    void OnBackFromSelling()
    {
        inventoryUI.gameObject.SetActive(false);
        StartCoroutine(StartMenuState());
    }

    IEnumerator SellItem(ItemBase item)
    {
        state = ShopState.Busy;

        if (!item.IsSellable)
        {
            yield return DialogManager.Instance.ShowDialogText($"You can't sell this item!");
            state = ShopState.Selling;
            yield break;
        }

        walletUI.Show();

        float sellingPrice = Mathf.Round(item.Price / 2);
        int countToSell = 1;
        int itemCount = inventory.GetItemCount(item);

        if (itemCount > 1)
        {
            yield return DialogManager.Instance.ShowDialogText($"How many are you willing to sell?",
                waitForInput: false, autoClose: false);

            yield return countSelectorUI.ShowSelector(itemCount, sellingPrice, (selectedCount) => countToSell = selectedCount);

            DialogManager.Instance.CloseDialog();
        }

        sellingPrice = sellingPrice * countToSell;
        int selectedChoice = 0;

        yield return DialogManager.Instance.ShowDialogText($"Best I can do is... {sellingPrice}. No more, no less.",
            waitForInput: false,
            choices: new List<string> { "Yes", "No" },
            onChoiceSelected: choiceIndex => selectedChoice = choiceIndex);

        // Continue selling.
        if (selectedChoice == 0)
        {
            inventory.RemoveItem(item, countToSell);
            Wallet.i.AddMoney(sellingPrice);
            yield return DialogManager.Instance.ShowDialogText($"You sold {item.Name} and recieved {sellingPrice}.");
        }

        walletUI.Close();

        state = ShopState.Selling;
    }

    IEnumerator BuyItem(ItemBase item)
    {
        state = ShopState.Busy;

        yield return DialogManager.Instance.ShowDialogText($"How many would you like to buy?", waitForInput: false, autoClose: false);

        int countToBuy = 1;
        yield return countSelectorUI.ShowSelector(100, item.Price, (selectedCount) => countToBuy = selectedCount);

        DialogManager.Instance.CloseDialog();

        float totalPrice = item.Price* countToBuy;

        if(Wallet.i.HasMoney(totalPrice))
        {
            int selectedChoice = 0;

            yield return DialogManager.Instance.ShowDialogText($"That will cost you {totalPrice}.",
                waitForInput: false,
                choices: new List<string> { "Yes", "No" },
                onChoiceSelected: choiceIndex => selectedChoice = choiceIndex);

            if(selectedChoice == 0)
            {
                // Selected yes.
                inventory.AddItem(item, countToBuy);
                Wallet.i.TakeMoney(totalPrice);
                yield return DialogManager.Instance.ShowDialogText($"Good choice, anything else?");
            }
        }
        else
        {
            yield return DialogManager.Instance.ShowDialogText($"You have insufficient funds!");
        }

        state = ShopState.Buying;
    }

    IEnumerator OnBackFromBuying()
    {
        yield return GameController.Instance.MoveCamera(-shopCameraOffset);

        shopUI.Close();
        walletUI.Close();
        StartCoroutine(StartMenuState());
    }
}
