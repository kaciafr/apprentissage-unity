using System.Collections.Generic;
using UnityEngine;
using System;



public class Inventory : MonoBehaviour
{
    public List<ItemData> content = new();

    [SerializeField] private GameObject inventoryPanel;

    [SerializeField]
    private Transform inventorySlotParent;

    // Événements pour la synchronisation temps réel
    public static event Action<List<ItemData>> OnInventoryChanged;

    public const int InventorySize = 35;

    private void Start()
    {
        RefreshInventory();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        }
    }



    public void AddItem(ItemData item)
    {
        if (item == null)
        {
            Debug.LogWarning("⚠️ Tentative d'ajout d'un item null!");
            return;
        }

        if (IsFull())
        {
            Debug.LogWarning("⚠️ Inventaire plein!");
            return;
        }

        // Debug.Log($"📦 Ajout item: {item.nom}, sprite={item.visualObject != null}");
        content.Add(item);
        RefreshInventory();
        NotifyInventoryChanged();
    }

    public void AddItemAtPosition(ItemData item, int position)
    {
        if (position < 0 || position >= InventorySize) return;

        // S'assurer que la liste est assez grande
        while (content.Count <= position)
        {
            content.Add(null);
        }

        content[position] = item;
        RefreshInventory();
        NotifyInventoryChanged();
    }

  
    public void RemoveItemAtPosition(int position)
    {
        if (position >= 0 && position < content.Count)
        {
            content[position] = null;
            RefreshInventory();
            NotifyInventoryChanged();
        }
    }

    public void CloseInventory()
    {
        inventoryPanel.SetActive(false);
    }

    public void RefreshInventory()
    {
        if (inventorySlotParent == null)
        {
            Debug.LogError("❌ inventorySlotParent est null!");
            return;
        }

        // Debug.Log($"🔄 RefreshInventory - {content.Count} items à afficher, {inventorySlotParent.childCount} slots disponibles");

        // Vider tous les slots d'abord
        for (int i = 0; i < inventorySlotParent.childCount; i++)
        {
            if (inventorySlotParent.GetChild(i).TryGetComponent<Slot>(out var slot))
            {
                slot.item = null;
                if (slot.itemVisual != null)
                {
                    slot.itemVisual.sprite = null;
                    slot.itemVisual.color = Color.clear; // Rendre transparent
                }
            }
        }

        // Remplir avec les items actuels
        for (int i = 0; i < content.Count && i < inventorySlotParent.childCount; i++)
        {
            if (content[i] != null && inventorySlotParent.GetChild(i).TryGetComponent<Slot>(out var currentSlot))
            {
                currentSlot.item = content[i];
                if (currentSlot.itemVisual != null && content[i].visualObject != null)
                {
                    currentSlot.itemVisual.sprite = content[i].visualObject;
                    currentSlot.itemVisual.color = Color.white; // Rendre visible
                }
            }
        }

        // Debug.Log($"✅ RefreshInventory terminé");
    }

    public bool IsFull()
    {
        int nonNullItems = 0;
        foreach (var item in content)
        {
            if (item != null) nonNullItems++;
        }
        return nonNullItems >= InventorySize;
    }

    public int GetItemCount(ItemData itemToCount)
    {
        int count = 0;
        foreach (var item in content)
        {
            if (item != null && item == itemToCount)
                count++;
        }
        return count;
    }

    public bool HasItem(ItemData itemToCheck)
    {
        return content.Contains(itemToCheck);
    }

    public void RemoveItem(ItemData itemToRemove)
    {
        if (content.Remove(itemToRemove))
        {
            RefreshInventory();
            NotifyInventoryChanged();
        }
    }

    private void NotifyInventoryChanged()
    {
        OnInventoryChanged?.Invoke(new List<ItemData>(content));
    }

    private void OnDestroy()
    {
        OnInventoryChanged = null;
    }
}
