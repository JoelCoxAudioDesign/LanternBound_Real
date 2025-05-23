using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LanternShrine : MonoBehaviour, ILightInteractable
{
    [Header("Shrine Settings")]
    [SerializeField] private float _activationTime = 2f;
    [SerializeField] private bool _requiresContinuousLight = true;
    [SerializeField] private bool _staysLitOnceActivated = false;

    [Header("Visual Elements")]
    [SerializeField] private Light _shrineLight;
    [SerializeField] private ParticleSystem _activationParticles;
    [SerializeField] private SpriteRenderer _shrineGlow;
    [SerializeField] private Color _inactiveColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
    [SerializeField] private Color _activatingColor = Color.yellow;
    [SerializeField] private Color _activeColor = Color.white;

    [Header("Audio")]
    [SerializeField] private AudioClip _activationSound;
    [SerializeField] private AudioClip _chargingSound;

    public bool IsIlluminated { get; private set; }
    public bool IsActivated { get; private set; }
    public float ActivationProgress { get; private set; }

    public System.Action OnShrineActivated;
    public System.Action OnShrineDeactivated;

    private AudioSource _audioSource;
    private Coroutine _activationCoroutine;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        SetupVisuals();
    }

    private void SetupVisuals()
    {
        if (_shrineLight != null)
        {
            _shrineLight.enabled = false;
        }

        if (_shrineGlow != null)
        {
            _shrineGlow.color = _inactiveColor;
        }

        if (_activationParticles != null)
        {
            _activationParticles.Stop();
        }
    }

    public void OnIlluminated(LanternController lantern)
    {
        if (IsIlluminated || (_staysLitOnceActivated && IsActivated)) return;

        IsIlluminated = true;

        if (_activationCoroutine != null)
            StopCoroutine(_activationCoroutine);

        _activationCoroutine = StartCoroutine(ActivationSequence());
    }

    public void OnLeftLight(LanternController lantern)
    {
        if (!IsIlluminated || (_staysLitOnceActivated && IsActivated)) return;

        IsIlluminated = false;

        if (_activationCoroutine != null)
        {
            StopCoroutine(_activationCoroutine);
            _activationCoroutine = null;
        }

        if (_requiresContinuousLight && IsActivated)
        {
            DeactivateShrine();
        }
        else if (!IsActivated)
        {
            // Reset charging progress
            StartCoroutine(ResetActivationProgress());
        }
    }

    private IEnumerator ActivationSequence()
    {
        // Play charging sound
        if (_audioSource != null && _chargingSound != null)
        {
            _audioSource.clip = _chargingSound;
            _audioSource.loop = true;
            _audioSource.Play();
        }

        // Start particles
        if (_activationParticles != null)
            _activationParticles.Play();

        float elapsedTime = ActivationProgress * _activationTime;

        while (elapsedTime < _activationTime && IsIlluminated)
        {
            elapsedTime += Time.deltaTime;
            ActivationProgress = elapsedTime / _activationTime;

            // Update visual feedback
            UpdateChargingVisuals();

            yield return null;
        }

        if (IsIlluminated && ActivationProgress >= 1f)
        {
            ActivateShrine();
        }

        // Stop charging sound
        if (_audioSource != null && _audioSource.isPlaying)
        {
            _audioSource.Stop();
            _audioSource.loop = false;
        }
    }

    private IEnumerator ResetActivationProgress()
    {
        float startProgress = ActivationProgress;
        float resetTime = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < resetTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / resetTime;
            ActivationProgress = Mathf.Lerp(startProgress, 0f, t);
            UpdateChargingVisuals();
            yield return null;
        }

        ActivationProgress = 0f;
        if (_shrineGlow != null)
            _shrineGlow.color = _inactiveColor;
    }

    private void UpdateChargingVisuals()
    {
        if (_shrineGlow != null)
        {
            Color currentColor = Color.Lerp(_inactiveColor, _activatingColor, ActivationProgress);
            _shrineGlow.color = currentColor;
        }
    }

    private void ActivateShrine()
    {
        IsActivated = true;
        ActivationProgress = 1f;

        // Update visuals
        if (_shrineLight != null)
        {
            _shrineLight.enabled = true;
            _shrineLight.color = _activeColor;
        }

        if (_shrineGlow != null)
        {
            _shrineGlow.color = _activeColor;
        }

        // Play activation sound
        if (_audioSource != null && _activationSound != null)
        {
            _audioSource.PlayOneShot(_activationSound);
        }

        OnShrineActivated?.Invoke();

        Debug.Log($"Shrine {gameObject.name} activated! Light restored.");
    }

    private void DeactivateShrine()
    {
        IsActivated = false;

        // Update visuals
        if (_shrineLight != null)
        {
            _shrineLight.enabled = false;
        }

        OnShrineDeactivated?.Invoke();

        StartCoroutine(ResetActivationProgress());
    }
}