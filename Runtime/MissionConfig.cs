using UnityEngine;

namespace MyVillage.GameKit
{
    public abstract class MissionConfig : ScriptableObject
    {
        [Tooltip("Unique identifier for this mission within the game.")]
        public string MissionId;

        [Tooltip("Name shown in HUD and result panel.")]
        public string DisplayName;

        [Tooltip("Score threshold treated as 'success' in the result UI. 0 disables.")]
        public int TargetScore;
    }
}
