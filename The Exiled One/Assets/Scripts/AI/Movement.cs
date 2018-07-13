using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {

    // References
    public Animator anim;
    private IEnumerator moveToLocationHandler;
    public BoxCollider2D characterCollider;
    private Vector2[] vectorList;
    private Vector2[] colliderCorners = new Vector2[4];
    private int vectorListIndex = 0; // Points to direction of raycast check
    private LayerMask obstacleLayer = (1 << 8);

    // Movement checks
    public bool isMoving; // Is moving or not
    public bool canMove; // Enables movement
    private bool lockMovementDirection = false;
    private bool reachedLocation = false;

    // Variables
    public float moveSpeed;
    public Vector2 direction;
    public float runSpeed; // Speed at which this character runs away or to

    private float rayLength = 2f; // What is the ray length to check for obstacles?
    private float oldMoveSpeed;
    public Vector2 lastMove;
    public Vector3 moveToLocation; // Direction for specific location movement

    // Pause variables
    private bool isPaused; // Is the game paused?
    private bool inDialog;

    private void Start()
    {
        // Subscribe to events
        EventManager.Instance.e_pauseGame.AddListener(Pause);
        EventManager.Instance.e_resumeGame.AddListener(Resume);
        EventManager.Instance.e_startDialog.AddListener(DialogStart);
        EventManager.Instance.e_endDialog.AddListener(DialogStop);

        // Set variables
        canMove = true;
        isMoving = false;
        oldMoveSpeed = moveSpeed;
        vectorList = new[] { Vector2.up, new Vector2(1, 1),
        Vector2.right, new Vector2(1, -1), Vector2.down,
        new Vector2(-1, -1), Vector2.left, new Vector2(-1, 1)};

        if (DialogManager.Instance.inDialog)
        {
            MovementStop();
        }
    }

    private void OnDisable()
    {
        EventManager.Instance.e_pauseGame.RemoveListener(Pause);
        EventManager.Instance.e_resumeGame.RemoveListener(Resume);
        EventManager.Instance.e_startDialog.RemoveListener(DialogStart);
        EventManager.Instance.e_endDialog.RemoveListener(DialogStop);
    }

    public void Move(Vector2 vectorDirection) {

        // Character isn't moving
        if (vectorDirection == Vector2.zero)
        {
            isMoving = false;
        }

        if (canMove && isMoving)
        {
            direction = BalanceDirection(vectorDirection);
            transform.Translate(direction * moveSpeed * Time.deltaTime); // Move character         
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

    public void MoveToLocation(Vector3 location)
    {
        if (moveToLocationHandler == null)
        {
            moveToLocationHandler = MoveToLocationHandler(location);
            StartCoroutine(moveToLocationHandler);
        }
    }

    private IEnumerator MoveToLocationHandler(Vector3 location)
    {
        moveToLocation = location;
        reachedLocation = false;

        while (!reachedLocation)
        {
            if (Vector3.Distance(transform.position, location) <= 1f)
            {
                reachedLocation = true;
                break;
            }

            if (!CollisionCheck())
            {
                direction = BalanceDirection(-(transform.position - location));
                isMoving = true;
                moveSpeed = runSpeed;
                transform.Translate(direction * moveSpeed * Time.deltaTime); // Move character         

                // Call movement animations
                AnimateMovement();

                // Reset movement variables
                isMoving = false;
                direction = new Vector2(0, 0);
                moveSpeed = oldMoveSpeed;
            }
            else
            {
                AvoidObstacle();
            }

            yield return null;
        }

        if (DialogManager.Instance.inDialog)
        {
            DialogManager.Instance.currentActionFinished = true;
        }

        lockMovementDirection = false;
        moveToLocationHandler = null;


    }

    #region Balance Direction
    // Sets direction to whole numbers only
    private Vector2 BalanceDirection(Vector2 runDirection)
    {
        // If x vector is close to 0 we put it to 0 and y vector to 1 or -1
        if (runDirection.x >= -0.3 && runDirection.x <= 0.3)
        {
            runDirection.x = 0;
        }
        else
        {
            if (runDirection.x > 0)
            {
                runDirection.x = 1;
            }
            else
            {
                runDirection.x = -1;
            }
        }

        // If y vector close to 0 we put it 0 and x vector to 1 or -1
        if (runDirection.y >= -0.3 && runDirection.y <= 0.3)
        {
            runDirection.y = 0;
        }
        else
        {
            if (runDirection.y > 0)
            {
                runDirection.y = 1;
            }
            else
            {
                runDirection.y = -1;
            }
        }

        return (runDirection);
    }
    #endregion

    private bool CollisionCheck()
    {
        // Raycast from corners of box collider to player to check for collision
        GetColliderCorners();

        for (int i = 0; i < colliderCorners.Length; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(colliderCorners[i], -(transform.position - moveToLocation), rayLength, obstacleLayer);

            if (hit) // Obstacle hit by a collider corner
            {
                return true; // There is a collision, we let obstacle avoidance take over movement
            }
        }

        lockMovementDirection = false;
        vectorListIndex = 0;
        return false;
    }

    private void GetColliderCorners()
    {
        float top = characterCollider.offset.y + (characterCollider.size.y / 2f);
        float btm = characterCollider.offset.y - (characterCollider.size.y / 2f);
        float left = characterCollider.offset.x - (characterCollider.size.x / 2f);
        float right = characterCollider.offset.x + (characterCollider.size.x / 2f);

        colliderCorners[0] = transform.TransformPoint(new Vector2(left, top));
        colliderCorners[1] = transform.TransformPoint(new Vector2(right, top));
        colliderCorners[2] = transform.TransformPoint(new Vector2(left, btm));
        colliderCorners[3] = transform.TransformPoint(new Vector2(right, btm));
    }

    // Logic for obstacle avoidance
    private void AvoidObstacle()
    {
        // If movement direction is not locked, we recaculate best path
        if (!lockMovementDirection)
        {
            lockMovementDirection = true;

            GetVectorListIndex();
        }

        // Movement direction is locked, we start raycasting to find best path

        bool hitSomething = false;

        for (int i = 0; i < colliderCorners.Length; i++)
        {
            var hitCheck = Physics2D.Raycast(colliderCorners[i], vectorList[vectorListIndex], rayLength, obstacleLayer);
            if (hitCheck)
            {
                hitSomething = true;
                break;
            }
        }

        if (hitSomething) // If we hit an obstacle, we check perpendicular directions
        {
            // If diagonal
            if (vectorListIndex == 1 || vectorListIndex == 3 || vectorListIndex == 5 || vectorListIndex == 7)
            {
                direction = BalanceDirection(vectorList[vectorListIndex]);
                isMoving = true;
                moveSpeed = runSpeed;
                transform.Translate(direction * moveSpeed * Time.deltaTime); // Move character         

                // Call movement animations
                AnimateMovement();

                // Reset movement variables
                isMoving = false;
                direction = new Vector2(0, 0);
                moveSpeed = oldMoveSpeed;

                return;
            }

            // If not diagonal we ensure diagonal movement
            int hitRightIndex = vectorListIndex + 1;

            if (hitRightIndex > 7)
            {
                hitRightIndex -= 8;
            }

            bool hitRight = false;

            for (int i = 0; i < colliderCorners.Length; i++)
            {
                var hitCheck = Physics2D.Raycast(colliderCorners[i], vectorList[hitRightIndex], rayLength, obstacleLayer);
                if (hitCheck)
                {
                    hitRight = true;
                    break;
                }
            }

            if (hitRight) // Something's on the right, we check left side
            {
                int hitLeftIndex = vectorListIndex - 1;
                if (hitLeftIndex < 0)
                {
                    hitLeftIndex += 8;
                }

                bool hitLeft = false;

                for (int i = 0; i < colliderCorners.Length; i++)
                {
                    var hitCheck = Physics2D.Raycast(colliderCorners[i], vectorList[hitLeftIndex], rayLength, obstacleLayer);
                    if (hitCheck)
                    {
                        hitLeft = true;
                        break;
                    }
                }

                if (hitLeft) // We just move in original vector direction no choice
                {
                    direction = BalanceDirection(vectorList[hitRightIndex]);
                    isMoving = true;
                    moveSpeed = runSpeed;
                    transform.Translate(direction * moveSpeed * Time.deltaTime); // Move character         

                    // Call movement animations
                    AnimateMovement();

                    // Reset movement variables
                    isMoving = false;
                    direction = new Vector2(0, 0);
                    moveSpeed = oldMoveSpeed;

                    return;
                }
                else // Move left
                {
                    direction = BalanceDirection(vectorList[hitLeftIndex]);
                    isMoving = true;
                    moveSpeed = runSpeed;
                    transform.Translate(direction * moveSpeed * Time.deltaTime); // Move character         

                    // Call movement animations
                    AnimateMovement();

                    // Reset movement variables
                    isMoving = false;
                    direction = new Vector2(0, 0);
                    moveSpeed = oldMoveSpeed;

                    return;
                }
            }
            else // Move right
            {
                direction = BalanceDirection(vectorList[hitRightIndex]);
                isMoving = true;
                moveSpeed = runSpeed;
                transform.Translate(direction * moveSpeed * Time.deltaTime); // Move character         

                // Call movement animations
                AnimateMovement();

                // Reset movement variables
                isMoving = false;
                direction = new Vector2(0, 0);
                moveSpeed = oldMoveSpeed;

                return;
            }
        }

        // Didn't hit anything, we move once more than calculate again
        direction = BalanceDirection(vectorList[vectorListIndex]);
        isMoving = true;
        moveSpeed = runSpeed;
        transform.Translate(direction * moveSpeed * Time.deltaTime); // Move character         

        // Call movement animations
        AnimateMovement();

        // Reset movement variables
        isMoving = false;
        direction = new Vector2(0, 0);
        moveSpeed = oldMoveSpeed;

        // Reset variables
        lockMovementDirection = false;
    }

    private void GetVectorListIndex()
    {
        // Get direction, we always aim to move towards here
        direction = BalanceDirection(-(transform.position - moveToLocation));

        // Target is diagonal
        if (direction.x != 0 && direction.y != 0)
        {
            if (direction.x > 0 && direction.y > 0) // Top right
            {
                vectorListIndex = 1;
            }
            else if (direction.x > 0 && direction.y < 0) // Bottom right
            {
                vectorListIndex = 3;
            }
            else if (direction.x < 0 && direction.y < 0) // Bottom left
            {
                vectorListIndex = 5;
            }
            else if (direction.x < 0 && direction.y > 0) // Top left
            {
                vectorListIndex = 7;
            }
            else
            {
                Debug.Log("Error avoiding obstacles.");
                return;
            }
        }
        else // Target is not diagonal
        {
            if (direction.x > 0 && direction.y == 0) // Right
            {
                vectorListIndex = 2;
            }
            else if (direction.x < 0 && direction.y == 0) // Left
            {
                vectorListIndex = 6;
            }
            else if (direction.y > 0 && direction.x == 0) // Up
            {
                vectorListIndex = 0;
            }
            else if (direction.y < 0 && direction.x == 0) // Down
            {
                vectorListIndex = 4;
            }
            else
            {
                Debug.Log("Error avoiding obstacles.");
                return;
            }
        }
    }

    void AnimateMovement() // All movement animations for characters go here
    {
        // We check for diagonal movement to update animation
        if (isMoving)
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
        isPaused = true;
        MovementStop();
    }

    private void Resume()
    {
        isPaused = false;
        MovementStart();
    }

    private void DialogStart()
    {
        inDialog = true;
        MovementStop();
    }

    private void DialogStop()
    {
        inDialog = false;
        MovementStart();
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
