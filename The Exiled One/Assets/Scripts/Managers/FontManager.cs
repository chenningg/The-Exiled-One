using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FontManager : MonoBehaviour {

    public List<Font> fonts;

	// Use this for initialization
	void Start () {
		for (int i = 0; i < fonts.Count; i++)
        {
            fonts[i].material.mainTexture.filterMode = FilterMode.Point;
        }
	}

}
