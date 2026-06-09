using UnityEngine;

namespace MyVillage.GameKit.Templates
{
    [CreateAssetMenu(menuName = "MyVillage/Match Mission Config", fileName = "MatchMissionConfig")]
    public sealed class MatchMissionConfig : MissionConfig
    {
        [Tooltip("Grid dimensions (must be even total).")]
        public Vector2Int GridSize = new Vector2Int(4, 4);

        [Tooltip("Sprites used as card faces; pairs are formed from this set.")]
        public Sprite[] CardFaces;

        [Tooltip("Points per matched pair.")]
        public int PointsPerMatch = 100;

        [Tooltip("Mismatch penalty.")]
        public int MismatchPenalty = 25;
    }
}
