using UnityEngine;
using System.Collections;

/// <summary>
/// A simple light-activated switch/button
/// Perfect for your puzzle mechanics
/// </summary>
public class LightActivatedSwitch : MonoBehaviour
{
    [Header("Switch Settings")]
    [SerializeField] private bool _requiresContinuousLight = true;
    [SerializeField] private float _activationDelay = 0f;

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer _indicator;
    [SerializeField] private Color _inactiveColor = Color.red;
    [SerializeField] private Color _activeColor = Color.green;

    public bool IsIlluminated { get; private set; }
    public bool IsActivated { get; private set; }

    // Events
    public System.Action<LightActivatedSwitch> OnSwitchActivated;
    public System.Action<LightActivatedSwitch> OnSwitchDeactivated;

    private Coroutine _activationCoroutine;

    private void Start()
    {
        UpdateVisuals();
    }

    public void OnIlluminated(LanternController lantern)
    {
        if (IsIlluminated) return;

        IsIlluminated = true;

        if (_activationCoroutine != null)
            StopCoroutine(_activationCoroutine);

        _activationCoroutine = StartCoroutine(ActivationDelayRoutine());
    }

    public void OnLeftLight(LanternController lantern)
    {
        if (!IsIlluminated) return;

        IsIlluminated = false;

        if (_activationCoroutine != null)
        {
            StopCoroutine(_activationCoroutine);
            _activationCoroutine = null;
        }

        if (_requiresContinuousLight && IsActivated)
        {
            DeactivateSwitch();
        }
    }

    private IEnumerator ActivationDelayRoutine()
    {
        if (_activationDelay > 0f)
            yield return new WaitForSeconds(_activationDelay);

        if (IsIlluminated && !IsActivated)
        {
            ActivateSwitch();
        }
    }

    private void ActivateSwitch()
    {
        IsActivated = true;
        UpdateVisuals();
        OnSwitchActivated?.Invoke(this);
    }

    private void DeactivateSwitch()
    {
        IsActivated = false;
        UpdateVisuals();
        OnSwitchDeactivated?.Invoke(this);
    }

    private void UpdateVisuals()
    {
        if (_indicator != null)
        {
            _indicator.color = IsActivated ? _activeColor : _inactiveColor;
        }
    }
}