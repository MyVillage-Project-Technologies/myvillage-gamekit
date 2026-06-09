using Unity.VisualScripting;

namespace MyVillage.GameKit.VisualScripting.HostUnits
{
    /// Visual Scripting node: ends the mission as a failure with a reason
    /// string (free text shown in admin logs).
    [UnitCategory("MyVillage/Mission")]
    [UnitTitle("Fail Mission")]
    [UnitSurtitle("MyVillage")]
    public sealed class FailMissionUnit : Unit
    {
        [DoNotSerialize] public ControlInput input;
        [DoNotSerialize] public ControlOutput output;
        [DoNotSerialize] public ValueInput reason;

        protected override void Definition()
        {
            input = ControlInput(nameof(input), Exec);
            output = ControlOutput(nameof(output));
            reason = ValueInput<string>(nameof(reason), "Player failed.");
            Succession(input, output);
        }

        ControlOutput Exec(Flow flow)
        {
            var host = HostResolver.Resolve(flow);
            host?.FailMission(flow.GetValue<string>(reason));
            return output;
        }
    }
}
