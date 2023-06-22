using SFML.System;

namespace GameEngine {
    // This class represents a simple timer. When the timer has existed for TimerLengthMS milliseconds, it is considered "expired".
    struct Timer
    {
        // The time in milliseconds when this was created or last restarted.
        public double StartMS { get; private set; }

        // This Timer is considered "expired" after it has existed for more than TimerLengthMS.
        public double TimerLengthMS { get; set; }

        // The Time since this was created or last restarted.
        public Time TimeSinceStart { get => Time.FromMicroseconds((long)(1000 * (Game.CurrentTimeMS - StartMS))); }

        // The Time remaining until this is considered "expired".
        public Time TimeUntilExpired { get => Time.FromMicroseconds((long)(1000 * (StartMS + TimerLengthMS - Game.CurrentTimeMS))); }

        // True if the time since this was started (or last restarted) is longer than TimerLengthMS.
        public bool IsExpired { get => Game.CurrentTimeMS > StartMS + TimerLengthMS; }

        // Initializes StartMS to the current time and TimerLengthMS to the given parameter.
        public Timer(double timerLengthMS)
        {
            StartMS = Game.CurrentTimeMS;
            TimerLengthMS = timerLengthMS;
        }

        // Sets StartMS to the current time.
        public void Restart()
        {
            StartMS = Game.CurrentTimeMS;
        }

        // Instead of setting StartMS to the current time, TimerLengthMS is added to StartMS. This is a more accurate way of tracking a repeated action.
        public void AddRestart()
        {
            StartMS += TimerLengthMS;
        }

        // Calls Restart() and returns true if this Timer is "expired", returning false otherwise.
        public bool RestartIfExpired()
        {
            if (IsExpired)
            {
                Restart();
                return true;
            }
            return false;
        }

        // Literally RestartIfExpired() but it calls AddRestart() instead of Restart().
        public bool AddRestartIfExpired()
        {
            if (IsExpired)
            {
                AddRestart();
                return true;
            }
            return false;
        }
    }
}