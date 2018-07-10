using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerSaveLoad : MonoBehaviour
{
    public void Start()
    {
        EventManager.Instance.e_saveGame.AddListener(Save);
        EventManager.Instance.e_loadGame.AddListener(Load);
    }

    public void OnDisable()
    {
        EventManager.Instance.e_saveGame.RemoveListener(Save);
        EventManager.Instance.e_loadGame.RemoveListener(Load);
    }

    public void Save()
    {
        var data = new PlayerData();

        // Character info
        data.playerName = PlayerController.Instance.playerName;
        data.position = transform.position;

        // Stats
        data.maxHealth = PlayerController.Instance.health.maxValue;
        data.currentHealth = PlayerController.Instance.health.currentValue;
        data.maxHunger = PlayerController.Instance.hunger.maxValue;
        data.currentHunger = PlayerController.Instance.hunger.currentValue;
        data.maxThirst = PlayerController.Instance.thirst.maxValue;
        data.currentThirst = PlayerController.Instance.thirst.currentValue;

        GameManager.Instance.gameData.player = data;
    }

    public void Load()
    {
        var data = GameManager.Instance.gameData.player;
        // Load character info
        PlayerController.Instance.playerName = data.playerName;
        PlayerController.Instance.gameObject.transform.position = data.position;

        // Load Stats
        PlayerController.Instance.health.maxValue = data.maxHealth;
        PlayerController.Instance.health.currentValue = data.currentHealth;
        PlayerController.Instance.hunger.maxValue = data.maxHunger;
        PlayerController.Instance.hunger.currentValue = data.currentHunger;
        PlayerController.Instance.thirst.maxValue = data.maxThirst;
        PlayerController.Instance.thirst.currentValue = data.currentThirst;
    }

    [System.Serializable]
    public class PlayerData
    {
        // Character info & position
        public string playerName;
        public Vector3 position;

        // Stats
        public float maxHealth;
        public float currentHealth;
        public float maxHunger;
        public float currentHunger;
        public float maxThirst;
        public float currentThirst;
    }
}