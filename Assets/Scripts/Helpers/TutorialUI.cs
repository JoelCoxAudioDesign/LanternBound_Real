using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Handles UI display for tutorial messages and guidance
/// Creates atmospheric, minimal UI that fits the light/dark theme
/// </summary>
public class TutorialUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject _tutorialPanel;
    [SerializeField] private TextMeshProUGUI _tutorialText;
    [SerializeField] private Image _progressBar;
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Animation Settings")]
    [SerializeField] private float _fadeInSpeed = 2f;
    [SerializeField] private float _fadeOutSpeed = 3f;
    [SerializeField] private float _textRevealSpeed = 30f; // Characters per second
    [SerializeField] private float _autoHideDelay = 5f;

    [Header("Visual Theme")]
    [SerializeField] private Color _normalTextColor = Color.white;
    [SerializeField] private Color _importantTextColor = Color.yellow;
    [SerializeField] private Color _progressBarColor = Color.cyan;

    private bool _isVisible = false;
    private Coroutine _currentAnimation;
    private Coroutine _autoHideCoroutine;
    private OpeningAreaManager _tutorialManager;

    private void Awake()
    {
        _tutorialManager = FindFirstObjectByType<OpeningAreaManager>();

        if (_tutorialManager != null)
        {
            _tutorialManager.OnTutorialTextChanged += DisplayTutorialText;
            _tutorialManager.OnStepCompleted += OnStepCompleted;
            _tutorialManager.OnTutorialCompleted += HideTutorialUI;
        }

        SetupUI();
    }

    private void SetupUI()
    {
        if (_canvasGroup == null)
            _canvasGroup = GetComponent<CanvasGroup>();

        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Start hidden
        _canvasGroup.alpha = 0f;

        // Only try to deactivate panel if it exists
        if (_tutorialPanel != null)
            _tutorialPanel.SetActive(false);

        // Setup progress bar if it exists
        if (_progressBar != null)
        {
            _progressBar.color = _progressBarColor;
            _progressBar.fillAmount = 0f;
        }
    }

    public void DisplayTutorialText(string text)
    {
        if (_currentAnimation != null)
            StopCoroutine(_currentAnimation);

        if (_autoHideCoroutine != null)
            StopCoroutine(_autoHideCoroutine);

        _currentAnimation = StartCoroutine(ShowTutorialTextAnimated(text));
    }

    private IEnumerator ShowTutorialTextAnimated(string text)
    {
        // Show panel if hidden and if it exists
        if (!_isVisible)
        {
            if (_tutorialPanel != null)
                _tutorialPanel.SetActive(true);
            yield return StartCoroutine(FadeIn());
        }

        // Clear current text if text component exists
        if (_tutorialText != null)
            _tutorialText.text = "";

        // Animate text reveal if text component exists
        if (_tutorialText != null)
            yield return StartCoroutine(RevealText(text));
        else
            Debug.Log($"Tutorial Text: {text}"); // Fallback to console if no UI

        // Auto-hide after delay
        _autoHideCoroutine = StartCoroutine(AutoHideAfterDelay());
    }

    private IEnumerator FadeIn()
    {
        _isVisible = true;
        float elapsedTime = 0f;
        float startAlpha = _canvasGroup.alpha;

        while (elapsedTime < 1f / _fadeInSpeed)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime * _fadeInSpeed;
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, t);
            yield return null;
        }

        _canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        float startAlpha = _canvasGroup.alpha;

        while (elapsedTime < 1f / _fadeOutSpeed)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime * _fadeOutSpeed;
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            yield return null;
        }

        _canvasGroup.alpha = 0f;

        // Clear the text when hiding (if text component exists)
        if (_tutorialText != null)
            _tutorialText.text = "";

        // Hide panel if it exists
        if (_tutorialPanel != null)
            _tutorialPanel.SetActive(false);
        _isVisible = false;
    }

    private IEnumerator RevealText(string fullText)
    {
        if (_tutorialText == null) yield break;

        // Process formatting ONCE before revealing
        string processedText = ProcessTextFormatting(fullText);

        // For reveal animation, we need to work with the raw text
        string rawText = fullText; // Use original text for character counting
        int charactersRevealed = 0;
        float timeBetweenChars = 1f / _textRevealSpeed;

        while (charactersRevealed < rawText.Length)
        {
            yield return new WaitForSeconds(timeBetweenChars);
            charactersRevealed++;

            // Apply formatting to the substring
            string currentSubstring = rawText.Substring(0, charactersRevealed);
            _tutorialText.text = ProcessTextFormatting(currentSubstring);
        }

        // Ensure final text is fully formatted
        _tutorialText.text = processedText;
    }

    private string ProcessTextFormatting(string text)
    {
        // Simple formatting - wrap important words in color tags
        text = text.Replace("[LANTERN]", $"<color=#{ColorUtility.ToHtmlStringRGB(_importantTextColor)}>LANTERN</color>");
        text = text.Replace("[LIGHT]", $"<color=#{ColorUtility.ToHtmlStringRGB(_importantTextColor)}>LIGHT</color>");
        text = text.Replace("[SHRINE]", $"<color=#{ColorUtility.ToHtmlStringRGB(_importantTextColor)}>SHRINE</color>");

        return text;
    }

    private IEnumerator AutoHideAfterDelay()
    {
        yield return new WaitForSeconds(_autoHideDelay);

        if (_currentAnimation != null)
            StopCoroutine(_currentAnimation);

        _currentAnimation = StartCoroutine(FadeOut());
    }

    public void UpdateProgress(float progress)
    {
        if (_progressBar != null)
        {
            _progressBar.fillAmount = progress;
        }
    }

    private void OnStepCompleted(int stepIndex)
    {
        // Update progress bar based on completed steps
        if (_tutorialManager != null)
        {
            // Note: This assumes _tutorialSteps is accessible - you might need to adjust this
            float progress = (float)(stepIndex + 1) / 6f; // Assuming 6 tutorial steps
            UpdateProgress(progress);
        }
    }

    public void HideTutorialUI()
    {
        if (_currentAnimation != null)
            StopCoroutine(_currentAnimation);

        if (_autoHideCoroutine != null)
            StopCoroutine(_autoHideCoroutine);

        _currentAnimation = StartCoroutine(FadeOut());
    }

    public void ShowInstantMessage(string message, float duration = 3f)
    {
        StartCoroutine(ShowInstantMessageCoroutine(message, duration));
    }

    private IEnumerator ShowInstantMessageCoroutine(string message, float duration)
    {
        if (_tutorialText != null)
        {
            string originalText = _tutorialText.text;
            _tutorialText.text = ProcessTextFormatting(message);

            yield return new WaitForSeconds(duration);

            _tutorialText.text = originalText;
        }
    }

    private void OnDestroy()
    {
        if (_tutorialManager != null)
        {
            _tutorialManager.OnTutorialTextChanged -= DisplayTutorialText;
            _tutorialManager.OnStepCompleted -= OnStepCompleted;
            _tutorialManager.OnTutorialCompleted -= HideTutorialUI;
        }
    }
}