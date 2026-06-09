namespace MyVillage.GameKit.Templates
{
    /// Data-driven quiz mission: questions, scoring, and pacing are configured
    /// via a QuizMissionConfig asset. No subclassing needed for standard quizzes.
    ///
    /// v1.0 ships the contract; full implementation lands during M1 build-out.
    public sealed class QuizMission : MissionBase
    {
        QuizMissionConfig _config;
        int _score;
        int _correct;
        int _incorrect;
        int _questionIndex;

        protected override void OnInitialize()
        {
            _config = Config as QuizMissionConfig;
            if (_config == null)
                Host.LogError("QuizMission requires a QuizMissionConfig asset.");
        }

        protected override void OnBegin()
        {
            // TODO(M1): wire UI prefab, present first question, start per-question timer.
            // For now, log so the host knows we reached this state.
            Host.LogEvent("quiz.begin", null);
        }

        // TODO(M1): public methods invoked by the UI prefab on answer selection.
        // public void OnAnswerSelected(int choiceIndex) { ... }
        // public void OnTimerExpired() { ... }
    }
}
