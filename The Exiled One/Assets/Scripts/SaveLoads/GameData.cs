using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

[Serializable]
public class GameData {

    /* Game session data */
    public string sceneName;

    // Weather data
    public WeatherManager.WeatherData weather;

    // Time
    public TimeManager.TimeData time;

    // Player & Inventory
    public PlayerSaveLoad.PlayerData player;
    public Inventory.InventoryData inventory;
    public Inventory.ItemData[] items;

    // Destroyed objects
    public List<string> destroyedObjects = new List<string>();

    // NPC data


    // Events data (cutscenes)
}
