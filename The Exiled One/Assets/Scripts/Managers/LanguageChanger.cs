using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LanguageChanger : MonoBehaviour {

    private Dropdown languageDropdown;

    private void Start()
    {
        languageDropdown = GetComponent<Dropdown>();
    }

    public void LanguageChange()
    {
        // Call change language event after updating language change
        PlayerPrefs.SetString("language", languageDropdown.options[languageDropdown.value].text);

        if (EventManager.Instance.e_languageChange != null)
        {
            EventManager.Instance.e_languageChange.Invoke();
        }

        if (EventManager.Instance.e_localize != null)
        {
            EventManager.Instance.e_localize.Invoke();
        }
    }
}
