using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour {

    #region Singleton
    // Singleton pattern
    private static DialogManager dialogManagerInstance;

    public static DialogManager Instance { get { return dialogManagerInstance; } }

    private void Awake()
    {
        if (dialogManagerInstance != null && dialogManagerInstance != this)
        {
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
            return;
        }

        dialogManagerInstance = this;
    }
    #endregion

    // References
    public GameObject dialogBox;
    public GameObject continueArrow;
    public Text dialogDisplayText;
    public List<DialogSet> dialogSetsList = new List<DialogSet>();
    private Dictionary<string, DialogSet> dialogSets = new Dictionary<string, DialogSet>();
    private IEnumerator displayDialog;
    private Queue<DialogSet.DialogLine> currentDialogSet;
    private DialogSet.DialogLine currentDialogLine;

    // Variables
    [HideInInspector]
    public bool inDialog = false;
    [SerializeField][Range(0,5)]
    private int textDisplaySpeed = 2;

    private bool currentActionFinished = true;
    private bool displayTextImmediately = false;

    private void Start()
    {
        continueArrow.SetActive(false);

        foreach (DialogSet dS in dialogSetsList)
        {
            dialogSets.Add(dS.dialogSetKey, dS);
        }
    }

    private void Update()
    {
        if (inDialog)
        {
            if (Input.GetButtonDown("Left Click"))
            {
                if (displayDialog != null) // If dialog is playing, we skip to end of dialog
                {
                    displayTextImmediately = true;
                    return;
                }
                else if (!currentActionFinished) // If haven't finish current action, don't allow continuation
                {
                    return;
                }

                // Else we move on to the next dialog line in dialog set
                RunDialogLine();
            }
        }
    }

    public void StartDialogSet(string dialogSetKey)
    {
        if (!inDialog)
        {
            currentDialogSet = new Queue<DialogSet.DialogLine>();

            if (dialogSets.ContainsKey(dialogSetKey))
            {
                foreach (DialogSet.DialogLine dialogLine in dialogSets[dialogSetKey].dialogSet)
                {
                    currentDialogSet.Enqueue(dialogLine);
                }
            }
            else
            {
                Debug.LogError("Missing dialog set: Dialog set not found.");
            }

            // Invoke start dialog
            inDialog = true;
            EventManager.Instance.e_startDialog.Invoke();

            RunDialogLine();
        }
        else
        {
            Debug.LogError("Already in a dialog!");
        }
    }

    private void RunDialogLine()
    {
        currentActionFinished = false;

        if (currentDialogSet.Count > 0) // Run the dialog line
        {
            currentDialogLine = currentDialogSet.Dequeue();

            // Carry out appropriate actions based on the dialog action type
            switch (currentDialogLine.dialogAction)
            {
                case (DialogSet.DialogAction.ShowBox):
                    ShowDialogBox();
                    return;

                case (DialogSet.DialogAction.HideBox):
                    HideDialogBox();
                    return;

                case (DialogSet.DialogAction.Move):
                    return;

                case (DialogSet.DialogAction.Speak):
                    if (currentDialogLine.dialogTag != "")
                    {
                        DisplayDialog(LocalizationManager.Instance.localizedText[currentDialogLine.dialogTag]);
                    }
                    else
                    {
                        Debug.LogError("Missing string for dialog line tag.");
                    } 
                    return;

                case (DialogSet.DialogAction.Spawn):
                    return;
            }
        }
        else // This dialog set is finished, we end
        {
            inDialog = false;
            EventManager.Instance.e_endDialog.Invoke();
        }
    }

    public void DisplayDialog(string dialogText)
    {
        if (displayDialog != null)
        {
            displayTextImmediately = true;
            return;
        }

        displayDialog = DisplayDialogHandler(dialogText);
        StartCoroutine(displayDialog);
    }

    private IEnumerator DisplayDialogHandler(string dialogText)
    {
        dialogDisplayText.text = "";
        continueArrow.SetActive(false);

        int endIndex = 0;

        while (endIndex < dialogText.Length)
        {
            if (displayTextImmediately)
            {
                dialogDisplayText.text = dialogText;
                break;
            }

            endIndex += textDisplaySpeed;

            if (endIndex >= dialogText.Length)
            {
                endIndex = dialogText.Length;
            }

            dialogDisplayText.text = dialogText.Substring(0, endIndex);
            yield return null;
        }

        displayTextImmediately = false;
        continueArrow.SetActive(true);
        displayDialog = null;
        currentActionFinished = true;
    }

    public void ResetDialogDisplay()
    {
        if (displayDialog != null)
        {
            StopCoroutine(displayDialog);
        }

        dialogDisplayText.text = "";
        displayDialog = null;
        continueArrow.SetActive(false);
        displayTextImmediately = false;
    }

    public void ShowDialogBox()
    {
        dialogBox.SetActive(true);
        ResetDialogDisplay();
        RunDialogLine(); // Call next line;
        currentActionFinished = true;
    }

    public void HideDialogBox()
    {
        dialogBox.SetActive(false);
        ResetDialogDisplay();
        RunDialogLine(); // Call next line;
        currentActionFinished = true;
    }
}
