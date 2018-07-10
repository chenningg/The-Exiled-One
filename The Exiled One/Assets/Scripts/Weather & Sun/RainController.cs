using UnityEngine;

public class RainController : MonoBehaviour {

    #region Instance
    private static RainController rainControllerInstance;

    public static RainController Instance { get { return rainControllerInstance; } }

    private void Awake()
    {
        if (rainControllerInstance != null && rainControllerInstance != this)
        {
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
            return;
        }
        rainControllerInstance = this;
    }
    #endregion

    public ParticleSystem rain1;
    public ParticleSystem rain2;

    private void Start()
    {
        rain1.Stop(true);
        rain2.Stop(true);
    }

    public void EnableRain()
    {
        rain1.gameObject.SetActive(true);
        rain2.gameObject.SetActive(true);
        rain1.Play(true);
        rain1.Play(true);
    }

    public void DisableRain()
    {
       rain1.Stop(true);
       rain2.Stop(true);
    }

    public void DisableRainImmediately()
    {
        rain1.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        rain2.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public void PauseRain()
    {
        rain1.Pause(true);
        rain2.Pause(true);
    }

    public void ResumeRain()
    {
        rain1.Play(true);
        rain2.Play(true);
    }
}
