using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShaker : MonoBehaviour {

    public bool cameraShakeCheck = true;
    private IEnumerator cameraShake;

    public void ShakeCamera(float magnitude, float duration)
    {
        if (cameraShake != null)
        {
            StopCoroutine(cameraShake);
            cameraShake = null;
        }

        cameraShake = CameraShake(magnitude, duration);
        StartCoroutine(cameraShake);
    }

    private IEnumerator CameraShake(float magnitude, float duration)
    {
        if (cameraShakeCheck) // Check in playerprefs
        {
            float elapsedTime = 0.0f;

            while (elapsedTime < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;

                transform.localPosition = new Vector2(x, y);
                elapsedTime += Time.deltaTime;

                yield return null;
            }
        }

        cameraShake = null;
    }
}
