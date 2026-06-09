using Unity.VisualScripting;
using UnityEngine;

namespace MyVillage.GameKit.VisualScripting
{
    /// Shared helper that all MyVillage Custom Units use to find the
    /// IMissionHost that the M-UNI host injected before the graph started.
    ///
    /// Convention: the host calls
    ///     Variables.Object(missionGameObject).Set("missionHost", host);
    /// before the ScriptMachine wakes up. Units read it from the same scope.
    ///
    /// If the variable is missing (e.g. running in the editor without a host),
    /// the resolver returns null. Units treat null as a no-op so graphs don't
    /// throw — they just don't reach the host. The host warns once on the
    /// first failed resolve so devs see the misconfiguration.
    public static class HostResolver
    {
        const string VARIABLE_NAME = "missionHost";
        static bool _warned;

        public static IMissionHost Resolve(Flow flow)
        {
            if (flow == null) return null;
            var go = flow.stack?.gameObject;
            if (go == null) return WarnAndReturnNull("no GameObject on the flow stack");
            if (!Variables.Object(go).IsDefined(VARIABLE_NAME))
                return WarnAndReturnNull($"variable '{VARIABLE_NAME}' not defined on {go.name}");
            return Variables.Object(go).Get(VARIABLE_NAME) as IMissionHost;
        }

        static IMissionHost WarnAndReturnNull(string reason)
        {
            if (!_warned)
            {
                Debug.LogWarning(
                    $"[MyVillage.GameKit] IMissionHost not available to Visual Scripting graph: {reason}. " +
                    "If you're running outside M-UNI Universe (e.g. local editor preview), this is expected.");
                _warned = true;
            }
            return null;
        }
    }
}
