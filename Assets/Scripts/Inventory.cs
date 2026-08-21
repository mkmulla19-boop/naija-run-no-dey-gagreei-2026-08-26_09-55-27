using System;
using System.Collections.Generic;
using UnityEngine;

namespace Olomu.Systems
{
    public class Inventory : MonoBehaviour
    {
        public event Action InventoryChanged;

        private readonly Dictionary<string, int> items = new Dictionary<string, int>();

        public void AddItem(string itemName, int amount = 1)
        {
            if (items.ContainsKey(itemName)) items[itemName] += amount;
            else items[itemName] = amount;
            InventoryChanged?.Invoke();
        }

        public bool RemoveItem(string itemName, int amount = 1)
        {
            if (!HasItem(itemName, amount)) return false;
            items[itemName] -= amount;
            if (items[itemName] <= 0) items.Remove(itemName);
            InventoryChanged?.Invoke();
            return true;
        }

        public bool HasItem(string itemName, int amount = 1)
        {
            return items.TryGetValue(itemName, out int count) && count >= amount;
        }

        public int GetQuantity(string itemName)
        {
            return items.TryGetValue(itemName, out int count) ? count : 0;
        }

        public Dictionary<string, int> GetAllItems() => new Dictionary<string, int>(items);

        public void RestoreAll(Dictionary<string, int> data)
        {
            items.Clear();
            if (data == null) return;
            foreach (var kv in data) items[kv.Key] = kv.Value;
            InventoryChanged?.Invoke();
        }
    }
}
