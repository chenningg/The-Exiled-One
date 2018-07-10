using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtboxController : MonoBehaviour {

    // References
    public EnemyAttack attackScript;

    // Variables
    private int damageModifier = 0;

    private int damageAmount = 0;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player Hitbox"))
        {
            damageModifier = Random.Range(-attackScript.damageVariation, attackScript.damageVariation + 1);

            if (attackScript.canMiss)
            {
                float missCheck = Random.Range(0f, 1f);

                if (missCheck <= attackScript.missChance)
                {
                    damageModifier = -attackScript.attackDamage;
                }
            }

            damageAmount = attackScript.attackDamage + damageModifier;

            PlayerController.Instance.takeDamageScript.Damage(damageAmount);
        }
    }
}
