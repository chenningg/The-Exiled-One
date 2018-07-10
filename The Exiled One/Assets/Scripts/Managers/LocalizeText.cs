using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalizeText : MonoBehaviour {

    // PUT THIS SCRIPT ON STATIC TEXT ONLY~

    private Text textToLocalize;
    public string textToLocalizeKey;

    // Called on startup
    private void Start()
    {
        EventManager.Instance.e_localize.AddListener(Localize);
        textToLocalize = GetComponent<Text>();
        Localize();
    }

    private void OnDisable()
    {
        EventManager.Instance.e_localize.RemoveListener(Localize);
    }

    void Localize()
    {
        textToLocalize.text = LocalizationManager.Instance.LocalizeText(textToLocalizeKey);
    }
}
