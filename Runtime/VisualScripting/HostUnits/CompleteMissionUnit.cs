using Unity.VisualScripting;
using UnityEngine;

namespace MyVillage.GameKit.VisualScripting.HostUnits
{
    /// Visual Scripting node: ends the mission with the given score.
    /// Reads IMissionHost from the Object-scope variable "missionHost"
    /// (set by the host at bundle load time).
    [UnitCategory("MyVillage/Mission")]
    [UnitTitle("Complete Mission")]
    [UnitSurtitle("MyVillage")]
    public sealed class CompleteMissionUnit : Unit
    {
        [DoNotSerialize] public ControlInput input;
        [DoNotSerialize] public ControlOutput output;
        [DoNotSerialize] public ValueInput score;
        [DoNotSerialize] public ValueInput correctAnswers;
        [DoNotSerialize] public ValueInput incorrectAnswers;

        protected override void Definition()
        {
            input = ControlInput(nameof(input), Exec);
            output = ControlOutput(nameof(output));
            score = ValueInput<int>(nameof(score), 0);
            correctAnswers = ValueInput<int>(nameof(correctAnswers), 0);
            incorrectAnswers = ValueInput<int>(nameof(incorrectAnswers), 0);

            Succession(input, output);
            Requirement(score, input);
        }

        ControlOutput Exec(Flow flow)
        {
            var host = HostResolver.Resolve(flow);
            host?.CompleteMission(new MissionResult
            {
                FinalScore = flow.GetValue<int>(score),
                CorrectAnswers = flow.GetValue<int>(correctAnswers),
                IncorrectAnswers = flow.GetValue<int>(incorrectAnswers),
            });
            return output;
        }
    }
}
