using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Adds flickering to a light
public class LightFlicker : MonoBehaviour {

    // Define flicker variables (flicker interval in seconds), flickerSpeed is how fast the light transitions to another intensity
    [SerializeField]
    private float flickerIntensityMin = 0.5f, flickerIntensityMax = 0.5f, flickerInterval = 0.15f, flickerSpeed = 60f;

    // Allow or disallow flickering in player preferences
    private bool flickerCheck = true;

    // Access the light
    private Light lightSource;

    // Store original intensity of light
    private float baseLightIntensity;

    void Start()
    {
        lightSource = GetComponent<Light>();
        if (lightSource == null) // Check if this script exists on a light GameObject
        {
            Debug.LogError("lightFlicker script must have a Light Component on the same GameObject.");
            return;
        }

        // Check playerprefs if flickering is turned on here! If it isn't change flickerCheck to false and return!
        baseLightIntensity = lightSource.intensity;

        StartCoroutine("Flicker");
    }

    private IEnumerator Flicker()
    {
        while (flickerCheck)
        {
            lightSource.intensity = Mathf.Lerp(lightSource.intensity, Random.Range(baseLightIntensity - flickerIntensityMin, baseLightIntensity + flickerIntensityMax), flickerSpeed * Time.deltaTime);
            yield return new WaitForSeconds(flickerInterval);
        }
    }

    private void OnDestroy()
    {
        flickerCheck = false;
        // Reset light's original intensity
        lightSource.intensity = baseLightIntensity;
    }
}
