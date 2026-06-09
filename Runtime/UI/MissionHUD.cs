using UnityEngine;

namespace MyVillage.GameKit.UI
{
    /// Heads-up display overlay for missions: score, timer, pause button.
    /// Owns the pause UX; mission code only calls Host.RequestPause() if needed.
    ///
    /// v1.0 ships the contract; full implementation lands during M1 build-out.
    public sealed class MissionHUD : MonoBehaviour
    {
        // TODO(M1): public fields for score/timer Text references, pause button.
        // TODO(M1): wire to mission.Host events for state changes.
    }
}
