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
    private Dictionary<string, Transform> spawnedEntities;

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
            spawnedEntities = new Dictionary<string, Transform>();

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
                    string[] splitLine = currentDialogLine.dialogTag.Split('|');

                    if (splitLine.Length == 3)
                    {
                        SpawnEntity(splitLine[0], new Vector2(float.Parse(splitLine[1]), float.Parse(splitLine[2])));
                    }
                    else
                    {
                        Debug.LogError("Error reading tag for spawn action in dialog. Make sure character name, x coord and y coords are included.");
                    }
                    
                    return;
            }
        }
        else // This dialog set is finished, we end
        {
            inDialog = false;
            ResetDialogDisplay();
            currentActionFinished = true;
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
        currentActionFinished = true;
        RunDialogLine(); // Call next line;
    }

    public void HideDialogBox()
    {
        dialogBox.SetActive(false);
        ResetDialogDisplay();
        currentActionFinished = true;
        RunDialogLine(); // Call next line;    
    }

    public void SpawnEntity(string prefabName, Vector2 coordinates)
    {
        var spawnPos = CameraController.Instance.mainCamera.ViewportToWorldPoint(new Vector3(coordinates.x, coordinates.y, 0));

        if (spawnedEntities.ContainsKey(prefabName)) // Already contains same key, we log an error
        {
            Debug.LogError("Spawn entity in dialog aleady contains this entity name. If spawning the same type of entity, put a number behind.");
        }  
        else // We check string for numbers and spawn entity
        {
            string newPrefabName = "";

            foreach (char s in prefabName)
            {
                int result;

                if (int.TryParse(s.ToString(), out result))
                {
                    break;
                }

                newPrefabName += s;
            }

            if (PrefabManager.Instance.prefabDatabase.ContainsKey(newPrefabName))
            {
                var spawnedEntity = Instantiate(PrefabManager.Instance.prefabDatabase[newPrefabName], spawnPos, Quaternion.identity);
                spawnedEntities.Add(prefabName, spawnedEntity);
            }
            else
            {
                Debug.LogError("Prefab name error in dialog spawn.");
            }
        }

        currentActionFinished = true;
        RunDialogLine();
    }
}
