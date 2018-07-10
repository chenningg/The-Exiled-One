using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {

    // Movement checks
    public bool isMoving; // Is moving or not
    public bool canMove; // Enables movement

    // Public vars
    public float moveSpeed;
    public Vector2 direction;
    public float runSpeed; // Speed at which this character runs away or to

    // Track movespeed
    private float oldMoveSpeed;

    // Animations
    public Animator anim;
    public Vector2 lastMove;

    // Pause variables
    private bool isPaused; // Is the game paused?
    private bool inDialog;

    private void Start()
    {
        EventManager.Instance.e_pauseGame.AddListener(Pause);
        EventManager.Instance.e_resumeGame.AddListener(Pause);
        EventManager.Instance.e_startDialog.AddListener(Dialog);
        EventManager.Instance.e_endDialog.AddListener(Dialog);
        canMove = true;
        isMoving = false;
        oldMoveSpeed = moveSpeed;
    }

    private void OnDisable()
    {
        EventManager.Instance.e_pauseGame.RemoveListener(Pause);
        EventManager.Instance.e_resumeGame.RemoveListener(Pause);
        EventManager.Instance.e_startDialog.RemoveListener(Dialog);
        EventManager.Instance.e_endDialog.RemoveListener(Dialog);
    }

    public void Move(Vector2 vectorDirection) {

        // Character isn't moving
        if (vectorDirection == Vector2.zero)
        {
            isMoving = false;
        }

        if (canMove && isMoving)
        {
            transform.Translate(vectorDirection * moveSpeed * Time.deltaTime); // Move character
            direction = vectorDirection;
        }
        else
        {
            isMoving = false; // If canMove is false, isMoving must be false
        }

        // Call movement animations
        AnimateMovement();

        // Reset movement variables
        isMoving = false;
        direction = new Vector2(0, 0);
        moveSpeed = oldMoveSpeed;
    }

    void AnimateMovement() // All movement animations for characters go here
    {

        // We check for diagonal movement to update animation
        if (canMove && isMoving)
        {
            lastMove = direction; // Update last move

            // IDLE ANIMATIONS
            if (direction.x != 0 && direction.y != 0) // Character moving diagonally
            {
                anim.SetFloat("lastMoveX", lastMove.x);
                anim.SetFloat("lastMoveY", 0f);
            }

            else // Face the last movement direction for idling
            {
                anim.SetFloat("lastMoveX", lastMove.x);
                anim.SetFloat("lastMoveY", lastMove.y);
            }

            // MOVING ANIMATIONS
            anim.SetFloat("moveX", direction.x);
            anim.SetFloat("moveY", direction.y);
        }

        // Set all other animation variables
        anim.SetBool("isMoving", isMoving);
    }

    // PAUSE EVENTS

    private void Pause()
    {
        if (isPaused)
        {
            isPaused = false;
            MovementStart();
        }
        else
        {
            isPaused = true;
            MovementStop();
        }
    }

    private void Dialog()
    {
        if (inDialog)
        {
            inDialog = false;
            MovementStart();
        }
        else
        {
            inDialog = true;
            MovementStop();
        }
    }

    public void MovementStop()
    {
        canMove = false;
        isMoving = false;
        AnimateMovement();
    }

    public void MovementStart()
    {
        if (!isPaused && !inDialog)
        {
            canMove = true;
            AnimateMovement();
        }
    }
}
