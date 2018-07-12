using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour {

    // Stats
    public string enemyName;

    // References
    public Movement moveScript;
    public PlayerDetectionTrigger playerDetectTriggerScript;
    public TakeDamage takeDamageScript;
    public EnemyAttack attackScript;
    public BoxCollider2D characterCollider;
    private LayerMask obstacleLayer = (1 << 8);

    // Movement variables
    [Range(0, 1)]
    public float moveChance; // Chance for animal to move
    private IEnumerator moveEnemyHandler;
    private Vector2 moveDirection;

    // Action variables
    public float actionDelay; // Delay between the action enemy takes in seconds   
    public bool isAggressive; // Is this enemy aggressive (will attack on sight)
    public bool retaliates; // Will this enemy retaliate? (Won't attack unless attacked)

    private float lockActionTimer = 0; // If enemy is idling or moving about, we lock its movement for a while unless player appears
    private bool lockAction = false; // Locks action of enemy
    private ActionState actionState;
    private float randomActionNumber;

    // Obstacle avoidance variables
    private float rayLength = 2f; // What is the ray length to check for obstacles?
    private bool runToPlayer = false; // Is this character running to or away from the player?
    private bool lockMovementDirection = false; // Lock this character movement direction?
    private Vector2[] vectorList;
    private Vector2[] colliderCorners = new Vector2[4];
    private int vectorListIndex = 0; // Points to direction of raycast check

    // Pause variables
    private bool isPaused; // Is the game paused?
    private bool inDialog;

    private IEnumerator actionHandler;

    private void Start()
    {
        // Subscribe to events
        EventManager.Instance.e_pauseGame.AddListener(Pause);
        EventManager.Instance.e_resumeGame.AddListener(Resume);
        EventManager.Instance.e_startDialog.AddListener(DialogStart);
        EventManager.Instance.e_endDialog.AddListener(DialogStop);

        actionState = ActionState.idle;

        // Set variables
        vectorList = new[] { Vector2.up, new Vector2(1, 1),
        Vector2.right, new Vector2(1, -1), Vector2.down,
        new Vector2(-1, -1), Vector2.left, new Vector2(-1, 1)};
    }

    private void OnDisable()
    {
        EventManager.Instance.e_pauseGame.RemoveListener(Pause);
        EventManager.Instance.e_resumeGame.RemoveListener(Resume);
        EventManager.Instance.e_startDialog.RemoveListener(DialogStart);
        EventManager.Instance.e_endDialog.RemoveListener(DialogStop);
    }

    private void Update()
    {
        if (takeDamageScript.isDead)
        {
            // Do nothing until game object is destroyed
        }
        else
        {
            DetermineAction();
        }
    }

    void DetermineAction()
    {
        // Check if ready for new action
        if (lockActionTimer >= actionDelay)
        {
            lockAction = false;
            lockActionTimer = 0; // Reset
        }
        else
        {
            if (!isPaused && !inDialog)
            {
                lockAction = true;
                lockActionTimer += Time.deltaTime;
            }
        }

        // Determine action
        if (isAggressive && playerDetectTriggerScript.playerDetected && attackScript)
        {
            // Attack player here
            takeDamageScript.inCombat = true;
            actionState = ActionState.attack;
            lockAction = true;
        }

        else if (isAggressive && takeDamageScript.inCombat && attackScript)
        {
            // Attack player here
            actionState = ActionState.attack;
            lockAction = true;
        }

        else if (retaliates && takeDamageScript.inCombat && attackScript)
        {
            // Attack back
            actionState = ActionState.attack;
            lockAction = true;
        }

        else if (!isAggressive && !retaliates && takeDamageScript.inCombat)
        {
            actionState = ActionState.runAway;
            lockAction = true;
        }

        // Run away if not aggressive
        else if (playerDetectTriggerScript.playerDetected && !retaliates)
        {
            actionState = ActionState.runAway;
            lockAction = true;
        }   

        // Ready for new action
        if (lockAction == false)
        {
            lockAction = true;

            // Determine if animal moves (and its random direction) or idles
            randomActionNumber = Random.Range(0.0f, 1.0f);
            var randomX = Random.Range(0, 2) == 0 ? -1 : 1; // Choose x direction between 1 and -1
            var randomY = Random.Range(0, 2) == 0 ? -1 : 1; // Choose y direction
            moveDirection = new Vector2(randomX, randomY);

            if (randomActionNumber <= moveChance)
            {
                actionState = ActionState.runRandomly;
            }
            else
            {
                actionState = ActionState.idle;
            }
        }

        switch (actionState)
        {
            case (ActionState.attack):
                Attack();
                return;
            case (ActionState.runAway):
                RunAway();
                return;
            case (ActionState.runRandomly):
                RunRandomly();
                return;
            case (ActionState.idle):
                moveScript.Move(new Vector2(0, 0));
                return;
        }
    }

    private void Attack()
    {
        // Cant attack yet, we kite
        if (!attackScript.canAttack)
        {
            // If doesn't kite we continue running towards player
            if (attackScript.kites)
            {
                RunAway();
                return;
            }
        }

        // If distance from this to player is more than attack distance we run closer until we can attack
        if ((PlayerController.Instance.transform.position - transform.position).sqrMagnitude > attackScript.attackDistance * attackScript.attackDistance)
        {
            RunToPlayer();
        }
        else if (attackScript.isRanged && CollisionCheck(attackScript.attackDistance))
        {
            AvoidObstacleRanged();
        }
        else
        {
            attackScript.Attack();
        }
    }

    private void RunToPlayer()
    {
        runToPlayer = true;
        if (!CollisionCheck(rayLength))
        {
            runToPlayer = true;
            moveDirection = BalanceDirection(GetRunDirection(transform.position));

            moveScript.moveSpeed = moveScript.runSpeed;
            moveScript.isMoving = true;
            moveScript.Move(new Vector2(moveDirection.x, moveDirection.y));
        }
        else
        {
            AvoidObstacle();
        }
    }

    private void RunAway()
    {
        runToPlayer = false;
        if (!CollisionCheck(rayLength))
        {
            runToPlayer = false;
            moveDirection = BalanceDirection(GetRunDirection(transform.position));

            moveScript.moveSpeed = moveScript.runSpeed;
            moveScript.isMoving = true;
            moveScript.Move(new Vector2(moveDirection.x, moveDirection.y));
        }
        else
        {
            AvoidObstacle();
        }
    }

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

    private bool CollisionCheck(float lengthOfRay)
    {
        // Raycast from corners of box collider to player to check for collision
        GetColliderCorners();

        for (int i = 0; i < colliderCorners.Length; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(colliderCorners[i], GetRunDirection(colliderCorners[i]), lengthOfRay, obstacleLayer);

            if (hit) // Obstacle hit by a collider corner
            {
                return true; // There is a collision, we let obstacle avoidance take over movement
            }
        }

        lockMovementDirection = false;
        vectorListIndex = 0;
        return false;
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
                Debug.DrawLine(transform.position, hitCheck.point, Color.red);
                hitSomething = true;
                break;
            }

            Debug.DrawLine(transform.position, vectorList[vectorListIndex] * rayLength, Color.green);
        }

        if (hitSomething) // If we hit an obstacle, we check perpendicular directions
        {
            // If diagonal
            if (vectorListIndex == 1 || vectorListIndex == 3 || vectorListIndex == 5 || vectorListIndex == 7)
            {
                moveDirection = BalanceDirection(vectorList[vectorListIndex]);

                moveScript.moveSpeed = moveScript.runSpeed;
                moveScript.isMoving = true;
                moveScript.Move(new Vector2(moveDirection.x, moveDirection.y));

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
                    Debug.DrawLine(transform.position, hitCheck.point, Color.red);
                    hitRight = true;
                    break;
                }

                Debug.DrawLine(transform.position, vectorList[vectorListIndex] * rayLength, Color.green);
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
                        Debug.DrawLine(transform.position, hitCheck.point, Color.red);
                        hitLeft = true;
                        break;
                    }

                    Debug.DrawLine(transform.position, vectorList[vectorListIndex] * rayLength, Color.green);
                }

                if (hitLeft) // We just move in original vector direction no choice
                {
                    moveDirection = BalanceDirection(vectorList[hitRightIndex]);

                    moveScript.moveSpeed = moveScript.runSpeed;
                    moveScript.isMoving = true;
                    moveScript.Move(new Vector2(moveDirection.x, moveDirection.y));

                    return;
                }
                else // Move left
                {
                    moveDirection = BalanceDirection(vectorList[hitLeftIndex]);

                    moveScript.moveSpeed = moveScript.runSpeed;
                    moveScript.isMoving = true;
                    moveScript.Move(new Vector2(moveDirection.x, moveDirection.y));

                    return;
                }
            }
            else // Move right
            {
                moveDirection = BalanceDirection(vectorList[hitRightIndex]);

                moveScript.moveSpeed = moveScript.runSpeed;
                moveScript.isMoving = true;
                moveScript.Move(new Vector2(moveDirection.x, moveDirection.y));

                return;
            }
        }

        // Didn't hit anything, we move once more than calculate again
        moveDirection = BalanceDirection(vectorList[vectorListIndex]);

        moveScript.moveSpeed = moveScript.runSpeed;
        moveScript.isMoving = true;
        moveScript.Move(new Vector2(moveDirection.x, moveDirection.y));

        // Reset variables
        lockMovementDirection = false;
    }

    private void AvoidObstacleRanged()
    {
        // If movement direction is not locked, we recaculate best path
        if (!lockMovementDirection)
        {
            lockMovementDirection = true;

            GetVectorListIndex();
        }

        // If diagonal
        if (vectorListIndex == 1 || vectorListIndex == 3 || vectorListIndex == 5 || vectorListIndex == 7)
        {
            moveDirection = BalanceDirection(vectorList[vectorListIndex]);

            moveScript.moveSpeed = moveScript.runSpeed;
            moveScript.isMoving = true;
            moveScript.Move(new Vector2(moveDirection.x, moveDirection.y));

            return;
        }

        // If not diagonal we ensure diagonal movement
        int rightIndex = vectorListIndex + 1;

        if (rightIndex > 7)
        {
            rightIndex -= 8;
        }

        moveDirection = BalanceDirection(vectorList[rightIndex]);

        moveScript.moveSpeed = moveScript.runSpeed;
        moveScript.isMoving = true;
        moveScript.Move(new Vector2(moveDirection.x, moveDirection.y));

        return;
    }

    private Vector2 GetRunDirection(Vector3 startingPos)
    {
        Vector2 directionToPlayer;

        if (runToPlayer) // Get direction to run to player
        {
            directionToPlayer = -(startingPos - PlayerController.Instance.transform.position);
        }
        else // Run away from player
        {
            directionToPlayer = -(PlayerController.Instance.transform.position - startingPos);
        }

        return (directionToPlayer);
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

    private void GetVectorListIndex()
    {
        // Get player direction, we always aim to move towards here
        moveDirection = BalanceDirection(GetRunDirection(transform.position));

        // Target is diagonal
        if (moveDirection.x != 0 && moveDirection.y != 0)
        {
            if (moveDirection.x > 0 && moveDirection.y > 0) // Top right
            {
                vectorListIndex = 1;
            }
            else if (moveDirection.x > 0 && moveDirection.y < 0) // Bottom right
            {
                vectorListIndex = 3;
            }
            else if (moveDirection.x < 0 && moveDirection.y < 0) // Bottom left
            {
                vectorListIndex = 5;
            }
            else if (moveDirection.x < 0 && moveDirection.y > 0) // Top left
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
            if (moveDirection.x > 0 && moveDirection.y == 0) // Right
            {
                vectorListIndex = 2;
            }
            else if (moveDirection.x < 0 && moveDirection.y == 0) // Left
            {
                vectorListIndex = 6;
            }
            else if (moveDirection.y > 0 && moveDirection.x == 0) // Up
            {
                vectorListIndex = 0;
            }
            else if (moveDirection.y < 0 && moveDirection.x == 0) // Down
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

    private void RunRandomly()
    {
        moveScript.isMoving = true;
        moveScript.Move(moveDirection);
    }

    public enum ActionState
    {
        runAway,
        attack,
        runRandomly,
        idle
    }

    // PAUSE EVENTS

    private void Pause()
    {
        isPaused = true;
    }

    private void Resume()
    {
        isPaused = false;
    }

    private void DialogStart()
    {
        inDialog = true;
    }

    private void DialogStop()
    {
        inDialog = false;
    }
}
