using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ItemCategory {  healing, PokeBalls, Tms, Item, Key }

public class Inventory : MonoBehaviour, ISavable
{
    [SerializeField] List<ItemSlot> slots;
    [SerializeField] List<ItemSlot> pokeballSlots;
    [SerializeField] List<ItemSlot> tmSlots;
    [SerializeField] List<ItemSlot> itemSlots;
    [SerializeField] List<ItemSlot> keySlots;

    List<List<ItemSlot>> allSlots;

    public event Action OnUpdated;

    private void Awake()
    {
        allSlots = new List<List<ItemSlot>> { slots, pokeballSlots, tmSlots, itemSlots, keySlots };
    }

    public List<ItemSlot> GetSlotsByCategory(int categoryIndex)
    {
        return allSlots[categoryIndex];
    }

    public ItemBase GetItem(int itemIndex, int categoryIndex)
    {
        var currentSlots = GetSlotsByCategory(categoryIndex);
        return currentSlots[itemIndex].Item;
    }

    public ItemBase UseItem(int itemIndex, Pokemon selectedPokemon, int selectedCategory)
    {
        var item = GetItem(itemIndex, selectedCategory);
        bool itemUsed = item.Use(selectedPokemon);

        if (itemUsed)
        {
            if (!item.IsReusable)
            {
                RemoveItem(item);
            }
            return item;
        }

        return null;
    }

    public void AddItem(ItemBase item, int count = 1)
    {
        int category = (int)GetCategoryFromItem(item);
        var currentSlots = GetSlotsByCategory(category);

        var itemSlot = currentSlots.FirstOrDefault(slot => slot.Item == item);
        if (itemSlot != null)
        {
            itemSlot.Count += count;
        }
        else
        {
            currentSlots.Add(new ItemSlot()
            {
                Item = item,
                Count = count
            });
        }

        OnUpdated?.Invoke();
    }

    public int GetItemCount(ItemBase item)
    {
        int category = (int)GetCategoryFromItem(item);
        var currentSlots = GetSlotsByCategory(category);

        var itemSlot = currentSlots.FirstOrDefault(slot => slot.Item == item);

        if (itemSlot != null)
        {
            return itemSlot.Count;
        }
        else
        {
            return 0;
        }
    }

    ItemCategory GetCategoryFromItem(ItemBase item)
    {
        if (item is RecoveryItem)
        {
            return ItemCategory.healing;
        }
        else if (item is PokeballItem)
        {
            return ItemCategory.PokeBalls;
        }
        else if (item is TmItem)
        {
            return ItemCategory.Tms;
        }
        else if (item is EvolutionItem)
        {
            return ItemCategory.Item;
        }
        else
        {
            return ItemCategory.Key;
        }
    }

    public void RemoveItem(ItemBase item, int countToRemove = 1)
    {
        int category = (int)GetCategoryFromItem(item);
        var currentSlots = GetSlotsByCategory(category);

        var itemSlot = currentSlots.First(slot => slot.Item == item);
        itemSlot.Count -= countToRemove;

        if (itemSlot.Count == 0)
        {
            // Not supposed to be just slots.
            currentSlots.Remove(itemSlot);
        }

        OnUpdated?.Invoke();
    }

    // Checks whether player inventory has specified item.
    public bool HasItem(ItemBase item)
    {
        int category = (int)GetCategoryFromItem(item);
        var currentSlots = GetSlotsByCategory(category);

        return currentSlots.Exists(slot => slot.Item == item);
    }

    public static Inventory GetInventory()
    {
        return FindObjectOfType<PlayerController>().GetComponent<Inventory>();
    }

    public object CaptureState()
    {
        var saveData = new InventorySaveData()
        {
            healing = slots.Select(i => i.GetSaveData()).ToList(),
            pokeballs = pokeballSlots.Select(i => i.GetSaveData()).ToList(),
            tms = tmSlots.Select(i => i.GetSaveData()).ToList(),
            items = tmSlots.Select(i => i.GetSaveData()).ToList(),
            key = tmSlots.Select(i => i.GetSaveData()).ToList(),
        };

        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = state as InventorySaveData;

        slots = saveData.healing.Select(i => new ItemSlot(i)).ToList();
        pokeballSlots = saveData.pokeballs.Select(i => new ItemSlot(i)).ToList();
        tmSlots = saveData.tms.Select(i => new ItemSlot(i)).ToList();
        tmSlots = saveData.items.Select(i => new ItemSlot(i)).ToList();
        tmSlots = saveData.key.Select(i => new ItemSlot(i)).ToList();

        allSlots = new List<List<ItemSlot>> { slots, pokeballSlots, tmSlots, itemSlots, keySlots }; 

        OnUpdated?.Invoke();
    }
}

[Serializable]
public class ItemSlot
{
    [SerializeField] ItemBase item;
    [SerializeField] int count;

    public ItemSlot()
    {

    }

    public ItemSlot(ItemSaveData saveData)
    {
        item = ItemDB.GetObjectByName(saveData.name);
        count = saveData.count;
    }

    public ItemSaveData GetSaveData()
    {
        var saveData = new ItemSaveData()
        {
            name = item.name,
            count = count
        };

        return saveData;
    }

    public ItemBase Item {
        get => item;
        set => item = value;
    }

    public int Count {
        get => count;
        set => count = value;
    }
}

[Serializable]
public class ItemSaveData
{
    public string name;
    public int count;
}

[Serializable]
public class InventorySaveData
{
    public List<ItemSaveData> healing;
    public List<ItemSaveData> pokeballs;
    public List<ItemSaveData> tms;
    public List<ItemSaveData> items;
    public List<ItemSaveData> key;
}