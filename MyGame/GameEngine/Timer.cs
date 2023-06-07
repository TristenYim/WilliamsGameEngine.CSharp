using SFML.System;

namespace GameEngine {
    // This class represents a timer. Can be used as a member inside another class, or as a GameObject itself.
    class Timer : GameObject {

        // Current Time in ms.
        public float Time { get; set; }

        // Time to surpass in ms.
        public int TargetTime { get; set; }

        // Automatically resets the timer as soon as it surpasses _targetTime if set to true.
        private bool _autoReset;

        // Initializes _targetTime and _autoreset based on given parameters.
        public Timer(int targetTime, bool autoReset)
        {
            BelongsOnTree = false;
            IsCollidable = false;
            _isCollisionCheckEnabled = false;

            TargetTime = targetTime;
            _autoReset = autoReset;
            Time = 0f;
        }

        // Counts _time up and subtract resets it if _autoReset is true. 
        public override void Update(Time elapsed)
        {
            // Using AsMicroseconds() / 1000f instead AsMilliseconds() improves timer consistency, especially at high framerates
            Time += elapsed.AsMicroseconds() / 1000f;
            if (_autoReset)
            {
                SubResetIfSurpassed();
            }
        }

        // True if _time has surpassed _targetTime.
        public bool SurpassedTarget { get => Time >= TargetTime; }

        // Returns true and calls Reset if _time has surpassed _targetTime. Returns false otherwise.
        public bool ResetIfSurpassed()
        {
            if (SurpassedTarget)
            {
                Reset();
                return true;
            }
            return false;
        }

        // Does the same thing as ResetIfSurpassed, except it calls SubReset and not Reset.
        public bool SubResetIfSurpassed()
        {
            if (SurpassedTarget)
            {
                SubReset();
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