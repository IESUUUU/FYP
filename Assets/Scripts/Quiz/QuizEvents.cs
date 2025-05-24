using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuizEvents", menuName = "Quiz/new QuizEvents")]
public class QuizEvents : ScriptableObject
{

    public delegate void UpdateQuestionUICallback(Question question);
    public UpdateQuestionUICallback UpdateQuestionUI = null;

    public delegate void UpdateQuestionAnswerCallback(AnswerData pickedAnswer);
    public UpdateQuestionAnswerCallback UpdateQuestionAnswer = null;

    public delegate void DisplayResolutionScreenCallback(UIManager.ResolutionScreenType type, int score);
    public DisplayResolutionScreenCallback DisplayResolutionScreen = null;

    public delegate void ScoreUpdatedCallback();
    public ScoreUpdatedCallback ScoreUpdated = null;

    //[HideInInspector]
    public int CurrentFinalScore = 0;
    [HideInInspector]
    public int StartupHighscore = 0;
}