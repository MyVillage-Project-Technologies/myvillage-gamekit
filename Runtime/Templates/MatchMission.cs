namespace MyVillage.GameKit.Templates
{
    /// Data-driven pair-matching mission. v1.0 ships the contract; full
    /// implementation lands during M1 build-out.
    public sealed class MatchMission : MissionBase
    {
        MatchMissionConfig _config;

        protected override void OnInitialize()
        {
            _config = Config as MatchMissionConfig;
            if (_config == null)
                Host.LogError("MatchMission requires a MatchMissionConfig asset.");
        }

        protected override void OnBegin()
        {
            // TODO(M1): build grid, shuffle pairs, wire input handlers.
            Host.LogEvent("match.begin", null);
        }
    }
}
