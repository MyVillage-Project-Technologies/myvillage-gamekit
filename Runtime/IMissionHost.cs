using System;
using System.Collections.Generic;

namespace MyVillage.GameKit
{
    /// Implemented by the M-UNI Universe host. Mission code calls into this
    /// interface; it never implements it. Provides session identity, terminal
    /// handoff (complete/fail/abandon), pause control, and telemetry.
    public interface IMissionHost
    {
        string GameSlug { get; }
        string SessionId { get; }
        string MissionId { get; }
        MissionConfig Config { get; }

        void ReportProgress(MissionProgress progress);

        void CompleteMission(MissionResult result);
        void FailMission(string reason);
        void AbandonMission();

        void RequestPause();
        void RequestResume();
        event Action<bool> OnPauseChanged;

        void LogEvent(string eventName, IDictionary<string, object> properties = null);
        void LogError(string message, Exception exception = null);
    }
}
