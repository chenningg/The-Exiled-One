using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerProfileImageController : MonoBehaviour {

    // References
    private EventManager eventManager;
    public Image playerProfileImage;
    public Sprite playerProfileImageDead;

	// Use this for initialization
	void Start () {
        EventManager.Instance.e_playerDeath.AddListener(DeadPlayerProfile);
	}

    private void OnDisable()
    {
        EventManager.Instance.e_playerDeath.RemoveListener(DeadPlayerProfile);
    }

    void DeadPlayerProfile()
    {
        playerProfileImage.sprite = playerProfileImageDead;
    }
}
