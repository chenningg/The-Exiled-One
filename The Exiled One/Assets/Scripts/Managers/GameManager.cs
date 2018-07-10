using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[Serializable]
public class GameManager : MonoBehaviour {

    // Save variables
    public string saveName;
    string path;
    private string jsonData;
    //private IEnumerator loadGameHandler;
 
    // Save data
    [HideInInspector]
    public GameData gameData;   

    #region Singleton
    // Singleton pattern
    private static GameManager gameManagerInstance;

    public static GameManager Instance { get { return gameManagerInstance; } }

    private void Awake()
    {
        if (gameManagerInstance != null && gameManagerInstance != this)
        {
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
            return;
        }

        gameManagerInstance = this;
        DontDestroyOnLoad(gameObject);

        if (gameData == null)
        {
            gameData = new GameData();
        }
    }
    #endregion

    private void Start()
    {
        saveName = "Save 1";
        path = Application.persistentDataPath + "/Saves/";
        gameData = new GameData();
        QualitySettings.vSyncCount = 0; // Turn off v-sync
        Application.targetFrameRate = 60; // Set fps to 60;
    }

    private void Update()
    {
        if (Input.GetKeyDown("k"))
        {
            SaveGame();
            print("Game saved.");
        }

        if (Input.GetKeyDown("j"))
        {
            LoadGame();
            print("Game Loaded");
        }
    }

    public void SaveGame()
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        if (!File.Exists(path + saveName + ".dat"))
        {
            File.Create(path + saveName + ".dat").Dispose();
        }

        if (EventManager.Instance.e_saveGame != null)
        {
            EventManager.Instance.e_saveGame.Invoke();
        }

        gameData.sceneName = SceneManager.GetActiveScene().name;

        string jsonData = JsonUtility.ToJson(gameData);
        File.WriteAllText(path + saveName + ".dat", jsonData);
    }

    public void NewGame()
    {
        var defaultPath = Application.streamingAssetsPath + "/Default Save.dat";
        if (File.Exists(defaultPath))
        {
            jsonData = File.ReadAllText(defaultPath);
            gameData = new GameData();
            gameData = JsonUtility.FromJson<GameData>(jsonData);
        }
        else
        {
            Debug.LogError("Save file is missing at: " + defaultPath);
        }

        LoadSceneWithData("main_level");
    }

    public void LoadGame()
    {
        if (File.Exists(path + saveName + ".dat"))
        {
            jsonData = File.ReadAllText(path + saveName + ".dat");
            gameData = new GameData();
            gameData = JsonUtility.FromJson<GameData>(jsonData);
        }
        else
        {
            Debug.LogError("Save file is missing at: " + path + "/" + saveName + ".dat");
        }

        LoadSceneWithData("main_level");
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void LoadSceneWithData(string sceneName)
    {
        SceneManager.sceneLoaded += LoadGameData;
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void LoadGameData(Scene scene, LoadSceneMode mode)
    {
        LoadGameHandler(); 
    }

    public void LoadGameHandler() {

        // Call load event
        EventManager.Instance.e_loadGame.Invoke();

        // Localize text
        EventManager.Instance.e_localize.Invoke();

        // Call loaded finish event
        EventManager.Instance.e_gameLoaded.Invoke();

        SceneManager.sceneLoaded -= LoadGameData;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
