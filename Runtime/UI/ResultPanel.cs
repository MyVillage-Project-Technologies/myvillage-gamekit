using UnityEngine;

namespace MyVillage.GameKit.UI
{
    /// End-of-mission summary panel: final score, correct/incorrect tally,
    /// time taken, and a continue button. Devs use this by default; missions
    /// that need a custom outro can disable it.
    ///
    /// v1.0 ships the contract; full implementation lands during M1 build-out.
    public sealed class ResultPanel : MonoBehaviour
    {
        // TODO(M1): show(MissionResult result) entry point; render fields.
    }
}
