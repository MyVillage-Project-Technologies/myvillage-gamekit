using System;

namespace MyVillage.GameKit
{
    [Serializable]
    public sealed class MissionResult
    {
        public int FinalScore;
        public int CorrectAnswers;
        public int IncorrectAnswers;
        public int DurationSeconds;
        public string Difficulty;
    }
}
