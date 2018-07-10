using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveExamine : MonoBehaviour {

    public string examineTag; // Tag that object has

    public string ExamineInfo() // Stores object examine scripts
    {
        return(LocalizationManager.Instance.LocalizeText(examineTag + " Examine"));
    }

    public void Examine() // Prints out examine of object
    {
        var examineResult = ExamineInfo();
        PlayerText.Instance.UIPrint(examineResult);
    }
}
