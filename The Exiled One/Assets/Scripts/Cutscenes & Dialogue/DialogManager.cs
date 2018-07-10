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
    public Text dialogDisplayText;

    // Variables
    [SerializeField]
    [Range(0, 1)]
    private float dialogTextDisplaySpeed;

    private void Start()
    {
        EventManager.Instance.e_startDialog.AddListener(ShowDialogBox);
        EventManager.Instance.e_endDialog.AddListener(HideDialogBox);
    }

    private void OnDisable()
    {
        EventManager.Instance.e_startDialog.RemoveListener(ShowDialogBox);
        EventManager.Instance.e_endDialog.RemoveListener(HideDialogBox);
    }

    private IEnumerator DisplayDialog(string dialogText)
    {
        dialogDisplayText.text = "";

        for (int i = 0; i < dialogText.Length; i++)
        {
            dialogDisplayText.text += dialogText[i];
            yield return null;
        }
    }

    private void ShowDialogBox()
    {
        dialogBox.SetActive(true);
    }

    private void HideDialogBox()
    {
        dialogBox.SetActive(false);
    }
}
