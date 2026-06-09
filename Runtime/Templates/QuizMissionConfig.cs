using System;
using UnityEngine;

namespace MyVillage.GameKit.Templates
{
    [CreateAssetMenu(menuName = "MyVillage/Quiz Mission Config", fileName = "QuizMissionConfig")]
    public sealed class QuizMissionConfig : MissionConfig
    {
        [Serializable]
        public sealed class Question
        {
            public string Prompt;
            public string[] Choices;
            public int CorrectChoiceIndex;
            public int Points = 10;
        }

        [Tooltip("Seconds to answer each question. 0 disables the per-question timer.")]
        public float SecondsPerQuestion = 15f;

        [Tooltip("Shuffle question order at runtime.")]
        public bool ShuffleQuestions = true;

        public Question[] Questions;
    }
}
