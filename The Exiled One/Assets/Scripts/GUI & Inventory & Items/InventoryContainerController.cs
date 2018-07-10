using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryContainerController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    private void OnDisable()
    {
        if (Inventory.Instance.currentlyOverSlotIndex >= 10)
        {
            Inventory.Instance.currentlyOverSlotIndex = -1;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Inventory.Instance.currentlyOverSlotIndex = -1;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!(Inventory.Instance.currentlyOverSlotIndex >= 10))
        {
            Inventory.Instance.currentlyOverSlotIndex = -2;
        }      
    }
}
