using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : MonoBehaviour {

    #region Instance
    private static EventManager eventManagerInstance;

    public static EventManager Instance { get { return eventManagerInstance; } }

    private void Awake()
    {
        if (eventManagerInstance != null && eventManagerInstance != this)
        {
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
            return;
        }
        if (eventManagerInstance == null)
        {
            eventManagerInstance = this;
        }
    }
    #endregion

    /* EVENTS */

    //======== Game save, load, localization events =========//

    public UnityEvent e_newGame = new UnityEvent(); // Call when new game started
    public UnityEvent e_saveGame = new UnityEvent(); // Call when saving a game
    public UnityEvent e_loadGame = new UnityEvent(); // Call when game has to be loaded
    public UnityEvent e_gameLoaded = new UnityEvent(); // Call when game has finished loading
    public UnityEvent e_languageChange = new UnityEvent(); // Call when language is changed
    public UnityEvent e_localize = new UnityEvent(); // Call when language needs to be loaded to UI stuff
    public UnityEvent e_getReferences = new UnityEvent(); // Call all singletons to get their references

    //======== Game pause, resume, cutscene events =========//

    public UnityEvent e_pauseGame = new UnityEvent(); // Call when game is paused (time still runs)
    public UnityEvent e_resumeGame = new UnityEvent(); // Call when game is resumed
    public UnityEvent e_startDialog = new UnityEvent(); // Call when dialog is playing
    public UnityEvent e_endDialog = new UnityEvent(); // Call when dialog finishes playing

    //======== Time of the day and weather events =========//
    public UnityEvent e_dawnTime = new UnityEvent();
    public UnityEvent e_dayTime = new UnityEvent(); // Call when it is day
    public UnityEvent e_duskTime = new UnityEvent(); // Call when it is evening
    public UnityEvent e_nightTime = new UnityEvent(); // Call when it is night

    public UnityEvent e_raining = new UnityEvent(); // Call when it is raining
    public UnityEvent e_rainStop = new UnityEvent(); // Call when rain stops

    public UnityEvent e_hourPass = new UnityEvent(); // Called every hour

    //======== Inventory and item events =========//

    public UnityEvent e_updateInventory = new UnityEvent(); // Call to update inventory slots

    //======== Player Events ========//
    public UnityEvent e_playerDeath = new UnityEvent(); // Call when player is dead

}
