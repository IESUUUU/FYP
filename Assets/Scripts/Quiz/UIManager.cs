using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

[Serializable()]
public struct UIManagerParameters
{
    [Header("Answers Options")]
    [SerializeField] float margins;
    public float Margins { get { return margins; } }

    [Header("Resolution Screen Options")]
    [SerializeField] Color correctBGColor;
    public Color CorrectBGColor { get { return correctBGColor; } }
    [SerializeField] Color incorrectBGColor;
    public Color IncorrectBGColor { get { return incorrectBGColor; } }
    [SerializeField] Color finalBGColor;
    public Color FinalBGColor { get { return finalBGColor; } }

}
[Serializable()]
public struct UIElements
{
    [SerializeField] RectTransform answerContentArea;
    public RectTransform AnswerContentArea { get { return answerContentArea; } }
    // ========== additional ==========
    [SerializeField] TextMeshProUGUI answerExplanation;
    public TextMeshProUGUI Explanation { get { return answerExplanation; } }
    [SerializeField] GameObject nextButton;
    public GameObject NextButton { get { return nextButton; } }
    // ========== end of additional ==========
    [SerializeField] TextMeshProUGUI questionInfoTextObject;
    public TextMeshProUGUI QuestionInfoTextObject { get { return questionInfoTextObject; } }
    [SerializeField] TextMeshProUGUI scoreText;
    public TextMeshProUGUI ScoreText { get { return scoreText; } }

    [Space]

    [SerializeField] Animator resolutionScreenAnimator;
    public Animator ResolutionScreenAnimator { get { return resolutionScreenAnimator; } }

    [SerializeField] Image resolutionBG;
    public Image ResolutionBG { get { return resolutionBG; } }
    [SerializeField] TextMeshProUGUI resolutionStateInfoText;
    public TextMeshProUGUI ResolutionStateInfoText { get { return resolutionStateInfoText; } }
    [SerializeField] TextMeshProUGUI resolutionScoreText;
    public TextMeshProUGUI ResolutionScoreText { get { return resolutionScoreText; } }

    [Space]

    [SerializeField] TextMeshProUGUI highScoreText;
    public TextMeshProUGUI HighScoreText { get { return highScoreText; } }
    [SerializeField] CanvasGroup mainCanvasGroup;
    public CanvasGroup MainCanvasGroup { get { return mainCanvasGroup; } }
    [SerializeField] RectTransform finishUIElements;
    public RectTransform FinishUIElements { get { return finishUIElements; } }

}
public class UIManager : MonoBehaviour
{
    public enum ResolutionScreenType { Correct, Incorrect, Finish};

    [Header("References")]
    [SerializeField] QuizEvents events;

    [Header("UI Elements (Prefabs)")]
    [SerializeField] AnswerData answerPrefabs;

    [SerializeField] UIElements uiElements;

    [Space]
    [SerializeField] UIManagerParameters parameters;

    List<AnswerData> currentAnswer = new List<AnswerData> ();
    private int resStateParaHash = 0;
    private IEnumerator IE_DisplayTimedResolution;

    void OnEnable()
    {
        events.UpdateQuestionUI += UpdateQuestionUI;
        events.DisplayResolutionScreen += DisplayResolution;
        events.ScoreUpdated += UpdateScoreUI;
    }

    void OnDisable()
    {
        events.UpdateQuestionUI -= UpdateQuestionUI;
        events.DisplayResolutionScreen -= DisplayResolution;
        events.ScoreUpdated -= UpdateScoreUI;
    }

    void Start()
    {
        UpdateScoreUI();
        resStateParaHash = Animator.StringToHash("ScreenState");
    }

    void UpdateQuestionUI (Question question)
    {
        uiElements.QuestionInfoTextObject.text = question.Info;
        CreateAnswers(question);
    }

    void DisplayResolution(ResolutionScreenType type, int score)
    {
        UpdateResUI(type, score);
        uiElements.ResolutionScreenAnimator.SetInteger(resStateParaHash, 2);
        //make maincanvas unable to interact
        uiElements.MainCanvasGroup.blocksRaycasts = false;

        if (type != ResolutionScreenType.Finish)
        {
            if (IE_DisplayTimedResolution != null)
            {
                StopCoroutine(IE_DisplayTimedResolution);
            }
            IE_DisplayTimedResolution = DisplayTimeResolution();
            StartCoroutine(IE_DisplayTimedResolution);
        }
    }

    IEnumerator DisplayTimeResolution()
    {
        yield return new WaitForSeconds(Utility.ResolutionDelayTime);
        uiElements.ResolutionScreenAnimator.SetInteger(resStateParaHash,1);
        uiElements.MainCanvasGroup.blocksRaycasts = true;
    }

    void UpdateResUI(ResolutionScreenType type, int score)
    {
        var highscore = PlayerPrefs.GetInt(Utility.PrefsQuizHighScore);
        switch (type)
        {
            case ResolutionScreenType.Correct:
                uiElements.ResolutionBG.color = parameters.CorrectBGColor;
                uiElements.ResolutionStateInfoText.text = "CORRECT!";
                uiElements.ResolutionScoreText.text = "+" + score
                    + "\nCurrent score: " + events.CurrentFinalScore;
                break;
            case ResolutionScreenType.Incorrect:
                uiElements.ResolutionBG.color = parameters.IncorrectBGColor;
                uiElements.ResolutionStateInfoText.text = "WRONG!";
                uiElements.ResolutionScoreText.text = "Current score: " + events.CurrentFinalScore;
                break;
            case ResolutionScreenType.Finish:
                uiElements.ResolutionBG.color = parameters.FinalBGColor;
                uiElements.ResolutionStateInfoText.text = "FINAL SCORE";
                uiElements.ResolutionScoreText.text = events.CurrentFinalScore.ToString();

                StartCoroutine(CalculateScore());
                uiElements.FinishUIElements.gameObject.SetActive(true);
                uiElements.HighScoreText.gameObject.SetActive(true);

                //Display highscore
                uiElements.HighScoreText.text = ((highscore > events.StartupHighscore)? "<color=yellow>new </color>" : String.Empty) + "Highscore: " + highscore;
                break;
        }
    }

    IEnumerator CalculateScore()
    {
        var ScoreValue = 0;
        while (ScoreValue < events.CurrentFinalScore)
        {
            ScoreValue++;
            uiElements.ResolutionScoreText.text = ScoreValue.ToString();

            yield return null;
        }
    }

    void CreateAnswers (Question question)
    {
        EraseAnswers();

        float offset = 0 - parameters.Margins;
        for (int i = 0; i < question.Answers.Length; i++)
        {
            AnswerData newAnswer = (AnswerData)Instantiate(answerPrefabs, uiElements.AnswerContentArea);
            newAnswer.UpdateData(question.Answers[i].Info, i);

            newAnswer.Rect.anchoredPosition = new Vector2(0, offset);

            offset -= (newAnswer.Rect.sizeDelta.y + parameters.Margins);
            uiElements.AnswerContentArea.sizeDelta = new Vector2(uiElements.AnswerContentArea.sizeDelta.x, offset * -1);

            currentAnswer.Add(newAnswer);
        }
    }

    void EraseAnswers()
    {
        foreach (var answer in currentAnswer)
        {
            Destroy (answer.gameObject);
        }
        currentAnswer.Clear ();
    }

    void UpdateScoreUI()
    {
        uiElements.ScoreText.text = "Score: " + events.CurrentFinalScore;
    }

    // ==================== additional ==================
    public void ShowExplanation(Question question, string status)
    {
        if (status == "show")
        {
            uiElements.Explanation.text = question.Explanation;
            ShowHideNextButton("show");
        }
        else
        {
            uiElements.Explanation.text = string.Empty;
        }
    }

    public void ShowHideNextButton(string status)
    {
        if (status == "show")
        {
            uiElements.NextButton.SetActive(true);
        }
        else
        {
            uiElements.NextButton.SetActive(false);
        }
    }
    // ==================== end of additional ==================
}
