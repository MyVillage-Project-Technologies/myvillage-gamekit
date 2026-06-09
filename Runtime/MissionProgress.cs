using System;

namespace MyVillage.GameKit
{
    [Serializable]
    public sealed class MissionProgress
    {
        public int Score;
        public int Lives = -1;
        public int Level = -1;
    }
}
