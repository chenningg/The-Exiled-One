using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stat : MonoBehaviour {

    // Only for showing stats
    public Image statDisplay;
    public string statName;

    // Variables
    [SerializeField]
    private float myMaxValue = 100;
    [SerializeField]
    private float myCurrentValue = 100;

    public float currentValue
    {
        get
        {
            return myCurrentValue;
        }

        set
        {
            if (value > myMaxValue)
            {
                myCurrentValue = myMaxValue; // Prevents setting overshot
            }
            else if (value < 0)
            {
                myCurrentValue = 0;
            }
            else
            {
                myCurrentValue = value;
            }

            // If after adding it's more then we reset to max
            if (myCurrentValue > myMaxValue)
            {
                myCurrentValue = myMaxValue;
            }

            if (myCurrentValue <= 0)
            {
                myCurrentValue = 0;
            }

            if (statDisplay != null) // Display updated stat value
            {
                statDisplay.fillAmount = myCurrentValue / myMaxValue;
            }
        }
    }

    public float maxValue
    {
        get
        {
            return myMaxValue;
        }

        set
        {
            myMaxValue = value;

            if (statDisplay != null) // Display updated stat value
            {
                statDisplay.fillAmount = myCurrentValue / myMaxValue;
            }
        }
    }
}
