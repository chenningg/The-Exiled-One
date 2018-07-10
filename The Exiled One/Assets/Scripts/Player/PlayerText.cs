using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerText : MonoBehaviour {

    public Text textField;
    public float textTime = 3;

    private IEnumerator printText;

    #region Instance
    private static PlayerText playerTextInstance;

    public static PlayerText Instance { get { return playerTextInstance; } }

    private void Awake()
    {
        if (playerTextInstance != null && playerTextInstance != this)
        {
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
            return;
        }
        playerTextInstance = this;
    }
    #endregion

    public void UIPrint(string textToPrint)
    {
        if (printText != null)
        {
            StopCoroutine(printText);
        }
        printText = PrintText(textToPrint);
        StartCoroutine(printText);
    }

    public IEnumerator PrintText(string textToPrint)
    {
        textField.text = textToPrint;
        yield return new WaitForSecondsRealtime(textTime);
        textField.text = "";
        printText = null;
    }
}
