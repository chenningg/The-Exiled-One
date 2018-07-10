using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectSaveLoad : MonoBehaviour {

    [HideInInspector]
    public bool isDestroyed = false;

    public void OnEnable()
    {
        EventManager.Instance.e_loadGame.AddListener(Load);
    }

    public void OnDisable()
    {
        EventManager.Instance.e_loadGame.RemoveListener(Load);
    }

    public void Save() // To be called if object is destroyed by player
    {
        if (isDestroyed)
        {
            string objectKey;
            objectKey = gameObject.name + "/" + transform.position.x + "/" + transform.position.y;

            GameManager.Instance.gameData.destroyedObjects.Add(objectKey);
        }
    }

    public void Load()
    {
        string objectKey;
        objectKey = gameObject.name + "/" + transform.position.x + "/" + transform.position.y;

        if (GameManager.Instance.gameData.destroyedObjects.Contains(objectKey))
        {
            Destroy(gameObject);
        }
    }
}
