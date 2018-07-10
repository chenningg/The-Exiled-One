using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SortingOrderCharacters : MonoBehaviour {

    private float playerPosY;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        GetComponent<SpriteRenderer>().sortingOrder = (int)(transform.position.y * -15);      
    }
}
