using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class QuizManager : MonoBehaviour
{
    Question[] _questions = null;
    public Question[] Questions { get { return _questions; } }

    [SerializeField] QuizEvents events = null;

    [SerializeField] Animator timerAnimator = null;
    [SerializeField] TextMeshProUGUI timerText = null;
    [SerializeField] Color timerHalfWayOutColor = Color.yellow;
    [SerializeField] Color timerAlmostOutColor = Color.red;

    private List<AnswerData> PickedAnswers = new List<AnswerData>();
    private List<int> FinishedQuestions = new List<int> ();
    private int currentQuestion = 0;

    private int timerStateParameterHash = 0;

    private IEnumerator IE_WaitTillNextRound = null;
    private IEnumerator IE_StartTimer = null;
    private Color timerDefaultColor = Color.white;

    private bool IsFinished
    {
        get
        {
            return (FinishedQuestions.Count < (Questions.Length) ? false : true);
        }
    }

    void OnEnable()
    {
        events.UpdateQuestionAnswer += UpdateAnswers;
    }
    void OnDisable()
    {
        events.UpdateQuestionAnswer -= UpdateAnswers;
    }

    void Awake()
    {
        events.CurrentFinalScore = 0;
    }

    private void Start()
    {
        events.CurrentFinalScore = 0;
        //store high score
        events.StartupHighscore = PlayerPrefs.GetInt(Utility.PrefsQuizHighScore);
        timerDefaultColor = timerText.color;
        LoadQuestions();

        timerStateParameterHash = Animator.StringToHash("TimerState");

        var seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        UnityEngine.Random.InitState(seed);

        Display();
    }

    public void UpdateAnswers(AnswerData newAnswer)
    {
        if (Questions[currentQuestion].GetAnswerType == Question.AnswerType.Single)
        {
            foreach (var answer in PickedAnswers)
            {
                if(answer != newAnswer)
                {
                    answer.Reset();
                }
                PickedAnswers.Clear();
                PickedAnswers.Add(newAnswer);
            }
        }
        else
        {
            bool alreadyPicked = PickedAnswers.Exists(x => x == newAnswer);
            if (alreadyPicked)
            {
                PickedAnswers.Remove(newAnswer);
            }
            else
            {
                PickedAnswers.Add(newAnswer);
            }
        }
    }
    public void EraseAnswers()
    {
        PickedAnswers = new List<AnswerData>();
    }
    void Display()
    {
        // hide next button
        var uiElements = GetComponent<UIManager>();
        uiElements.ShowHideNextButton("hide");
        // erase explanation text
        uiElements.ShowExplanation(Questions[currentQuestion], "erase");

        EraseAnswers();
        var question = GetRandomQuestion();

        if (events.UpdateQuestionUI != null)
        {
            events.UpdateQuestionUI(question);
        }
        else
        {
            Debug.LogWarning("Ops! QuizEvents.UpdateQuestionUI is null. Issue in UizManager.Display() method.");
        }

        if (question.UseTimer)
        {
            UpdateTimer(question.UseTimer);
        }
    }

    //check answer
    public void Submit()
    {
        UpdateTimer(false);
        bool isCorrect = CheckAnswers();
        FinishedQuestions.Add(currentQuestion);

        // ==================== additional ==================
        if (!isCorrect)
        {
            var uiElements = GetComponent<UIManager>();
            uiElements.ShowExplanation(Questions[currentQuestion], "show");
        }
        // ==================== end of additional ==================
        else
        {
            TurnScreen(isCorrect);
        }
    }

    //show status + next question
    public void TurnScreen(bool isCorrect)
    {
        UpdateScore((isCorrect) ? Questions[currentQuestion].AddScore : Questions[currentQuestion].ZeroScore);

        if (IsFinished)
        {
            SetHighscore();
        }

        var type = (IsFinished) ? UIManager.ResolutionScreenType.Finish : (isCorrect) ? UIManager.ResolutionScreenType.Correct : UIManager.ResolutionScreenType.Incorrect;

        if (events.DisplayResolutionScreen != null)
        {
            events.DisplayResolutionScreen(type, Questions[currentQuestion].AddScore);
        }

        if (IE_WaitTillNextRound != null)
        {
            StopCoroutine(IE_WaitTillNextRound);
        }
        IE_WaitTillNextRound = WaitTillNextRound();
        StartCoroutine(IE_WaitTillNextRound);
    }

    void UpdateTimer(bool state)
    {
        switch (state)
        {
            case true:
                IE_StartTimer = StartTimer();
                StartCoroutine(IE_StartTimer);

                timerAnimator.SetInteger(timerStateParameterHash, 2);
                break;
            case false:
                if (IE_StartTimer != null)
                {
                    StopCoroutine(IE_StartTimer);
                }

                timerAnimator.SetInteger(timerStateParameterHash, 1);
                break;
        }
    }

    IEnumerator StartTimer()
    {
        var totalTime = Questions[currentQuestion].Timer;
        var timeLeft = totalTime;

        timerText.color = timerDefaultColor;
        while(timeLeft > 0)
        {
            timeLeft--;

            if (timeLeft < totalTime / 2 && timeLeft < totalTime / 4)
            {
                timerText.color = timerHalfWayOutColor;
            }
            if (timeLeft < totalTime / 4)
            {
                timerText.color = timerAlmostOutColor;
            }

            timerText.text = timeLeft.ToString();
            yield return new WaitForSeconds(1.0f);
        }
        Submit();
    }

    IEnumerator WaitTillNextRound()
    {
        yield return new WaitForSeconds(Utility.ResolutionDelayTime);
        Display();
    }
    Question GetRandomQuestion ()
    {
        var randomIndex = GetRandomQuestionIndex();
        currentQuestion = randomIndex;

        return Questions[currentQuestion];
    }
    int GetRandomQuestionIndex()
    {
        var random = 0;
        if(FinishedQuestions.Count < Questions.Length)
        {
            do
            {
                random = UnityEngine.Random.Range(0, Questions.Length);
            } while (FinishedQuestions.Contains(random) || random == currentQuestion);
        }
        return random;
    }

    bool CheckAnswers()
    {
        if (!CompareAnswers())
        {
            return false;
        }
        return true;
    }
    bool CompareAnswers()
    {
        if (PickedAnswers.Count > 0)
        {
            List<int> c = Questions[currentQuestion].GetCorrectAnswers();
            List<int> p = PickedAnswers.Select(x => x.AnswerIndex).ToList();

            var f = c.Except(p).ToList();
            var s = p.Except(c).ToList();

            return !f.Any() && !s.Any();
        }
        return false;
    }

    private void SetHighscore()
    {
        var highscore = PlayerPrefs.GetInt(Utility.PrefsQuizHighScore);
        if (highscore < events.CurrentFinalScore)
        {
            PlayerPrefs.SetInt(Utility.PrefsQuizHighScore, events.CurrentFinalScore);
            Utility.SavePrefsTimestamp();
            SaveScore();
        }
    }

    void SaveScore()
    {
        DBManager dbManager = FindObjectOfType<DBManager>();
        if (dbManager != null)
        {
            var script = dbManager.GetComponent<DBManager>();
            Debug.Log("uploading score");
            script.SyncButton();
            Debug.Log("uploaded");
        }
    }

    void LoadQuestions()
    {
        Object[] objs = Resources.LoadAll("Questions", typeof(Question));
        _questions = new Question[objs.Length];
        for (int i = 0; i < objs.Length; i++)
        {
            _questions[i] = (Question)objs[i];
        }
    }

    private void UpdateScore (int add)
    {
        events.CurrentFinalScore += add;

        if (events.ScoreUpdated != null)
        {
            events.ScoreUpdated();
        }
    }
}
