using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDetectionTrigger : MonoBehaviour {

    public bool playerDetected; // Returns true when player is detected

    private void Start()
    {
        playerDetected = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.root.CompareTag("Player"))
        {
            playerDetected = true; 
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.root.CompareTag("Player"))
        {
            playerDetected = false;
        }
    }
}
