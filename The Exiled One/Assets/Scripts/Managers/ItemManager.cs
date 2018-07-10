using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class ItemManager : MonoBehaviour
{

    // Assign prefabs of items here
    public List<Item> itemList = new List<Item>();

    // Access dictionary by prefab name to spawn
    public Dictionary<string, Item> itemDatabase = new Dictionary<string, Item>();

    #region Singleton
    // Singleton pattern
    private static ItemManager itemManagerInstance;

    public static ItemManager Instance { get { return itemManagerInstance; } }

    private void Awake()
    {
        if (itemManagerInstance != null && itemManagerInstance != this)
        {
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
            return;
        }

        itemManagerInstance = this;

        // Populate item database with all items to call
        for (int i = 0; i < itemList.Count; i++)
        {
            itemDatabase[itemList[i].itemName] = itemList[i];
        }

    }
    #endregion
}
