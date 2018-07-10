using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Placed on things that take damage
public class TakeDamage : MonoBehaviour {

    // Variables
    public bool inCombat; // Check if entity is engaging in combat
    public bool regensHealth; // If true, entity regenerates health after leaving combat
    public bool isDead; // Returns true if entity is dead
    public bool recentlyAttacked; // Returns true if entity has been attacked recently
    public bool canBeKnockedback; // Will this unit be knockedback on damage?
    [Range(0, 1)]
    public float knockbackChance; // Chance of knockback on taking damage
    public float knockbackDelay; // Time of knockback
    private bool invulnerable = false; // Invulnerable state

    // Editor variables
    public float deathDelay; // Time after which object disappears on death
    public float regenInterval; // Time between each regen tick
    public float regenPercent; // Percentage that entity regens health per tick
    public float outOfCombatDelay; // Time after which inCombat state is cancelled
    public List<Item> takeDamageFromList = new List<Item>(); // List of items which this thing can take damage from

    // References
    public Stat health;
    public Animator anim;
    public Movement moveScript;
    public EnemyAttack attackScript;
    public GameObject healthBarContainer;
    public GameObject popupTextContainer;
    public SpriteRenderer spriteRenderer;
    public PlayerDetectionTrigger playerDetectTriggerScript;
    public DropLoot dropLootScript;
    public ObjectSaveLoad saveLoadScript;
    public ParticleSystem particles;
    private ParticleSystemRenderer particleRenderer;
    private Material spriteFlashWhiteMat;
    private Material oldMat;

    private IEnumerator regenHealth = null;

    private void Start()
    {
        inCombat = false;
        isDead = false;
        recentlyAttacked = false;
        spriteFlashWhiteMat = new Material(Shader.Find("Sprites/SpriteFlashWhite"));
        particleRenderer = GetComponent<ParticleSystemRenderer>();
        oldMat = spriteRenderer.material;

        if (regensHealth)
        {
            StartCoroutine(CombatCheck());
        }
    }

    // Apply or heal damage
    public void Damage(int amount)
    {
        if (!invulnerable)
        {
            // Check if any specific items this thing can only take damage from
            if (takeDamageFromList.Count > 0)
            {
                bool canDamageThis = false;

                for (int i = 0; i < takeDamageFromList.Count; i++)
                {
                    if (takeDamageFromList[i].itemName == Inventory.Instance.currentlySelectedItem.itemName)
                    {
                        canDamageThis = true;
                        break;
                    }
                }

                if (!canDamageThis)
                {
                    return;
                }
            }

            // Show hp bar if any
            if (healthBarContainer != null)
            {
                healthBarContainer.SetActive(true);
            }

            // Flash sprite white
            if (!(health.currentValue <= 0) && spriteRenderer)
            {
                SpriteFlashWhite();
            }

            // Particle effects
            if (particles)
            {
                if (particleRenderer && spriteRenderer)
                {
                    particleRenderer.sortingOrder = spriteRenderer.sortingOrder;
                }
                particles.Play();
            }

            // If player, camera shake
            if (gameObject.CompareTag("Player"))
            {
                CameraController.Instance.cameraShakeScript.ShakeCamera(0.1f, 0.15f); 
                HealthBarController.Instance.FlashWhite();
            }

            // Minus health
            health.currentValue -= amount;
            inCombat = true;
            recentlyAttacked = true;

            // Calculate knockback
            if (canBeKnockedback)
            {
                if (Random.Range(0f, 1f) <= knockbackChance)
                {
                    Knockback();
                }
            }

            if (popupTextContainer)
            {
                // Floating damage text
                StartCoroutine(PopupText(amount, "damage"));
            }

            // Entity is dead
            if (health.currentValue <= 0)
            {
                if (!isDead)
                {
                    Die();
                }
            }
        } 
    }

    public void Heal(int amount)
    {
        if (healthBarContainer != null)
        {
            healthBarContainer.SetActive(true);
        }

        health.currentValue += amount;

        if (popupTextContainer)
        {
            // Floating damage text
            StartCoroutine(PopupText(amount, "heal"));
        }
    }

    // Knockback
    private void Knockback()
    {
        Invoke("ResetKnockback", knockbackDelay);

        moveScript.MovementStop();
        anim.SetBool("isKnockedback", true);
    }

    private void ResetKnockback()
    {
        anim.SetBool("isKnockedback", false);
        moveScript.MovementStart();
    }

    // Death
    public void Die()
    {
        if (healthBarContainer != null)
        {
            if (healthBarContainer.activeSelf)
            {
                healthBarContainer.SetActive(false);
            }
        }

        // If player is dead, call the death event
        if (gameObject.CompareTag("Player"))
        {
            EventManager.Instance.e_playerDeath.Invoke();
            moveScript.MovementStop();
            anim.SetBool("isDead", true); 
            invulnerable = false;
            return;
        }

        // Prevent further damage
        invulnerable = true;
        isDead = true;

        if (moveScript)
        {
            moveScript.MovementStop();
        }

        if (anim)
        {
            anim.SetBool("isDead", isDead);
        }

        // Drop items on death
        if (dropLootScript && dropLootScript.enabled)
        {
            dropLootScript.DropItems();
        }

        // Save this object's destroyed status
        if (saveLoadScript)
        {
            saveLoadScript.isDestroyed = true;
            saveLoadScript.Save();
        }

        Invoke("DestroyGameObject", deathDelay);
    }

    private void DestroyGameObject()
    {
        Destroy(gameObject);
    }

    // Check for combat repeatedly

    private IEnumerator CombatCheck()
    {
        float outOfCombatTimer = 0f;
        bool repeat = true;

        while (repeat)
        {
            if (recentlyAttacked)
            {
                outOfCombatTimer = 0f;
            }

            if (playerDetectTriggerScript != null)
            {
                if (playerDetectTriggerScript.playerDetected)
                {
                    outOfCombatTimer = 0f;
                }
            }           

            outOfCombatTimer += 0.5f;
            recentlyAttacked = false;

            if (outOfCombatTimer >= outOfCombatDelay)
            {
                inCombat = false;
                outOfCombatTimer = 0f;
                RegenHealth();
            }

            yield return new WaitForSecondsRealtime(0.5f);
        }
    }


    // Regen hp if not in combat
    public void RegenHealth()
    {
        if (regenHealth != null)
        {
            StopCoroutine(regenHealth);
            regenHealth = null;
        }

        if (regensHealth && health.currentValue < health.maxValue)
        {
            regenHealth = RegenHealthHandler();
            StartCoroutine(regenHealth);
        }
        
        // Full health, don't regen
        else
        {
            if (healthBarContainer != null)
            {
                healthBarContainer.SetActive(false);
            }
        }
    }

    private IEnumerator RegenHealthHandler()
    {
        while (health.currentValue < health.maxValue)
        {
            Heal(Mathf.RoundToInt((health.maxValue/100) * regenPercent));
            yield return new WaitForSecondsRealtime(regenInterval);
        }

        if (healthBarContainer != null)
        {
            healthBarContainer.SetActive(false);
        }

        regenHealth = null;
    }

    // Visual stuff
    public void SpriteFlashWhite()
    {
        spriteRenderer.material = spriteFlashWhiteMat;
        Invoke("RevertSpriteMaterial", 0.1f);
    }

    public void RevertSpriteMaterial()
    {
        spriteRenderer.material = oldMat;
    }

    private IEnumerator PopupText(int popupAmount, string popupType)
    {
        var popupText = Instantiate(PrefabManager.Instance.prefabDatabase["Damage Popup Text"], popupTextContainer.transform);
        popupText.localPosition = new Vector2(Random.Range(-5, 6), Random.Range(-2, 3));

        var displayText = popupText.GetComponent<Text>();

        displayText.text = popupAmount.ToString();

        // Change colour based on popupText type

        switch (popupType)
        {
            case ("damage"):
                displayText.color = new Color32(255, 23, 23, 255);
                break;
            case ("heal"):
                displayText.color = new Color32(70, 244, 27, 255);
                break;
            case ("poison"):
                displayText.color = new Color32(169, 27, 184, 255);
                break;
            default:
                displayText.color = new Color32(255, 23, 23, 255);
                break;
        }

        // Change size (and text) based on amount
        if (popupAmount <= 0)
        {
            displayText.text = "Miss!";
            displayText.color = Color.white;
        }
        else if (popupAmount >= 10)
        {
            displayText.fontSize += 2;
        }
        else if (popupAmount >= 20)
        {
            displayText.fontSize += 4;
        }
        else if (popupAmount >= 40)
        {
            displayText.fontSize += 6;
        }

        // Timer for text disappearance
        float timer = 0;

        while (timer < 0.6f)
        {
            timer += Time.deltaTime;

            popupText.localPosition = new Vector2(popupText.localPosition.x, popupText.localPosition.y + 0.05f);

            yield return null;
        }

        Destroy(popupText.gameObject);
    } 
}
