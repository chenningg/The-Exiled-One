using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour {

    // References
    public Animator anim;
    public Movement moveScript;
    private IEnumerator attackHandler;

    // Attack variables
    public int attackDamage; // Damage each attack does
    public int damageVariation; // Fluctuation of damage
    public bool canMiss; // Can attack have a chance to miss?
    [Range(0, 1)]
    public float missChance; // What is the chance of missing

    public float attackDistance; // Distance to attack from
    public bool isRanged; // Is this a ranged unit?
    public bool kites; // Does this unit attempt to kite the player?
    public float attackDelay; // Time between attacks
    public float attackTime; // Time it takes for one attack and before unit can move again
    public bool isAttacking; // Is this unit attacking
    public bool canAttack; // Can this unit attack again?
    private bool allowAttack = true; // Allow this unit to attack?

    // Pause events
    private bool inDialog = false;
    private bool isPaused = false;

    // Hurtboxes (meelee)
    public GameObject hurtboxLeft;
    public GameObject hurtboxRight;
    public GameObject hurtboxUp;
    public GameObject hurtboxDown;

    // Where to spawn projectiles
    public GameObject projectileSpawnerLeft;
    public GameObject projectileSpawnerRight;
    public GameObject projectileSpawnerUp;
    public GameObject projectileSpawnerDown;

    private void Start()
    {
        canAttack = true;
        EventManager.Instance.e_pauseGame.AddListener(Pause);       
        EventManager.Instance.e_resumeGame.AddListener(Resume);
        EventManager.Instance.e_startDialog.AddListener(DialogStart);
        EventManager.Instance.e_endDialog.AddListener(DialogStop);

        if (DialogManager.Instance.inDialog)
        {
            DisallowAttack();
        }
    }

    private void OnDisable()
    {
        EventManager.Instance.e_pauseGame.RemoveListener(Pause);
        EventManager.Instance.e_resumeGame.RemoveListener(Resume);
        EventManager.Instance.e_startDialog.RemoveListener(DialogStart);
        EventManager.Instance.e_endDialog.RemoveListener(DialogStop);
    }

    public void Attack()
    {
        if (allowAttack && canAttack && attackHandler == null)
        {
            isAttacking = true;
            attackHandler = AttackHandler();
            StartCoroutine(attackHandler);
        }
    }

    private IEnumerator AttackHandler()
    {
        // Allow attack again after delay
        Invoke("ResetAttack", attackDelay);
        canAttack = false;

        // Stop movement and call animation
        var moveDirection = BalanceDirection(-(transform.position - PlayerController.Instance.transform.position));

        moveScript.moveSpeed = moveScript.runSpeed;
        moveScript.isMoving = true;
        moveScript.Move(new Vector2(moveDirection.x, moveDirection.y));

        moveScript.MovementStop();
        anim.SetBool("isAttacking", true);

        // Pause for attackTime seconds (time it takes to attack)
        yield return new WaitForSecondsRealtime(attackTime);

        // Reset variables
        anim.SetBool("isAttacking", false);
        isAttacking = false;
        attackHandler = null;
        moveScript.MovementStart(); // Resume movement       
    }

    private void ResetAttack()
    {
        canAttack = true;
    }

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

    // Animation events

    // For meelee attacks
    public void SpawnHurtbox(string direction)
    {
        switch (direction)
        {
            case ("left"):
                hurtboxLeft.SetActive(true);
                return;
            case ("right"):
                hurtboxRight.SetActive(true);
                return;
            case ("up"):
                hurtboxUp.SetActive(true);
                return;
            case ("down"):
                hurtboxDown.SetActive(true);
                return;
        }
    }

    public void DisableHurtbox()
    {
        hurtboxLeft.SetActive(false);
        hurtboxRight.SetActive(false);
        hurtboxUp.SetActive(false);
        hurtboxDown.SetActive(false);
    }

    // For ranged attacks
    public void SpawnProjectile(string direction)
    {
        switch (direction)
        {
            case ("left"):
                return;
            case ("right"):
                return;
            case ("up"):
                return;
            case ("down"):
                return;
        }
    }

    // PAUSE EVENTS

    private void Pause()
    {
        isPaused = true;
        DisallowAttack();
    }

    private void Resume()
    {
        isPaused = false;
        AllowAttack();
    }

    private void DialogStart()
    {
        inDialog = true;
        DisallowAttack();
    }

    private void DialogStop()
    {
        inDialog = false;
        AllowAttack();
    }

    public void DisallowAttack()
    {
        allowAttack = false;
    }

    public void AllowAttack()
    {
        if (!inDialog && !isPaused)
        {
            allowAttack = true;
        }
    }
}
