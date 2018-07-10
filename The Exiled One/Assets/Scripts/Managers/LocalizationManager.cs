using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

public class LocalizationManager : MonoBehaviour {

    #region Instance
    private static LocalizationManager localizationManagerInstance;

    public static LocalizationManager Instance { get { return localizationManagerInstance; } }

    private void Awake()
    {
        if (localizationManagerInstance != null && localizationManagerInstance != this)
        {
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
            return;
        }
        localizationManagerInstance = this;

        // If no language pref default to English
        if (PlayerPrefs.GetString("language") == null)
        {
            PlayerPrefs.SetString("language", "English");
        }

        LoadLanguage();
    }
    #endregion

    public Dictionary<string, string> localizedText = new Dictionary<string, string>();
    private string path;
    private string language;

	// Use this for initialization
	void Start () {
        EventManager.Instance.e_languageChange.AddListener(LoadLanguage);
	}

    private void OnDisable()
    {
        EventManager.Instance.e_languageChange.RemoveListener(LoadLanguage);
    }

    public void LoadLanguage()
    {
        string line;
        language = PlayerPrefs.GetString("language");
        path = Application.streamingAssetsPath + "/Locales";

        // Check if language exists
        if (!Directory.Exists(path))
        {
            throw new Exception("The locales folder is missing at '" + path + "'. Please reinstall the game to get the folder back.");
        }

        if (!File.Exists(path + "/" + language + ".csv"))
        {
            throw new Exception("The language file is missing at '" + path + "/" + language + ".csv'. This language is either not currently supported or you are missing the language file. Consider reinstallation of the game if you know that this language is currently supported in this version of the game.");
        }

        // Clear dictionary
        localizedText.Clear();

        // Read each line in the translated csv file and add to dictionary
        StreamReader reader = new StreamReader(path + "/" + language + ".csv");
        while ((line = reader.ReadLine()) != null)
        {
            string[] splitline = line.Split('|');
            if (splitline[0] == "*****" || splitline[1] == "*****")
            {
                // Do nothing, treat as comment
            }
            else
            {
                localizedText.Add(splitline[0], splitline[1]);
            }           
        }

        reader.Close();

        // Call for all menu and stuff to update their language
        EventManager.Instance.e_localize.Invoke();
    }

    public string LocalizeText(string textKey)
    {
        string text;

        if (localizedText.TryGetValue(textKey, out text))
        {
            return (text);
        }
        else
        {
            Debug.LogError("No key in localization dictionary.");
            return (textKey);
        }
    }
}
