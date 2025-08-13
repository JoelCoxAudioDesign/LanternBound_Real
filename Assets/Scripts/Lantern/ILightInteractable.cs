using UnityEngine;
using System.Collections;

/// <summary>
/// Enhanced interface for objects that can interact with the new lantern system
/// Now supports different light types and more complex interactions
/// </summary>
public interface ILightInteractable
{
    /// <summary>
    /// Called when this object enters the lantern's light beam
    /// </summary>
    /// <param name="lantern">The enhanced lantern controller illuminating this object</param>
    void OnIlluminated(EnhancedLanternController lantern);

    /// <summary>
    /// Called when this object leaves the lantern's light beam
    /// </summary>
    /// <param name="lantern">The enhanced lantern controller that was illuminating this object</param>
    void OnLeftLight(EnhancedLanternController lantern);

    /// <summary>
    /// Whether this object is currently being illuminated
    /// </summary>
    bool IsIlluminated { get; }

    /// <summary>
    /// Whether this object can be affected by the current light type
    /// </summary>
    /// <param name="lightType">The light type to check</param>
    /// <returns>True if this object responds to the given light type</returns>
    bool RespondsToLightType(EnhancedLanternController.LightType lightType);
}

/// <summary>
/// A platform that becomes visible/solid when illuminated by specific light types
/// Perfect for teaching light type mechanics
/// </summary>
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class EnhancedRevealablePlatform : MonoBehaviour, ILightInteractable
{
    [Header("Light Type Requirements")]
    [SerializeField]
    private EnhancedLanternController.LightType[] _requiredLightTypes =
        { EnhancedLanternController.LightType.Ember, EnhancedLanternController.LightType.Radiance };
    [SerializeField] private bool _requiresSpecificLightType = false;

    [Header("Reveal Settings")]
    [SerializeField] private bool _startVisible = false;
    [SerializeField] private float _fadeSpeed = 2f;
    [SerializeField] private bool _solidWhenVisible = true;
    [SerializeField] private bool _stayRevealedOnceFound = false;

    [Header("Visual Settings")]
    [SerializeField] private Color _visibleColor = Color.white;
    [SerializeField] private Color _hiddenColor = new Color(1f, 1f, 1f, 0.1f);
    [SerializeField] private Color _wrongLightTypeColor = new Color(1f, 0.5f, 0.5f, 0.3f);

    [Header("Effects")]
    [SerializeField] private ParticleSystem _revealEffect;
    [SerializeField] private AudioClip _revealSound;
    [SerializeField] private AudioClip _hideSound;

    public bool IsIlluminated { get; private set; }
    public bool IsRevealed { get; private set; }

    private SpriteRenderer _spriteRenderer;
    private Collider2D _platformCollider;
    private AudioSource _audioSource;
    private Coroutine _fadeCoroutine;
    private bool _permanentlyRevealed = false;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _platformCollider = GetComponent<Collider2D>();
        _audioSource = GetComponent<AudioSource>();

        // Set initial state
        if (_startVisible)
        {
            RevealPlatform(true);
        }
        else
        {
            HidePlatform(true);
        }
    }

    public void OnIlluminated(EnhancedLanternController lantern)
    {
        if (IsIlluminated || _permanentlyRevealed) return;

        IsIlluminated = true;

        // Check if the current light type can reveal this platform
        if (RespondsToLightType(lantern.CurrentLightType))
        {
            RevealPlatform();
        }
        else if (_requiresSpecificLightType)
        {
            // Show hint that wrong light type is being used
            ShowWrongLightTypeHint();
        }
    }

    public void OnLeftLight(EnhancedLanternController lantern)
    {
        if (!IsIlluminated || _permanentlyRevealed) return;

        IsIlluminated = false;

        if (!_stayRevealedOnceFound)
        {
            HidePlatform();
        }
    }

    public bool RespondsToLightType(EnhancedLanternController.LightType lightType)
    {
        if (!_requiresSpecificLightType) return true;

        foreach (var requiredType in _requiredLightTypes)
        {
            if (lightType == requiredType)
                return true;
        }

        return false;
    }

    private void RevealPlatform(bool instant = false)
    {
        if (IsRevealed) return;

        IsRevealed = true;

        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        if (instant)
        {
            _spriteRenderer.color = _visibleColor;
            if (_solidWhenVisible)
                _platformCollider.enabled = true;
        }
        else
        {
            _fadeCoroutine = StartCoroutine(FadeToColor(_visibleColor));

            if (_solidWhenVisible)
                _platformCollider.enabled = true;

            PlayRevealEffects();
        }

        if (_stayRevealedOnceFound)
        {
            _permanentlyRevealed = true;
        }
    }

    private void HidePlatform(bool instant = false)
    {
        if (!IsRevealed) return;

        IsRevealed = false;

        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        if (instant)
        {
            _spriteRenderer.color = _hiddenColor;
            if (_solidWhenVisible)
                _platformCollider.enabled = false;
        }
        else
        {
            _fadeCoroutine = StartCoroutine(FadeToColor(_hiddenColor));

            if (_solidWhenVisible)
                _platformCollider.enabled = false;

            PlayHideEffects();
        }
    }

    private void ShowWrongLightTypeHint()
    {
        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        _fadeCoroutine = StartCoroutine(FlashWrongLightType());
    }

    private IEnumerator FadeToColor(Color targetColor)
    {
        Color startColor = _spriteRenderer.color;
        float elapsedTime = 0f;

        while (elapsedTime < 1f / _fadeSpeed)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime * _fadeSpeed;
            _spriteRenderer.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        _spriteRenderer.color = targetColor;
    }

    private IEnumerator FlashWrongLightType()
    {
        Color originalColor = _spriteRenderer.color;

        // Flash to wrong light type color
        yield return StartCoroutine(FadeToColor(_wrongLightTypeColor));
        yield return new WaitForSeconds(0.5f);

        // Fade back to original
        yield return StartCoroutine(FadeToColor(originalColor));
    }

    private void PlayRevealEffects()
    {
        if (_revealEffect != null)
            _revealEffect.Play();

        if (_audioSource != null && _revealSound != null)
            _audioSource.PlayOneShot(_revealSound);
    }

    private void PlayHideEffects()
    {
        if (_audioSource != null && _hideSound != null)
            _audioSource.PlayOneShot(_hideSound);
    }
}

/// <summary>
/// An enemy that reacts differently to different light types
/// Perfect for teaching light type combat and interaction
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnhancedLightSensitiveEnemy : MonoBehaviour, ILightInteractable
{
    [Header("Light Type Reactions")]
    [SerializeField] private LightReactionData[] _lightReactions;
    [SerializeField] private LightReactionData _defaultReaction;

    [Header("Movement Settings")]
    [SerializeField] private float _normalSpeed = 2f;
    [SerializeField] private Transform[] _patrolPoints;
    [SerializeField] private bool _patrolWhenNotIlluminated = true;

    [Header("Health System")]
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private bool _canBeDamaged = true;

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private ParticleSystem _reactionEffect;

    public bool IsIlluminated { get; private set; }
    public float CurrentHealth { get; private set; }

    [System.Serializable]
    public class LightReactionData
    {
        public EnhancedLanternController.LightType lightType;
        public EnemyReaction reaction = EnemyReaction.Retreat;
        public float reactionSpeed = 5f;
        public float damage = 0f;
        public float stunDuration = 0f;
        public Color reactionColor = Color.white;
        public AudioClip reactionSound;
    }

    public enum EnemyReaction
    {
        None,           // No reaction
        Retreat,        // Moves away from light
        Freeze,         // Stops moving
        Dissolve,       // Becomes transparent/non-solid
        TakeDamage,     // Receives damage over time
        Enrage,         // Becomes faster/more aggressive
        Transform       // Changes into different enemy type
    }

    private Rigidbody2D _rb;
    private AudioSource _audioSource;
    private Vector2 _retreatDirection;
    private int _currentPatrolIndex = 0;
    private bool _isStunned = false;
    private bool _isReacting = false;
    private Coroutine _reactionCoroutine;
    private EnhancedLanternController.LightType _currentAffectingLightType;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _audioSource = GetComponent<AudioSource>();
        _spriteRenderer = _spriteRenderer ?? GetComponent<SpriteRenderer>();

        CurrentHealth = _maxHealth;
    }

    private void Update()
    {
        if (!IsIlluminated && !_isStunned && _patrolWhenNotIlluminated)
        {
            Patrol();
        }
    }

    public void OnIlluminated(EnhancedLanternController lantern)
    {
        if (IsIlluminated) return;

        IsIlluminated = true;
        _currentAffectingLightType = lantern.CurrentLightType;

        // Calculate retreat direction
        _retreatDirection = ((Vector2)transform.position - (Vector2)lantern.transform.position).normalized;

        // Get reaction for this light type
        var reaction = GetReactionForLightType(lantern.CurrentLightType);

        StartReaction(reaction);
    }

    public void OnLeftLight(EnhancedLanternController lantern)
    {
        if (!IsIlluminated) return;

        IsIlluminated = false;
        StopReaction();
    }

    public bool RespondsToLightType(EnhancedLanternController.LightType lightType)
    {
        // This enemy responds to all light types, but with different reactions
        return true;
    }

    private LightReactionData GetReactionForLightType(EnhancedLanternController.LightType lightType)
    {
        foreach (var reaction in _lightReactions)
        {
            if (reaction.lightType == lightType)
                return reaction;
        }

        return _defaultReaction;
    }

    private void StartReaction(LightReactionData reactionData)
    {
        if (_reactionCoroutine != null)
            StopCoroutine(_reactionCoroutine);

        _isReacting = true;

        switch (reactionData.reaction)
        {
            case EnemyReaction.Retreat:
                StartRetreating(reactionData);
                break;
            case EnemyReaction.Freeze:
                StartFreezing(reactionData);
                break;
            case EnemyReaction.Dissolve:
                StartDissolving(reactionData);
                break;
            case EnemyReaction.TakeDamage:
                StartTakingDamage(reactionData);
                break;
            case EnemyReaction.Enrage:
                StartEnraging(reactionData);
                break;
        }

        // Play visual and audio feedback
        ApplyReactionEffects(reactionData);
    }

    private void StopReaction()
    {
        if (_reactionCoroutine != null)
        {
            StopCoroutine(_reactionCoroutine);
            _reactionCoroutine = null;
        }

        _isReacting = false;
        _rb.linearVelocity = Vector2.zero;

        // Reset visual state
        if (_spriteRenderer != null)
            _spriteRenderer.color = _normalColor;
    }

    private void StartRetreating(LightReactionData reactionData)
    {
        _rb.linearVelocity = _retreatDirection * reactionData.reactionSpeed;
    }

    private void StartFreezing(LightReactionData reactionData)
    {
        _rb.linearVelocity = Vector2.zero;

        if (reactionData.stunDuration > 0f)
        {
            _reactionCoroutine = StartCoroutine(StunCoroutine(reactionData.stunDuration));
        }
    }

    private void StartDissolving(LightReactionData reactionData)
    {
        _reactionCoroutine = StartCoroutine(DissolveCoroutine(reactionData));
    }

    private void StartTakingDamage(LightReactionData reactionData)
    {
        if (_canBeDamaged)
        {
            _reactionCoroutine = StartCoroutine(DamageOverTimeCoroutine(reactionData));
        }
    }

    private void StartEnraging(LightReactionData reactionData)
    {
        // Make enemy move faster and more erratically
        Vector2 randomDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        _rb.linearVelocity = randomDirection * reactionData.reactionSpeed;
    }

    private IEnumerator StunCoroutine(float stunDuration)
    {
        _isStunned = true;
        yield return new WaitForSeconds(stunDuration);
        _isStunned = false;
    }

    private IEnumerator DissolveCoroutine(LightReactionData reactionData)
    {
        Color startColor = _spriteRenderer.color;
        Color transparentColor = new Color(startColor.r, startColor.g, startColor.b, 0.1f);

        float fadeTime = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeTime && IsIlluminated)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeTime;
            _spriteRenderer.color = Color.Lerp(startColor, transparentColor, t);
            yield return null;
        }

        // Make enemy non-solid while dissolved
        GetComponent<Collider2D>().enabled = false;

        // Wait for light to leave, then restore
        yield return new WaitUntil(() => !IsIlluminated);

        GetComponent<Collider2D>().enabled = true;
        _spriteRenderer.color = startColor;
    }

    private IEnumerator DamageOverTimeCoroutine(LightReactionData reactionData)
    {
        while (IsIlluminated && CurrentHealth > 0)
        {
            TakeDamage(reactionData.damage * Time.deltaTime);
            yield return null;
        }
    }

    private void TakeDamage(float damage)
    {
        CurrentHealth = Mathf.Max(CurrentHealth - damage, 0f);

        if (CurrentHealth <= 0f)
        {
            DestroyEnemy();
        }
    }

    private void DestroyEnemy()
    {
        // Play death effect
        if (_reactionEffect != null)
            _reactionEffect.Play();

        Debug.Log($"Enemy destroyed by {_currentAffectingLightType} light!");

        // Destroy after brief delay for effects
        Destroy(gameObject, 0.5f);
    }

    private void ApplyReactionEffects(LightReactionData reactionData)
    {
        // Visual feedback
        if (_spriteRenderer != null)
            _spriteRenderer.color = reactionData.reactionColor;

        // Audio feedback
        if (_audioSource != null && reactionData.reactionSound != null)
            _audioSource.PlayOneShot(reactionData.reactionSound);

        // Particle effect
        if (_reactionEffect != null)
        {
            var main = _reactionEffect.main;
            main.startColor = reactionData.reactionColor;
            _reactionEffect.Play();
        }
    }

    private void Patrol()
    {
        if (_patrolPoints == null || _patrolPoints.Length == 0) return;

        Transform targetPoint = _patrolPoints[_currentPatrolIndex];
        Vector2 direction = (targetPoint.position - transform.position).normalized;

        _rb.linearVelocity = direction * _normalSpeed;

        if (Vector2.Distance(transform.position, targetPoint.position) < 0.5f)
        {
            _currentPatrolIndex = (_currentPatrolIndex + 1) % _patrolPoints.Length;
        }
    }
}

/// <summary>
/// A shrine that requires specific light types to activate
/// Perfect for teaching light type puzzle mechanics
/// </summary>
public class EnhancedLanternShrine : MonoBehaviour, ILightInteractable
{
    [Header("Light Type Requirements")]
    [SerializeField] private EnhancedLanternController.LightType _requiredLightType = EnhancedLanternController.LightType.Radiance;
    [SerializeField] private bool _acceptsAnyLightType = false;

    [Header("Activation Settings")]
    [SerializeField] private float _activationTime = 2f;
    [SerializeField] private bool _requiresContinuousLight = true;
    [SerializeField] private bool _staysLitOnceActivated = true;

    [Header("Visual Elements")]
    [SerializeField] private Light _shrineLight;
    [SerializeField] private ParticleSystem _activationParticles;
    [SerializeField] private SpriteRenderer _shrineGlow;
    [SerializeField] private Color _inactiveColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
    [SerializeField] private Color _chargingColor = Color.yellow;
    [SerializeField] private Color _activeColor = Color.white;
    [SerializeField] private Color _wrongLightColor = Color.red;

    [Header("Rewards")]
    [SerializeField] private GameObject _rewardPrefab;
    [SerializeField] private EnhancedLanternController.LightType _grantedLightType = EnhancedLanternController.LightType.None;

    public bool IsIlluminated { get; private set; }
    public bool IsActivated { get; private set; }
    public float ActivationProgress { get; private set; }

    public System.Action OnShrineActivated;
    public System.Action OnShrineDeactivated;

    private AudioSource _audioSource;
    private Coroutine _activationCoroutine;
    private bool _hasBeenActivated = false;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        SetupVisuals();
    }

    private void SetupVisuals()
    {
        if (_shrineLight != null)
            _shrineLight.enabled = false;

        if (_shrineGlow != null)
            _shrineGlow.color = _inactiveColor;

        if (_activationParticles != null)
            _activationParticles.Stop();
    }

    public void OnIlluminated(EnhancedLanternController lantern)
    {
        if (IsIlluminated || (_staysLitOnceActivated && IsActivated)) return;

        IsIlluminated = true;

        if (RespondsToLightType(lantern.CurrentLightType))
        {
            if (_activationCoroutine != null)
                StopCoroutine(_activationCoroutine);

            _activationCoroutine = StartCoroutine(ActivationSequence());
        }
        else
        {
            ShowWrongLightTypeEffect();
        }
    }

    public void OnLeftLight(EnhancedLanternController lantern)
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
            StartCoroutine(ResetActivationProgress());
        }
    }

    public bool RespondsToLightType(EnhancedLanternController.LightType lightType)
    {
        return _acceptsAnyLightType || lightType == _requiredLightType;
    }

    private IEnumerator ActivationSequence()
    {
        // Start charging effects
        if (_activationParticles != null)
            _activationParticles.Play();

        float elapsedTime = ActivationProgress * _activationTime;

        while (elapsedTime < _activationTime && IsIlluminated)
        {
            elapsedTime += Time.deltaTime;
            ActivationProgress = elapsedTime / _activationTime;

            UpdateChargingVisuals();
            yield return null;
        }

        if (IsIlluminated && ActivationProgress >= 1f)
        {
            ActivateShrine();
        }
    }

    private void UpdateChargingVisuals()
    {
        if (_shrineGlow != null)
        {
            Color currentColor = Color.Lerp(_inactiveColor, _chargingColor, ActivationProgress);
            _shrineGlow.color = currentColor;
        }
    }

    private void ActivateShrine()
    {
        IsActivated = true;
        _hasBeenActivated = true;
        ActivationProgress = 1f;

        // Update visuals
        if (_shrineLight != null)
        {
            _shrineLight.enabled = true;
            _shrineLight.color = _activeColor;
        }

        if (_shrineGlow != null)
            _shrineGlow.color = _activeColor;

        // Grant rewards
        GrantRewards();

        OnShrineActivated?.Invoke();
        Debug.Log($"Shrine activated! Light restored.");
    }

    private void GrantRewards()
    {
        // Spawn reward object
        if (_rewardPrefab != null)
        {
            Instantiate(_rewardPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
        }

        // Grant new light type
        if (_grantedLightType != EnhancedLanternController.LightType.None)
        {
            var player = FindFirstObjectByType<EnhancedLanternController>();
            if (player != null)
            {
                player.DiscoverLightType(_grantedLightType);
            }
        }
    }

    private void DeactivateShrine()
    {
        IsActivated = false;

        if (_shrineLight != null)
            _shrineLight.enabled = false;

        OnShrineDeactivated?.Invoke();
        StartCoroutine(ResetActivationProgress());
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

    private void ShowWrongLightTypeEffect()
    {
        StartCoroutine(FlashWrongLightType());
    }

    private IEnumerator FlashWrongLightType()
    {
        Color originalColor = _shrineGlow.color;

        // Flash red to indicate wrong light type
        _shrineGlow.color = _wrongLightColor;
        yield return new WaitForSeconds(0.3f);
        _shrineGlow.color = originalColor;

        Debug.Log($"This shrine requires {_requiredLightType} light type!");
    }
}