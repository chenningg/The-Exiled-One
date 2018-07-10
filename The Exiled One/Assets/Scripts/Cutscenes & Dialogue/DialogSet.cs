using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "New Dialogue Set", menuName = "Dialogue/Dialogue Set")]
public class DialogSet : ScriptableObject {

    public DialogLine[] dialogSet;

    public enum DialogAction // Contains an action to play in order specified by the dialogue set
    {
        Start, // Starts dialog
        Move,
        Speak,
        Spawn,
        End // Ends dialog (calls event)
    }

    [Serializable]
    public class DialogLine // Contains information in one dialogue line
    {
        public DialogAction dialogAction;
        public string dialogTag; // Tag of the dialog
    }
}
