using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class QuestionManager : MonoBehaviour
{
    [Header("Answer Images")]
    public Image[] answerImages; // Assign 4 answer images in Inspector
    public int correctAnswerIndex; // 0-3, assign in Inspector

    [Header("Events")]
    public UnityEvent OnGameFinishedWin; // Event to invoke when game finishes Winning
    public UnityEvent OnGameFinishedLose; // Event to invoke when game finishes Lossing
    private void Start()
    {
        ShuffleAnswers();

        // Assign click listeners to each answer button
        for (int i = 0; i < answerImages.Length; i++)
        {
            int idx = i; // Capture index for the lambda
            if (answerImages[i] != null)
            {
                Button btn = answerImages[i].GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => CheckAnswer(idx));
                }
                else
                {
                    Debug.LogWarning($"Answer image at index {i} does not have a Button component.");
                }
            }
        }

        // Hide all at start
        foreach (var img in answerImages)
            if (img != null) img.gameObject.SetActive(false);

        // Animate in after a short delay
        Invoke(nameof(AnimateAnswersIn), 2f);
    }

    // Shuffle the answerImages array and update correctAnswerIndex and hierarchy
    private void ShuffleAnswers()
    {
        // Store the reference to the correct answer image before shuffling
        Image correctImage = answerImages[correctAnswerIndex];

        // Fisher-Yates shuffle
        for (int i = answerImages.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = answerImages[i];
            answerImages[i] = answerImages[j];
            answerImages[j] = temp;
        }

        // Find the new index of the correct answer image
        for (int i = 0; i < answerImages.Length; i++)
        {
            if (answerImages[i] == correctImage)
            {
                correctAnswerIndex = i;
                break;
            }
        }

        // Update the sibling index in the hierarchy for visual shuffle
        for (int i = 0; i < answerImages.Length; i++)
        {
            answerImages[i].transform.SetSiblingIndex(i);
        }
    }

    private void AnimateAnswersIn()
    {
        float delayStep = 0.5f; // Time between each button's animation

        for (int i = 0; i < answerImages.Length; i++)
        {
            var img = answerImages[i];
            if (img != null)
            {
                img.gameObject.SetActive(true);
                img.transform.localScale = Vector3.zero; // Start hidden (scaled down)
                img.transform.DOScale(Vector3.one, 0.4f)
                    .SetDelay(i * delayStep)
                    .SetEase(Ease.OutBack);
            }
        }
    }

    // Public method to check and mark the answer
    public void CheckAnswer(int selectedIndex)
    {
        // Hide all right/wrong indicators first
        foreach (var img in answerImages)
        {
            if (img != null)
            {
                var marker = img.GetComponent<AnswerMarker>();
                if (marker != null)
                    marker.HideAll();
            }
        }

        if (selectedIndex == correctAnswerIndex)
        {
            // Mark only the correct answer as right
            var marker = answerImages[correctAnswerIndex].GetComponent<AnswerMarker>();
            if (marker != null)
                marker.ShowRight();
            //OnGameFinishedWin?.Invoke();
            ScanningFileHandler.Instance.result = "Win";
            ScanningFileHandler.Instance.FileSave();
            StartCoroutine(WinGameCoroutine());
        }
        else
        {
            // Mark selected as wrong, and correct as right
            var wrongMarker = answerImages[selectedIndex].GetComponent<AnswerMarker>();
            var rightMarker = answerImages[correctAnswerIndex].GetComponent<AnswerMarker>();
            if (wrongMarker != null)
                wrongMarker.ShowWrong();
            if (rightMarker != null)
                rightMarker.ShowRight();
            ScanningFileHandler.Instance.result = "Lose";
            ScanningFileHandler.Instance.FileSave();
            OnGameFinishedLose?.Invoke();
        }
    }

        
    
    private IEnumerator WinGameCoroutine()
    {
        // Wait for a short duration before invoking the win event
        yield return new WaitForSeconds(5f);
        OnGameFinishedWin?.Invoke();
    }

}
