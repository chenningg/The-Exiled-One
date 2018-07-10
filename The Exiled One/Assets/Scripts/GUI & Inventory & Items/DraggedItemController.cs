using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DraggedItemController : MonoBehaviour {

    public Item itemBeingDragged;
    public Image itemSprite;
    public Text itemCount;

    #region Instance
    private static DraggedItemController draggedItemControllerInstance;

    public static DraggedItemController Instance { get { return draggedItemControllerInstance; } }

    private void Awake()
    {
        if (draggedItemControllerInstance == null)
        {
            draggedItemControllerInstance = this;
        }
    }
    #endregion

    public void SetItem(Item item)
    {
        itemBeingDragged = item;

        DisplayItem();
    }

    public void DisplayItem()
    {
        if (itemBeingDragged)
        {
            itemSprite.sprite = itemBeingDragged.itemSprite;
            itemSprite.enabled = true;

            if (itemBeingDragged.currentCount > 1)
            {
                itemCount.text = itemBeingDragged.currentCount.ToString();
                itemCount.enabled = true;
            }
            else
            {
                itemCount.text = "";
                itemCount.enabled = false;
            }
        }    
    }

    public void ResetItem()
    {
        itemBeingDragged = null;
        itemSprite.sprite = null;
        itemCount.text = "";

        itemSprite.enabled = false;
        itemCount.enabled = false;
    }
}
