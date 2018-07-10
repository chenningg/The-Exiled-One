using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabManager : MonoBehaviour {

    // Assign prefabs into this list in inspector
    public List<Transform> prefabs = new List<Transform>();

    // Access dictionary by prefab name to spawn
    public Dictionary<string, Transform> prefabDatabase = new Dictionary<string, Transform>();

    // Singleton pattern
    private static PrefabManager prefabManagerInstance;

    public static PrefabManager Instance { get { return prefabManagerInstance; } }

    private void Awake()
    {
        if (prefabManagerInstance != null && prefabManagerInstance != this)
        {
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
            return;
        }

        prefabManagerInstance = this;
    }

    private void Start()
    {
        for (int i = 0; i < prefabs.Count; i++)
        {
            prefabDatabase[prefabs[i].name] = prefabs[i];
        }
    }
}
