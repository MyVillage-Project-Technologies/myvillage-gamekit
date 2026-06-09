using System.Collections.Generic;
using Unity.VisualScripting;

namespace MyVillage.GameKit.VisualScripting.HostUnits
{
    /// Visual Scripting node: send a named telemetry event to the host with
    /// an optional value. Useful for analytics (level start/end, key choices,
    /// power-ups picked up).
    [UnitCategory("MyVillage/Mission")]
    [UnitTitle("Log Event")]
    [UnitSurtitle("MyVillage")]
    public sealed class LogEventUnit : Unit
    {
        [DoNotSerialize] public ControlInput input;
        [DoNotSerialize] public ControlOutput output;
        [DoNotSerialize] public ValueInput eventName;
        [DoNotSerialize] public ValueInput value;

        protected override void Definition()
        {
            input = ControlInput(nameof(input), Exec);
            output = ControlOutput(nameof(output));
            eventName = ValueInput<string>(nameof(eventName), "event");
            value = ValueInput<string>(nameof(value), string.Empty);
            Succession(input, output);
            Requirement(eventName, input);
        }

        ControlOutput Exec(Flow flow)
        {
            var host = HostResolver.Resolve(flow);
            if (host == null) return output;
            var name = flow.GetValue<string>(eventName);
            var val = flow.GetValue<string>(value);
            if (string.IsNullOrEmpty(val))
            {
                host.LogEvent(name);
            }
            else
            {
                host.LogEvent(name, new Dictionary<string, object> { { "value", val } });
            }
            return output;
        }
    }
}
