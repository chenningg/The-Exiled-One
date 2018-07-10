using UnityEngine;
using UnityEngine.UI;

public class LightningController : MonoBehaviour {

    #region Instance
    private static LightningController lightningControllerInstance;

    public static LightningController Instance { get { return lightningControllerInstance; } }

    private void Awake()
    {
        if (lightningControllerInstance != null && lightningControllerInstance != this)
        {
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
            return;
        }
        lightningControllerInstance = this;
    }
    #endregion

    public Image lightningImage;

    public void EnableLightning()
    {
        lightningImage.enabled = true;
    }

    public void DisableLightning()
    {
        lightningImage.enabled = false;
    }
}
