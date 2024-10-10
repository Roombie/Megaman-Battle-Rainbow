using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    // used to store item counts
    private Dictionary<Item.ItemType, int> inventory = new Dictionary<Item.ItemType, int>();

    public delegate void OnInventoryUpdate(Item.ItemType itemType, int newAmount);
    public static event OnInventoryUpdate InventoryUpdated;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddItem(Item.ItemType itemType, int amount = 1)
    {
        if (inventory.ContainsKey(itemType))
        {
            inventory[itemType] += amount;
        }
        else
        {
            inventory[itemType] = amount;
        }

        // Trigger an update event for UI or other systems to reflect changes
        InventoryUpdated?.Invoke(itemType, inventory[itemType]);
    }

    public bool UseItem(Item.ItemType itemType)
    {
        if (inventory.ContainsKey(itemType) && inventory[itemType] > 0)
        {
            inventory[itemType]--;
            InventoryUpdated?.Invoke(itemType, inventory[itemType]);
            return true;
        }
        return false;
    }

    public int GetItemCount(Item.ItemType itemType)
    {
        return inventory.ContainsKey(itemType) ? inventory[itemType] : 0;
    }
}
