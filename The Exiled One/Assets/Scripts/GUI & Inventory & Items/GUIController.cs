using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GUIController : MonoBehaviour {

    #region Singleton
    // Singleton pattern
    private static GUIController guiControllerInstance;

    public static GUIController Instance { get { return guiControllerInstance; } }

    private void Awake()
    {
        if (guiControllerInstance != null && guiControllerInstance != this)
        {
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
            return;
        }

        guiControllerInstance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    // References
    public GameObject UIContainer;

    private void Start()
    {
        SceneManager.activeSceneChanged += CheckScene;
        EventManager.Instance.e_startDialog.AddListener(HideUI);
        EventManager.Instance.e_endDialog.AddListener(ShowUI);
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= CheckScene;
        EventManager.Instance.e_startDialog.RemoveListener(HideUI);
        EventManager.Instance.e_endDialog.RemoveListener(ShowUI);
    }

    void CheckScene(Scene currentScene, Scene nextScene)
    {
        if (nextScene.name == "main_menu")
        {
            Destroy(gameObject);
        }
    }

    // Dialog and cutscene events
    private void HideUI()
    {
        UIContainer.SetActive(false);
    }

    private void ShowUI()
    {
        UIContainer.SetActive(true);
    }
}
