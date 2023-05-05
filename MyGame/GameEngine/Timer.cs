using SFML.System;

namespace GameEngine {
    // This class represents a timer. Meant to be used as a member object and not a game object.
    class Timer : GameObject {

        // Current time in ms.
        private float _time;
        public float Time {
            get { return _time; }
        }

        // Time to surpass.
        private int _targetTime;

        // Automatically resets the timer as soon as it surpasses the target if set to true.
        private bool _autoReset;

        // Initializes the target time and autoreset based on parameters.
        public Timer(int targetTime, bool autoReset) {
            _targetTime = targetTime;
            _autoReset = autoReset;
            _time = 0f;
        }

        // Counts the timer up and subtract resets it if auto reset is enabled. 
        public override void Update(Time elapsed) {
            // Using AsMicroseconds() / 1000f instead AsMilliseconds() improves timer consistency, especially at high framerates
            _time += elapsed.AsMicroseconds() / 1000f;
            if (_autoReset) {
                subResetIfSurpassed();
            }
        }

        // Returns true if the timer has surpassed the target and false otherwise.
        public bool surpassedTarget() {
            return _time >= _targetTime;
        }

        // Checks if the timer has surpassed the target. If true, it resets and returns it. Otherwise false is returned.
        public bool resetIfSurpassed() {
            if (surpassedTarget()) {
                reset();
                return true;
            }
            return false;
        }

        // Does the same thing as resetIfSurpassed, except it calls subReset and not reset.
        public bool subResetIfSurpassed() {
            if (surpassedTarget()) {
                subReset();
                return true;
            }
            return false;
        }

        // Resets the timer to 0.
        public void reset() {
            _time = 0f;
            resetAction();
        }

        // Subracts the target time instead of setting the timer to 0. Generally improves timer consistency vs forceReset.
        public void subReset() {
            _time -= _targetTime;
            resetAction();
        }

        // Override to make the timer do something else on reset.
        public virtual void resetAction() {}
    }
}