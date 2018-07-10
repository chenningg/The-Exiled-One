using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitboxController : MonoBehaviour {

    #region Instance
    private static PlayerHitboxController playerHitBoxControllerInstance;

    public static PlayerHitboxController Instance { get { return playerHitBoxControllerInstance; } }

    private void Awake()
    {
        if (playerHitBoxControllerInstance == null)
        {
            playerHitBoxControllerInstance = this;
        }
    }
    #endregion

}
