using SFML.System;

namespace GameEngine
{
    // This represents a more complex timer that automatically Restarts and performs Restart actions. See Timer.cs for more explanation on how timers work.
    class AutoTimerGameObject : GameObject
    {
        // This is the internal Timer.
        private Timer _timer;

        // If AutoRestarts is true, this AutoTimerGameObject will automatically retart.
        public bool AutoRestarts { get; set; }

        // If AutoAddRestarts is true, this will automatically call AddRestart() instead of Restart().
        public bool AutoAddRestarts { get; set; }

        // The time in milliseconds when this was created or last restarted.
        public double StartMS { get => _timer.StartMS; }

        // This Timer is considered "expired" after it has existed for more than TimerLengthMS.
        public double TimerLengthMS { get => _timer.TimerLengthMS; set => _timer.TimerLengthMS = value; }
       
        // The Time since this was created or restarted.
        public Time TimeSinceStart { get => _timer.TimeSinceStart; }

        // The Time remaining until this is considered "expired".
        public Time TimeUntilExpired { get => _timer.TimeUntilExpired; }

        // True if the time since this was started (or last restarted) is longer than TimerLengthMS.
        public bool IsExpired { get => _timer.IsExpired; }

        // Constructs the _timer with the given length and sets AutoRestarts to false.
        public AutoTimerGameObject(double timerLengthMS)
        {
            _timer = new Timer(timerLengthMS);
            AutoRestarts = false;
        }

        // Constructs the _timer with the given length and sets AutoRestarts to true and AutoAddRestarts based on the given bool.
        public AutoTimerGameObject(double timerLengthMS, bool autoAddRestarts)
        {
            _timer = new Timer(timerLengthMS);
            AutoRestarts = true;
            AutoAddRestarts = autoAddRestarts;
        }

        // Restarts the internal Timer (Sets StartMS to the current time).
        public void Restart()
        {
            _timer.Restart();
            RestartAction();
        }

        // Additively restarts the internal Timer (Adds TimerLengthMs to StartMS).
        public void AddRestart()
        {
            _timer.AddRestart();
            RestartAction();
        }

        // Calls Restart() and returns true if this is "expired", returning false otherwise.
        public bool RestartIfExpired()
        {
            if (_timer.RestartIfExpired())
            {
                RestartAction();
                return true;
            }
            return false;
        }

        // Literally RestartIfExpired() but it calls AddRestart() instead of Restart().
        public bool AddRestartIfExpired()
        {
            if (_timer.AddRestartIfExpired())
            {
                RestartAction();
                return true;
            }
            return false;
        }

        // Sets AutoRestarts to true and AutoAddRestarts based on the given parameter.
        public void EnableAutoRestart(bool autoSubRestarts)
        {
            AutoAddRestarts = autoSubRestarts;
            AutoRestarts = true;
        }

        // Sets AutoRestarts to false.
        public void DisableRestart()
        {
            AutoRestarts = false;
        }

        // Checks to see if this is expired and automatically restarts if it is, as long as AutoRestarts is enabled.
        public override void Update(Time elapsed)
        {
            if (AutoRestarts)
            {
                if (_timer.IsExpired)
                {
                    if (AutoAddRestarts)
                    {
                        AddRestart();
                    }
                    else
                    {
                        Restart();
                    }
                }
            }
        }

        // This is called whenever this AutoTimerGameObject is restarted. Override it to make this do something cool when restarts!
        protected virtual void RestartAction() {}
    }
}