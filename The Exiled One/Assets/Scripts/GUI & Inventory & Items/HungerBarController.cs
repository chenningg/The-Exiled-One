using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HungerBarController : MonoBehaviour {

    #region Instance
    private static HungerBarController hungerBarControllerInstance;

    public static HungerBarController Instance { get { return hungerBarControllerInstance; } }

    private void Awake()
    {
        if (hungerBarControllerInstance == null)
        {
            hungerBarControllerInstance = this;
        }
    }
    #endregion

    public Image image;

}
