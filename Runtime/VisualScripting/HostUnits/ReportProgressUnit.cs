using Unity.VisualScripting;

namespace MyVillage.GameKit.VisualScripting.HostUnits
{
    /// Visual Scripting node: report in-flight progress (score, lives, level).
    /// Used for HUD updates and partial-save telemetry. Lives/Level default to
    /// -1 which means "not used" — leave them at the default if your game
    /// doesn't have lives or levels.
    [UnitCategory("MyVillage/Mission")]
    [UnitTitle("Report Progress")]
    [UnitSurtitle("MyVillage")]
    public sealed class ReportProgressUnit : Unit
    {
        [DoNotSerialize] public ControlInput input;
        [DoNotSerialize] public ControlOutput output;
        [DoNotSerialize] public ValueInput score;
        [DoNotSerialize] public ValueInput lives;
        [DoNotSerialize] public ValueInput level;

        protected override void Definition()
        {
            input = ControlInput(nameof(input), Exec);
            output = ControlOutput(nameof(output));
            score = ValueInput<int>(nameof(score), 0);
            lives = ValueInput<int>(nameof(lives), -1);
            level = ValueInput<int>(nameof(level), -1);
            Succession(input, output);
        }

        ControlOutput Exec(Flow flow)
        {
            var host = HostResolver.Resolve(flow);
            host?.ReportProgress(new MissionProgress
            {
                Score = flow.GetValue<int>(score),
                Lives = flow.GetValue<int>(lives),
                Level = flow.GetValue<int>(level),
            });
            return output;
        }
    }
}
