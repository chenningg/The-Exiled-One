using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SortingOrderObjects : MonoBehaviour {

	// Use this for initialization
	void Awake()
    {
        // We assign the y position to the sorting order within the layer
        GetComponent<SpriteRenderer>().sortingOrder = (int)(transform.position.y * -15);
    }

    public void RefreshSortingOrder()
    {
        GetComponent<SpriteRenderer>().sortingOrder = (int)(transform.position.y * -15);
    }
}
