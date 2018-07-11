using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour {

    #region Singleton
    // Singleton pattern
    private static PlayerController playerControllerInstance;

    public static PlayerController Instance { get { return playerControllerInstance; } }

    private void Awake()
    {
        if (playerControllerInstance != null && playerControllerInstance != this)
        {
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
            return;
        }

        playerControllerInstance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    // Character info
    public string playerName;
    
    // Stats
    public Stat health, hunger, thirst;

    // References
    public AudioPlayer audioPlayer;
    public Movement moveScript;
    public TakeDamage takeDamageScript;
    public Animator anim;

    // Variables
    bool isExamining;
    bool allowInput = true; // Allow player input?

    // Layer mask
    private LayerMask hitboxLayer = (1 << 9);

    private void Start()
    {
        // Set references
        health.statDisplay = HealthBarController.Instance.image;
        hunger.statDisplay = HungerBarController.Instance.image;
        thirst.statDisplay = ThirstBarController.Instance.image;

        // Subscribe to events
        SceneManager.activeSceneChanged += CheckScene;
        EventManager.Instance.e_startDialog.AddListener(DisallowInput);
        EventManager.Instance.e_endDialog.AddListener(AllowInput);
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= CheckScene;
        EventManager.Instance.e_startDialog.RemoveListener(DisallowInput);
        EventManager.Instance.e_endDialog.RemoveListener(AllowInput);
    }

    void CheckScene(Scene currentScene, Scene nextScene)
    {
        if (nextScene.name == "main_menu")
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update ()
    {
        GetInput();
        moveScript.Move(moveScript.direction);
        AnimatePlayerExamining();
    }

    void GetInput() // Get player's inputs
    {
        if (allowInput)
        {
            // Input for interacting with objects
            if (Input.GetButtonDown("Left Click"))
            {
                var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
                if (hit)
                {
                    // TESTING

                    var takeDamage = hit.transform.gameObject.GetComponent<TakeDamage>();

                    if (takeDamage)
                    {
                        takeDamage.Damage(1);
                    }
                    else
                    {
                        var takeDamageTwo = hit.transform.parent.gameObject.GetComponent<TakeDamage>();

                        if (takeDamageTwo)
                        {
                            takeDamageTwo.Damage(1);
                        }
                    }

                    // TESTING END

                    var interactive = hit.transform.gameObject.GetComponent<InteractiveCheck>();
                    if (interactive == null)
                    {
                        return;
                    }
                    else
                    {
                        interactive.DetermineInteraction();
                    }
                }
            }

            // Input for examining objects
            if (Input.GetButtonDown("Right Click"))
            {
                //TEST
                DialogManager.Instance.StartDialogSet("Test Dialog");

                isExamining = true;
                var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, hitboxLayer);
                if (hit)
                {
                    var examinable = hit.transform.GetComponent<InteractiveExamine>();
                    if (examinable)
                    {
                        examinable.Examine();
                    }
                    else
                    {
                        var examinableChild = hit.transform.parent.GetComponent<InteractiveExamine>();
                        if (examinableChild)
                        {
                            examinableChild.Examine();
                        }
                    }
                }
            }

            // Input for vertical movement
            if (Input.GetAxisRaw("Vertical") > 0.5f || Input.GetAxisRaw("Vertical") < -0.5f)
            {
                moveScript.isMoving = true;
                moveScript.direction.y = Input.GetAxisRaw("Vertical"); // Update movement y direction
            }

            // Get input for horizontal movement
            if (Input.GetAxisRaw("Horizontal") > 0.5f || Input.GetAxisRaw("Horizontal") < -0.5f)
            {
                moveScript.isMoving = true;
                moveScript.direction.x = Input.GetAxisRaw("Horizontal"); // Update movement x direction
            }
        }        
    }

    void AnimatePlayerExamining()
    {
        anim.SetBool("isExamining", isExamining);

        // Reset animation variables
        isExamining = false;
    }

    void AllowInput()
    {
        //allowInput = true;
    }

    void DisallowInput()
    {
        //allowInput = false;
    }
}
