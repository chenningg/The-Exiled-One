using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorChanger : MonoBehaviour {

    public Texture2D defaultMouse;
    public Texture2D attackMouse;
    public Texture2D talkMouse;

	// Use this for initialization
	void Start () {
        Cursor.SetCursor(defaultMouse, new Vector2(34, 34), CursorMode.Auto);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
