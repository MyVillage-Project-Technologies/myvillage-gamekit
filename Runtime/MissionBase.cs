using System;
using UnityEngine;

namespace MyVillage.GameKit
{
    /// Base class for every mission. Scenes shipped via a MyVillage bundle must
    /// contain exactly one GameObject with a MissionBase subclass component;
    /// the host finds and drives it via Initialize -> Begin.
    [DisallowMultipleComponent]
    public abstract class MissionBase : MonoBehaviour
    {
        public MissionLifecycleState State { get; private set; } = MissionLifecycleState.Idle;

        protected IMissionHost Host { get; private set; }
        protected MissionConfig Config => Host?.Config;
        protected float TimeSinceBegin =>
            State >= MissionLifecycleState.Running ? Time.time - _beginTime : 0f;

        float _beginTime;

        public void Initialize(IMissionHost host)
        {
            if (State != MissionLifecycleState.Idle)
                throw new InvalidOperationException("Mission already initialized");
            Host = host ?? throw new ArgumentNullException(nameof(host));
            State = MissionLifecycleState.Initialized;
            Host.OnPauseChanged += HandleHostPauseChanged;
            OnInitialize();
        }

        public void Begin()
        {
            if (State != MissionLifecycleState.Initialized)
                throw new InvalidOperationException($"Cannot Begin from state {State}");
            _beginTime = Time.time;
            State = MissionLifecycleState.Running;
            OnBegin();
        }

        protected void CompleteMission(int finalScore, int correctAnswers = 0, int incorrectAnswers = 0, string difficulty = null)
        {
            EnsureState(MissionLifecycleState.Running, MissionLifecycleState.Paused);
            var result = new MissionResult
            {
                FinalScore = finalScore,
                CorrectAnswers = correctAnswers,
                IncorrectAnswers = incorrectAnswers,
                DurationSeconds = Mathf.RoundToInt(TimeSinceBegin),
                Difficulty = difficulty,
            };
            State = MissionLifecycleState.Completed;
            OnEnd();
            Host.CompleteMission(result);
        }

        protected void FailMission(string reason)
        {
            EnsureState(MissionLifecycleState.Running, MissionLifecycleState.Paused);
            State = MissionLifecycleState.Failed;
            OnEnd();
            Host.FailMission(reason);
        }

        protected void ReportProgress(int score, int lives = -1, int level = -1)
        {
            Host.ReportProgress(new MissionProgress
            {
                Score = score,
                Lives = lives,
                Level = level,
            });
        }

        // Lifecycle hooks for subclasses.
        protected virtual void OnInitialize() { }
        protected abstract void OnBegin();
        protected virtual void OnPause() { }
        protected virtual void OnResume() { }
        protected virtual void OnEnd() { }

        void OnDestroy()
        {
            if (Host != null)
                Host.OnPauseChanged -= HandleHostPauseChanged;
        }

        void HandleHostPauseChanged(bool isPaused)
        {
            if (isPaused && State == MissionLifecycleState.Running)
            {
                State = MissionLifecycleState.Paused;
                OnPause();
            }
            else if (!isPaused && State == MissionLifecycleState.Paused)
            {
                State = MissionLifecycleState.Running;
                OnResume();
            }
        }

        void EnsureState(params MissionLifecycleState[] allowed)
        {
            for (int i = 0; i < allowed.Length; i++)
                if (allowed[i] == State) return;
            throw new InvalidOperationException(
                $"Operation not valid from state {State}; expected one of: {string.Join(", ", allowed)}");
        }
    }
}
