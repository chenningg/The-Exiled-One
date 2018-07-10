using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeObject : MonoBehaviour
{
    public float fadeAmount = 0.6f; // Transparency out of 1;
    private float fadeSpeed = 0.1f; // Each fade increment
    private float fadeDelay = 0.05f; // Each delay between fade increments

    public int ownSortOrder;
    public SpriteRenderer colorRenderer;

    private IEnumerator fadeOut;
    private IEnumerator fadeIn;

    private void Start()
    {
        colorRenderer = transform.parent.GetComponent<SpriteRenderer>();
        ownSortOrder = transform.parent.gameObject.GetComponent<SpriteRenderer>().sortingOrder;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player Hitbox"))
        {
            if (ownSortOrder > PlayerController.Instance.GetComponent<SpriteRenderer>().sortingOrder)
            {
                if (fadeIn != null)
                {
                    StopCoroutine(fadeIn);
                }

                fadeOut = FadeOut();
                StartCoroutine(fadeOut);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player Hitbox"))
        {
            if (fadeOut != null)
            {
                StopCoroutine(fadeOut);
            }

            fadeIn = FadeIn();
            StartCoroutine(fadeIn);
        }
    }

    private IEnumerator FadeOut()
    {
        var currentFadeValue = 1f;
        while (currentFadeValue >= fadeAmount)
        {
            currentFadeValue -= fadeSpeed;
            colorRenderer.color = new Color(1, 1, 1, currentFadeValue);
            yield return new WaitForSeconds(fadeDelay);
        }
        yield return null;
    }

    private IEnumerator FadeIn()
    {
        var currentFadeValue = fadeAmount;
        while (currentFadeValue < 1f)
        {
            currentFadeValue += fadeSpeed;
            colorRenderer.color = new Color(1, 1, 1, currentFadeValue);
            yield return new WaitForSeconds(fadeDelay);
        }
        yield return null;
    }
}
