using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    // References
    public CameraShaker cameraShakeScript;
    public Camera mainCamera;

    public float smoothTime = 0.05f;
    public bool followPlayer = true;

    private Vector2 velocity;
    private Vector2 newPosition;

    #region Singleton
    // Singleton pattern
    private static CameraController cameraControllerInstance;

    public static CameraController Instance { get { return cameraControllerInstance; } }

    private void Awake()
    {
        if (cameraControllerInstance != null && cameraControllerInstance != this)
        {
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
            return;
        }

        cameraControllerInstance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    private void Start()
    {
        followPlayer = true;
    }

    // Follow the player with smoothing
    void FixedUpdate () {
        if (followPlayer)
        {
            if (PlayerController.Instance != null)
            {
                newPosition.x = Mathf.SmoothDamp(transform.position.x, PlayerController.Instance.transform.position.x, ref velocity.x, smoothTime);
                newPosition.y = Mathf.SmoothDamp(transform.position.y, PlayerController.Instance.transform.position.y, ref velocity.y, smoothTime);

                transform.position = newPosition;
            }
        }       
	}
}
