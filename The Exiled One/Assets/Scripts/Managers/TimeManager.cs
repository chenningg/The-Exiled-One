using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

// Manages the time and day of the game
public class TimeManager : MonoBehaviour {

    #region Instance
    private static TimeManager timeManagerInstance;

    public static TimeManager Instance { get { return timeManagerInstance; } }

    private void Awake()
    {
        if (timeManagerInstance != null && timeManagerInstance != this)
        {
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
            return;
        }
        timeManagerInstance = this;
    }
    #endregion

    // References
    private EventManager eventManager;
    private GameManager gameManager;

    // Variables
    public float dayLength; // Day length in seconds
    public bool timeIsRunning = true; // Pause time;
    public int timeForDawn; // Sets time for dawn event to be called
    public int timeForDay; // Sets time for day event to be called
    public int timeForDusk; // Sets time for dusk event to be called
    public int timeForNight; // Sets time for night event to be called

    private int days; // Number of days elapsed
    private float time = 0; // Time since new game started
    private int currentTime = 0; // Time in game time (24hr clock)
    
    // Bool checks
    private bool dawnEventCalled = false;
    private bool dayEventCalled = false;
    private bool duskEventCalled = false;
    private bool nightEventCalled = false;
    private bool hourlyEventCalled = false;

    private IEnumerator gameClock;  

    void Start()
    {

        // Events
        EventManager.Instance.e_newGame.AddListener(NewGameClock);
        EventManager.Instance.e_loadGame.AddListener(Load);
        EventManager.Instance.e_gameLoaded.AddListener(StartGameClock);
        EventManager.Instance.e_saveGame.AddListener(Save);
        EventManager.Instance.e_pauseGame.AddListener(PauseGameClock);
        EventManager.Instance.e_resumeGame.AddListener(StartGameClock);

        // Variables
        timeIsRunning = false;

        // TESTING
        StartGameClock();
    }

    void OnDisable()
    {
        EventManager.Instance.e_newGame.RemoveListener(NewGameClock);
        EventManager.Instance.e_loadGame.RemoveListener(Load);
        EventManager.Instance.e_gameLoaded.RemoveListener(StartGameClock);
        EventManager.Instance.e_saveGame.RemoveListener(Save);
        EventManager.Instance.e_pauseGame.RemoveListener(PauseGameClock);
        EventManager.Instance.e_resumeGame.RemoveListener(StartGameClock);
    }

    // Accessors for other classes

    public int GetCurrentTime()
    {
        return (currentTime);
    }

    public float GetTime()
    {
        return (time);
    }

    public int GetNumberOfDays()
    {
        return (days);
    }

    public float GetDayLength()
    {
        return (dayLength);
    }

    public string GetTimeOfTheDay()
    {
        if (currentTime >= timeForNight)
        {
            return ("Night");
        }
        else if (currentTime >= timeForDusk)
        {
            return ("Dusk");
        }
        else if (currentTime >= timeForDay)
        {
            return ("Day");
        }
        else if (currentTime >= timeForDawn)
        {
            return ("Dawn");
        }
        else
        {
            return ("Night");
        }
    }

    // GAME CLOCK
    // When player starts a new game, start day at 8am.

    void NewGameClock()
    {
        currentTime = timeForDay;
    }

    public void StartGameClock()
    {
        timeIsRunning = true;

        if (gameClock != null)
        {
            StopCoroutine(gameClock);
            gameClock = null;
        }

        gameClock = GameClock();
        StartCoroutine(gameClock);
    }

    public void PauseGameClock()
    {
        timeIsRunning = false;
    }

    private IEnumerator GameClock()
    {
        while (timeIsRunning)
        {
            // Update time for saving
            time += 1;

            // If a day has passed
            if (time % dayLength == 0)
            {
                days += 1;
                currentTime = 0;
                dawnEventCalled = false;
                dayEventCalled = false;
                duskEventCalled = false;
                nightEventCalled = false;
            }

            // Update current game time (24hr clock)
            currentTime = Mathf.Clamp(Mathf.RoundToInt(((time % dayLength) / dayLength) * 2400), 0, 2400);

            // Call time events
            CallTimeEvents();

            // Call hourly event
            CallHourlyEvent();

            yield return new WaitForSecondsRealtime(1);
        }

        gameClock = null;
    }

    private void CallHourlyEvent()
    {
        if (currentTime % 100 == 0)
        {
            if (!hourlyEventCalled)
            {
                hourlyEventCalled = true;
                EventManager.Instance.e_hourPass.Invoke();
            }               
        }
        else
        {
            hourlyEventCalled = false;
        }
    }

    private void CallTimeEvents()
    {
        // Call time events
        switch (GetTimeOfTheDay())
        {
            case ("Dawn"):
                if (!dawnEventCalled)
                {
                    dawnEventCalled = true;
                    EventManager.Instance.e_dawnTime.Invoke();
                }
                return;
            case ("Day"):
                if (!dayEventCalled)
                {
                    dayEventCalled = true;
                    nightEventCalled = false;
                    EventManager.Instance.e_dayTime.Invoke();
                }
                return;
            case ("Dusk"):
                if (!duskEventCalled)
                {
                    duskEventCalled = true;
                    EventManager.Instance.e_duskTime.Invoke();
                }
                return;
            case ("Night"):
                if (!nightEventCalled)
                {
                    nightEventCalled = true;
                    EventManager.Instance.e_nightTime.Invoke();
                }
                return;
        }
    }


    // SAVELOAD DATA

    public void Save()
    {
        var data = new TimeData();
        data.days = days;
        data.time = time;
        data.currentTime = currentTime;
        GameManager.Instance.gameData.time = data;
    }

    public void Load()
    {
        days = GameManager.Instance.gameData.time.days;
        time = GameManager.Instance.gameData.time.time;
        currentTime = GameManager.Instance.gameData.time.currentTime;

        // Call time events
        CallTimeEvents();
    }

    [System.Serializable]
    public class TimeData
    {
        public int days; // Number of days elapsed
        public float time; // Time since new game started
        public int currentTime; // Time in game time (24hr clock, hours only)
    }
}
