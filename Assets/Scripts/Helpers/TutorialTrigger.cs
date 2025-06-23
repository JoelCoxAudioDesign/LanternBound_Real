using UnityEngine;

/// <summary>
/// Simple trigger system for advancing tutorial steps
/// Place these in your level to detect when player reaches certain areas
/// </summary>
public class TutorialTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [SerializeField] private OpeningAreaManager.TutorialStepType _stepToTrigger;
    [SerializeField] private bool _triggerOnce = true;
    [SerializeField] private string _customMessage = "";
    [SerializeField] private float _messageDuration = 3f;

    private bool _hasTriggered = false;
    private OpeningAreaManager _tutorialManager;
    private TutorialUI _tutorialUI;

    private void Awake()
    {
        _tutorialManager = FindFirstObjectByType<OpeningAreaManager>();
        _tutorialUI = FindFirstObjectByType<TutorialUI>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasTriggered && _triggerOnce) return;

        // Check if it's the player
        if (other.CompareTag("Player"))
        {
            TriggerStep();
        }
    }

    public void TriggerStep()
    {
        if (_hasTriggered && _triggerOnce) return;

        _hasTriggered = true;

        // Show custom message if provided
        if (!string.IsNullOrEmpty(_customMessage) && _tutorialUI != null)
        {
            _tutorialUI.ShowInstantMessage(_customMessage, _messageDuration);
        }

        // Trigger tutorial step completion
        if (_tutorialManager != null)
        {
            _tutorialManager.TriggerStepCompletion(_stepToTrigger);
        }
    }
}

// ================== RevealablePlatform.cs ==================
