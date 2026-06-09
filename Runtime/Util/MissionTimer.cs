using UnityEngine;

namespace MyVillage.GameKit.Util
{
    /// Lightweight pausable timer for missions. Tracks elapsed time across
    /// pause/resume cycles. Designed to be safe to use with MissionBase.State.
    public sealed class MissionTimer
    {
        float _accumulatedSeconds;
        float _runStartTime;
        bool _running;

        public float ElapsedSeconds => _running ? _accumulatedSeconds + (Time.time - _runStartTime) : _accumulatedSeconds;
        public bool IsRunning => _running;

        public void Start()
        {
            if (_running) return;
            _runStartTime = Time.time;
            _running = true;
        }

        public void Pause()
        {
            if (!_running) return;
            _accumulatedSeconds += Time.time - _runStartTime;
            _running = false;
        }

        public void Reset()
        {
            _accumulatedSeconds = 0f;
            _running = false;
        }
    }
}
