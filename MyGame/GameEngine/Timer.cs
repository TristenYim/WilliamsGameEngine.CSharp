using SFML.System;

namespace GameEngine {
    // This class represents a timer. Can be used as a member inside another class, or as a GameObject itself.
    class Timer : GameObject {

        // Current Time in ms.
        public float Time { get; set; }

        // Time to surpass in ms.
        public float TargetTime { get; set; }

        // Automatically resets the timer as soon as it surpasses _targetTime if set to true.
        private bool _autoReset;

        // Determines if the timer should SubReset or Reset when it surpasses _targetTime.
        private bool _autoSubReset;

        // Initializes _targetTime based on the given time, and _autoreset set to false.
        public Timer(float targetTime)
        {
            TargetTime = targetTime;
            _autoReset = false;
            Time = 0f;
        }

        // Initializes _targetTime and _autoSubReset based on the given parameters, with _autoReset set to true.
        public Timer(float targetTime, bool autoSubReset)
        {
            TargetTime = targetTime;
            _autoReset = true;
            _autoSubReset = autoSubReset;
            Time = 0f;   
        }

        // Counts _time up and subtract resets it if _autoReset is true. 
        public override void Update(Time elapsed)
        {
            // Using AsMicroseconds() / 1000f instead AsMilliseconds() improves timer consistency, especially at high framerates
            Time += elapsed.AsMicroseconds() / 1000f;
            if (_autoReset)
            {
                if (_autoSubReset)
                {
                    SubResetIfSurpassed();
                }
                else
                {
                    ResetIfSurpassed();
                }
            }
        }

        // True if _time has surpassed _targetTime.
        public bool SurpassedTarget { get => Time >= TargetTime; }

        // Returns true and calls Reset if _time has surpassed _targetTime. Returns false otherwise.
        public bool ResetIfSurpassed()
        {
            if (SurpassedTarget)
            {
                int extraResetActions = (int)((Time - TargetTime) / TargetTime);
                Reset();

                // Ensures ResetAction() is called the correct number of times regardless of the framerate.
                while (extraResetActions > 0)
                {
                    ResetAction();
                    extraResetActions--;
                }
                return true;
            }
            return false;
        }

        // Does the same thing as ResetIfSurpassed, except it calls SubReset and not Reset.
        public bool SubResetIfSurpassed()
        {
            if (SurpassedTarget)
            {   
                // Ensures ResetAction() is called the correct number of times regardless of the framerate.
                while (SurpassedTarget)
                {
                    SubReset();
                }
                return true;
            }
            return false;
        }

        // Resets the _time to 0 and calls ResetAction.
        public void Reset()
        {
            Time = 0f;
            ResetAction();
        }

        // Subracts the _targetTime instead of setting _time to 0. Generally improves consistency vs Reset.
        public void SubReset()
        {
            Time -= TargetTime;
            ResetAction();
        }

        // Override to make Timer do something on Reset (like spawn a meteor in the space invaders game for example).
        public virtual void ResetAction() {}
    }
}