using SFML.System;

namespace GameEngine {
    // This class represents a timer. Meant to be used as a member object and not a game object.
    class Timer : GameObject {

        // Current Time in ms.
        private float _time;
        public float Time {
            get => _time;
        }

        // Time to surpass.
        private int _targetTime;

        // Automatically resets the timer as soon as it surpasses _targetTime if set to true.
        private bool _autoReset;

        // Initializes _targetTime and _autoreset based on given parameters.
        public Timer(int targetTime, bool autoReset)
        {
            BelongsOnTree = true;
            IsCollidable = false;
            _isCollisionCheckEnabled = false;

            _targetTime = targetTime;
            _autoReset = autoReset;
            _time = 0f;
        }

        // Counts _time up and subtract resets it if _autoReset is true. 
        public override void Update(Time elapsed)
        {
            // Using AsMicroseconds() / 1000f instead AsMilliseconds() improves timer consistency, especially at high framerates
            _time += elapsed.AsMicroseconds() / 1000f;
            if (_autoReset)
            {
                SubResetIfSurpassed();
            }
        }

        // True if _time has surpassed _targetTime.
        public bool SurpassedTarget { get => _time >= _targetTime; }

        // Checks if the _time has surpassed _targetTime. If true, it resets and returns it. Otherwise false is returned.
        public bool ResetIfSurpassed()
        {
            if (SurpassedTarget)
            {
                Reset();
                return true;
            }
            return false;
        }

        // Does the same thing as resetIfSurpassed, except it calls subReset and not reset.
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
            _time = 0f;
            ResetAction();
        }

        // Subracts the _targetTime instead of setting _time to 0. Generally improves timer consistency vs forceReset.
        public void SubReset()
        {
            _time -= _targetTime;
            ResetAction();
        }

        // Override to make Timer do something else on reset.
        public virtual void ResetAction() {}
    }
}