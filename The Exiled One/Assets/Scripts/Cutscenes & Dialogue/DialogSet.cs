using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "New Dialogue Set", menuName = "Dialogue/Dialogue Set")]
public class DialogSet : ScriptableObject {

    public string dialogSetKey;
    public DialogLine[] dialogSet;

    public enum DialogAction // Contains an action to play in order specified by the dialogue set
    {
        ShowBox, // Shows dialogue box
        HideBox, // Hides dialogue box
        Move,
        Speak, // Prints dialogue
        Spawn
    }

    [Serializable]
    public class DialogLine // Contains information in one dialogue line
    {
        public DialogAction dialogAction;
        public string dialogTag; // Tag of the dialog
    }
}
