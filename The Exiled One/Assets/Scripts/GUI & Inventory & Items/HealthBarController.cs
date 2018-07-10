using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour {

    #region Instance
    private static HealthBarController healthBarControllerInstance;

    public static HealthBarController Instance { get { return healthBarControllerInstance; } }

    private void Awake()
    {
        if (healthBarControllerInstance == null)
        {
            healthBarControllerInstance = this;
        }
    }
    #endregion

    // References

    public Image image;
    private Material spriteFlashWhiteMat;

    private void Start()
    {
        spriteFlashWhiteMat = new Material(Shader.Find("Sprites/SpriteFlashWhite"));
    }

    public void FlashWhite()
    {
        image.material = spriteFlashWhiteMat;
        Invoke("RevertMaterial", 0.1f);
    }

    public void RevertMaterial()
    {
        image.material = null;
    }
}
