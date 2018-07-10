using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThirstBarController : MonoBehaviour {
    
    #region Instance
    private static ThirstBarController thirstBarControllerInstance;

    public static ThirstBarController Instance { get { return thirstBarControllerInstance; } }

    private void Awake()
    {
        if (thirstBarControllerInstance == null)
        {
            thirstBarControllerInstance = this;
        }
    }
    #endregion

    public Image image;
}
