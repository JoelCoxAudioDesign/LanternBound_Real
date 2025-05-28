using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages the opening area tutorial sequence and puzzles
/// Guides player through discovering and learning lantern mechanics
/// </summary>
public class OpeningAreaManager : MonoBehaviour
{
    [Header("Tutorial Sequence")]
    [SerializeField] private TutorialStep[] _tutorialSteps;
    [SerializeField] private float _stepCompletionDelay = 1f;

    [Header("World State")]
    [SerializeField] private Color _darkenedWorldColor = new Color(0.3f, 0.3f, 0.4f, 1f);
    [SerializeField] private Color _illuminatedWorldColor = Color.white;
    [SerializeField] private float _worldLightTransitionSpeed = 2f;

    private int _currentStepIndex = 0;
    private LanternController _playerLantern;
    private Camera _mainCamera;

    // Events for UI/narrative system
    public System.Action<string> OnTutorialTextChanged;
    public System.Action<int> OnStepCompleted;
    public System.Action OnTutorialCompleted;

    [System.Serializable]
    public class TutorialStep
    {
        public string stepName;
        [TextArea(3, 5)]
        public string tutorialText;
        public TutorialStepType stepType;
        public GameObject[] objectsToActivate;
        public GameObject[] objectsToDeactivate;
        public bool waitForPlayerAction = true;
        public float autoCompleteDelay = 0f; // For steps that auto-complete
    }

    public enum TutorialStepType
    {
        Introduction,
        LanternDiscovery,
        FirstIllumination,
        RevealHiddenPath,
        EnemyEncounter,
        PuzzleSolving,
        WorldOpening
    }

    private void Awake()
    {
        _playerLantern = FindFirstObjectByType<LanternController>();
        _mainCamera = Camera.main;

        // Start with darkened world
        if (_mainCamera != null)
        {
            _mainCamera.backgroundColor = _darkenedWorldColor;
        }
    }

    private void Start()
    {
        StartTutorial();
    }

    public void StartTutorial()
    {
        _currentStepIndex = 0;
        ExecuteCurrentStep();
    }

    private void ExecuteCurrentStep()
    {
        if (_currentStepIndex >= _tutorialSteps.Length)
        {
            CompleteTutorial();
            return;
        }

        TutorialStep currentStep = _tutorialSteps[_currentStepIndex];

        // Update tutorial text
        OnTutorialTextChanged?.Invoke(currentStep.tutorialText);

        // Handle object activation/deactivation
        foreach (GameObject obj in currentStep.objectsToActivate)
        {
            if (obj != null) obj.SetActive(true);
        }

        foreach (GameObject obj in currentStep.objectsToDeactivate)
        {
            if (obj != null) obj.SetActive(false);
        }

        // Handle step-specific logic
        switch (currentStep.stepType)
        {
            case TutorialStepType.LanternDiscovery:
                HandleLanternDiscovery();
                break;
            case TutorialStepType.FirstIllumination:
                HandleFirstIllumination();
                break;
            case TutorialStepType.WorldOpening:
                HandleWorldOpening();
                break;
        }

        // Auto-complete if specified
        if (!currentStep.waitForPlayerAction || currentStep.autoCompleteDelay > 0f)
        {
            StartCoroutine(AutoCompleteStep(currentStep.autoCompleteDelay));
        }
    }

    private void HandleLanternDiscovery()
    {
        // When player gets near the lantern, activate it
        if (_playerLantern != null)
        {
            _playerLantern.ActivateLantern();
        }
    }

    private void HandleFirstIllumination()
    {
        // Start gradually brightening the world as player uses lantern
        StartCoroutine(GradualWorldIllumination());
    }

    private void HandleWorldOpening()
    {
        // Final step - world becomes fully illuminated
        StartCoroutine(FullWorldIllumination());
    }

    private IEnumerator AutoCompleteStep(float delay)
    {
        yield return new WaitForSeconds(delay);
        CompleteCurrentStep();
    }

    private IEnumerator GradualWorldIllumination()
    {
        Color startColor = _mainCamera.backgroundColor;
        Color targetColor = Color.Lerp(_darkenedWorldColor, _illuminatedWorldColor, 0.5f);

        float elapsedTime = 0f;
        float duration = 3f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            _mainCamera.backgroundColor = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        _mainCamera.backgroundColor = targetColor;
    }

    private IEnumerator FullWorldIllumination()
    {
        Color startColor = _mainCamera.backgroundColor;

        float elapsedTime = 0f;
        float duration = 5f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            _mainCamera.backgroundColor = Color.Lerp(startColor, _illuminatedWorldColor, t);
            yield return null;
        }

        _mainCamera.backgroundColor = _illuminatedWorldColor;
    }

    public void CompleteCurrentStep()
    {
        if (_currentStepIndex < _tutorialSteps.Length)
        {
            OnStepCompleted?.Invoke(_currentStepIndex);
            _currentStepIndex++;

            StartCoroutine(DelayedStepExecution());
        }
    }

    private IEnumerator DelayedStepExecution()
    {
        yield return new WaitForSeconds(_stepCompletionDelay);
        ExecuteCurrentStep();
    }

    private void CompleteTutorial()
    {
        OnTutorialCompleted?.Invoke();
        Debug.Log("Tutorial completed! Player is ready to explore the world.");
    }

    // Call this from other systems when conditions are met
    public void TriggerStepCompletion(TutorialStepType stepType)
    {
        if (_currentStepIndex < _tutorialSteps.Length &&
            _tutorialSteps[_currentStepIndex].stepType == stepType)
        {
            CompleteCurrentStep();
        }
    }
}