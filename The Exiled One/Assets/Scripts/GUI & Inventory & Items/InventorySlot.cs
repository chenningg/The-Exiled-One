using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // References
    public int slotNumber;
    public Item item;
    public Text itemCounter;
    public Image itemImage;

    // Variables
    public bool isCraftingSlot = false;
    public bool isEquipmentSlot = false;

    private void OnDisable()
    {
        if (Inventory.Instance.currentlyOverSlotIndex >= 10 || Inventory.Instance.currentlyOverSlotIndex < 0)
        {
            Inventory.Instance.HideItemDescription();
        }
    }

    public void SetItem(Item itemToAdd)
    {
        item = itemToAdd;
        DisplayItem();
    }

    public void RemoveItem()
    {
        item = null;
        DisplayItem();
    }

    public void DisplayItem() {
        // If there is an item...
        if (item != null)
        {
            // If item is used up remove it
            if (item.currentCount <= 0)
            {
                itemImage.sprite = null;
                itemImage.enabled = false;
                if (itemCounter)
                {
                    itemCounter.text = "";
                    itemCounter.enabled = false;
                }

                return;
            }

            itemImage.enabled = true;
            itemImage.sprite = item.itemSprite;

            if (itemCounter)
            {
                if (item.currentCount <= 1)
                {
                    itemCounter.text = "";
                    itemCounter.enabled = false;
                }
                else
                {
                    itemCounter.enabled = true;
                    itemCounter.text = item.currentCount.ToString();
                }
            }
        }
        else
        {
            itemImage.sprite = null;
            itemImage.enabled = false;
            if (itemCounter)
            {
                itemCounter.text = "";
                itemCounter.enabled = false;
            }  
        }
	}

    // Show description

    // On pointer enter, we display information about the item (if any)
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Give inventory the slot number we are over now
        Inventory.Instance.currentlyOverSlotIndex = slotNumber;

        if (item)
        {
            Inventory.Instance.ShowItemDescription(item);
        }
    }

    // On pointer exit, we hide the information display
    public void OnPointerExit(PointerEventData eventData)
    {

        if (Inventory.Instance.currentlyOverSlotIndex >= 10 || Inventory.Instance.currentlyOverSlotIndex == -1)
        {
            Inventory.Instance.currentlyOverSlotIndex = -1;
        }
        else
        {
            Inventory.Instance.currentlyOverSlotIndex = -2;
        }

        Inventory.Instance.HideItemDescription();
    }
}
