using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDB 
{
    static Dictionary<string, ItemBase> items;

    public static void Init()
    {
        items = new Dictionary<string, ItemBase>();

        // Allows us to interact with the objects from the assets folder.
        var itemList = Resources.LoadAll<ItemBase>("");

        foreach (var item in itemList)
        {
            if (items.ContainsKey(item.Name))
            {
                Debug.LogError("We found 2 pokemons with the same name: " + item.Name + ".");
                continue;
            }

            items[item.Name] = item;
        }
    }

    public static ItemBase GetItemByName(string name)
    {
        if (!items.ContainsKey(name))
        {
            Debug.LogError($"Item with the name {name} not found in the database");
            return null;
        }

        return items[name];
    }
}
