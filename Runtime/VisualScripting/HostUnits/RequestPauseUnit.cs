using Unity.VisualScripting;

namespace MyVillage.GameKit.VisualScripting.HostUnits
{
    /// Visual Scripting node: request a pause or resume from the host. The
    /// host owns the pause overlay and fires OnPauseChanged on IMissionHost.
    [UnitCategory("MyVillage/Mission")]
    [UnitTitle("Request Pause")]
    [UnitSurtitle("MyVillage")]
    public sealed class RequestPauseUnit : Unit
    {
        [DoNotSerialize] public ControlInput pause;
        [DoNotSerialize] public ControlInput resume;
        [DoNotSerialize] public ControlOutput output;

        protected override void Definition()
        {
            pause = ControlInput(nameof(pause), DoPause);
            resume = ControlInput(nameof(resume), DoResume);
            output = ControlOutput(nameof(output));
            Succession(pause, output);
            Succession(resume, output);
        }

        ControlOutput DoPause(Flow flow)
        {
            HostResolver.Resolve(flow)?.RequestPause();
            return output;
        }

        ControlOutput DoResume(Flow flow)
        {
            HostResolver.Resolve(flow)?.RequestResume();
            return output;
        }
    }
}
