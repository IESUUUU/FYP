using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Answer
{
    [SerializeField] private string _info;
    public string Info { get { return _info; } }
    [SerializeField] private bool _isCorrect;
    public bool IsCorrect { get { return _isCorrect; } }
}

[CreateAssetMenu(fileName = "New Question", menuName = "Quiz/new Question")]
public class Question : ScriptableObject
{
    public enum AnswerType { Multi, Single};
    [SerializeField] private string info = string.Empty;
    public string Info { get { return info; } }
    [SerializeField] Answer[] answers = null; //size
    public Answer[] Answers { get { return answers; } }

    //parameters

    [SerializeField] private bool _useTimer = false;
    public bool UseTimer { get { return _useTimer; } }
    [SerializeField] private int _timer = 0;
    public int Timer { get { return _timer; } }

    [SerializeField] private AnswerType _answerType = AnswerType.Multi;
    public AnswerType GetAnswerType { get { return _answerType; } }

    [SerializeField] private int _addScore = 1;
    public int AddScore { get { return _addScore; } }
    [SerializeField] private int _zero = 1;
    public int ZeroScore { get { return _zero; } }

    // ========== additional modification for explanation =========
    [SerializeField] private string _explanation = string.Empty;
    public string Explanation { get { return _explanation; } }
    // ========== END of additional modification for explanation =========

    public List<int> GetCorrectAnswers()
    {
        List<int> CorrectAnswers = new List<int>();
        for (int i = 0; i < Answers.Length; i++)
        {
            if (Answers[i].IsCorrect)
            {
                CorrectAnswers.Add(i);
            }
        }
        return CorrectAnswers;
    }
}
