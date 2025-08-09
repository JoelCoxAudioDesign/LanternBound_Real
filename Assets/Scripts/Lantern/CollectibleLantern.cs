using UnityEngine;
using System.Collections;

/// <summary>
/// Enhanced collectible lantern that grants the player their lantern abilities
/// Creates an emotionally impactful first discovery moment
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class EnhancedCollectibleLantern : MonoBehaviour
{
    [Header("Discovery Experience")]
    [SerializeField] private float _discoveryRadius = 3f;
    [SerializeField] private bool _requiresPlayerTouch = true;
    [SerializeField] private float _anticipationTime = 2f;

    [Header("Visual Effects")]
    [SerializeField] private Light _ambientGlow;
    [SerializeField] private ParticleSystem _discoveryEffect;
    [SerializeField] private SpriteRenderer _lanternSprite;
    [SerializeField] private Color _glowColor = Color.yellow;
    [SerializeField] private float _pulseSpeed = 2f;
    [SerializeField] private float _floatHeight = 0.3f;
    [SerializeField] private float _floatSpeed = 1f;

    [Header("Audio")]
    [SerializeField] private AudioClip _discoverySound;
    [SerializeField] private AudioClip _ambientHum;
    [SerializeField] private AudioSource _audioSource;

    [Header("Camera Effects")]
    [SerializeField] private bool _useCameraFocus = true;
    [SerializeField] private float _cameraFocusTime = 3f;
    [SerializeField] private float _cameraZoomAmount = 0.8f;

    private Vector3 _startPosition;
    private bool _discovered = false;
    private bool _playerInRange = false;
    private Camera _mainCamera;
    private Vector3 _originalCameraPosition;
    private float _originalCameraSize;

    // Events
    public static System.Action OnLanternDiscovered;

    private void Awake()
    {
        _startPosition = transform.position;
        _mainCamera = Camera.main;

        if (_mainCamera != null)
        {
            _originalCameraPosition = _mainCamera.transform.position;
            _originalCameraSize = _mainCamera.orthographicSize;
        }

        SetupComponents();
    }

    private void SetupComponents()
    {
        // Setup collider
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;

        // Setup audio
        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();

        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();

        _audioSource.playOnAwake = false;
        _audioSource.loop = true;

        // Setup ambient light
        if (_ambientGlow == null)
        {
            _ambientGlow = gameObject.AddComponent<Light>();
        }

        _ambientGlow.type = LightType.Point;
        _ambientGlow.color = _glowColor;
        _ambientGlow.intensity = 1f;
        _ambientGlow.range = _discoveryRadius;

        // Setup sprite if not assigned
        if (_lanternSprite == null)
            _lanternSprite = GetComponent<SpriteRenderer>();

        // Start ambient sound
        if (_ambientHum != null)
        {
            _audioSource.clip = _ambientHum;
            _audioSource.volume = 0.3f;
            _audioSource.Play();
        }
    }

    private void Update()
    {
        if (!_discovered)
        {
            AnimateLantern();
            CheckPlayerProximity();
        }
    }

    private void AnimateLantern()
    {
        // Floating animation
        float newY = _startPosition.y + Mathf.Sin(Time.time * _floatSpeed) * _floatHeight;
        transform.position = new Vector3(_startPosition.x, newY, _startPosition.z);

        // Pulsing glow
        if (_ambientGlow != null)
        {
            float pulseIntensity = 1f + Mathf.Sin(Time.time * _pulseSpeed) * 0.5f;
            _ambientGlow.intensity = pulseIntensity;
        }

        // Sprite pulsing
        if (_lanternSprite != null)
        {
            float alpha = 0.8f + Mathf.Sin(Time.time * _pulseSpeed * 1.5f) * 0.2f;
            Color spriteColor = _lanternSprite.color;
            spriteColor.a = alpha;
            _lanternSprite.color = spriteColor;
        }
    }

    private void CheckPlayerProximity()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.transform.position);

        if (distance <= _discoveryRadius && !_playerInRange)
        {
            _playerInRange = true;
            OnPlayerApproach();
        }
        else if (distance > _discoveryRadius && _playerInRange)
        {
            _playerInRange = false;
            OnPlayerLeave();
        }
    }

    private void OnPlayerApproach()
    {
        // Increase glow intensity
        if (_ambientGlow != null)
        {
            StartCoroutine(AdjustGlowIntensity(2f, 1f));
        }

        // Increase audio volume
        if (_audioSource != null && _audioSource.isPlaying)
        {
            StartCoroutine(AdjustAudioVolume(0.6f, 1f));
        }

        Debug.Log("Player approaches the ancient lantern... something stirs within...");
    }

    private void OnPlayerLeave()
    {
        // Decrease effects back to normal
        if (_ambientGlow != null)
        {
            StartCoroutine(AdjustGlowIntensity(1f, 1f));
        }

        if (_audioSource != null && _audioSource.isPlaying)
        {
            StartCoroutine(AdjustAudioVolume(0.3f, 1f));
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_discovered || !other.CompareTag("Player")) return;

        if (_requiresPlayerTouch)
        {
            StartDiscoverySequence(other.gameObject);
        }
    }

    private void StartDiscoverySequence(GameObject player)
    {
        _discovered = true;
        StartCoroutine(DiscoverySequence(player));
    }

    private IEnumerator DiscoverySequence(GameObject player)
    {
        Debug.Log("✨ The ancient lantern awakens...");

        // Stop player movement briefly for the moment
        var playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // Camera focus effect
        if (_useCameraFocus && _mainCamera != null)
        {
            yield return StartCoroutine(CameraFocusSequence());
        }

        // Anticipation phase
        yield return StartCoroutine(AnticipationPhase());

        // Discovery climax
        yield return StartCoroutine(DiscoveryClimax());

        // Grant lantern to player
        GrantLanternToPlayer(player);

        // Restoration phase
        yield return StartCoroutine(RestorationPhase(player));

        // Cleanup
        OnLanternDiscovered?.Invoke();
        Destroy(gameObject, 1f);
    }

    private IEnumerator CameraFocusSequence()
    {
        Vector3 targetPosition = new Vector3(transform.position.x, transform.position.y, _originalCameraPosition.z);
        float targetSize = _originalCameraSize * _cameraZoomAmount;

        float duration = _cameraFocusTime * 0.5f;
        float elapsed = 0f;

        // Zoom in
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            _mainCamera.transform.position = Vector3.Lerp(_originalCameraPosition, targetPosition, t);
            _mainCamera.orthographicSize = Mathf.Lerp(_originalCameraSize, targetSize, t);

            yield return null;
        }

        _mainCamera.transform.position = targetPosition;
        _mainCamera.orthographicSize = targetSize;
    }

    private IEnumerator AnticipationPhase()
    {
        // Increase all effects gradually
        float duration = _anticipationTime;
        float elapsed = 0f;

        float startIntensity = _ambientGlow ? _ambientGlow.intensity : 1f;
        float targetIntensity = startIntensity * 3f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Intensify glow
            if (_ambientGlow != null)
            {
                _ambientGlow.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
                _ambientGlow.range = Mathf.Lerp(_discoveryRadius, _discoveryRadius * 2f, t);
            }

            // Faster pulsing
            _pulseSpeed = Mathf.Lerp(2f, 8f, t);

            yield return null;
        }
    }

    private IEnumerator DiscoveryClimax()
    {
        // Play discovery sound
        if (_audioSource != null && _discoverySound != null)
        {
            _audioSource.Stop();
            _audioSource.clip = _discoverySound;
            _audioSource.loop = false;
            _audioSource.volume = 0.8f;
            _audioSource.Play();
        }

        // Intense light flash
        if (_ambientGlow != null)
        {
            _ambientGlow.intensity = 8f;
            _ambientGlow.range = _discoveryRadius * 3f;
        }

        // Spawn discovery particles
        if (_discoveryEffect != null)
        {
            _discoveryEffect.Play();
        }

        yield return new WaitForSeconds(1f);

        Debug.Log("🌟 The lantern bonds with your soul... Your inner light begins to grow...");
    }

    private void GrantLanternToPlayer(GameObject player)
    {
        // Add lantern controller to player
        var lanternController = player.GetComponent<LanternController>();
        if (lanternController == null)
        {
            lanternController = player.AddComponent<LanternController>();
        }

        // Grant the lantern
        lanternController.AcquireLantern();

        Debug.Log("✨ You have acquired the Lantern of Inner Light!");
    }

    private IEnumerator RestorationPhase(GameObject player)
    {
        // Restore camera
        if (_useCameraFocus && _mainCamera != null)
        {
            float duration = 2f;
            float elapsed = 0f;

            Vector3 startPos = _mainCamera.transform.position;
            float startSize = _mainCamera.orthographicSize;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                _mainCamera.transform.position = Vector3.Lerp(startPos, _originalCameraPosition, t);
                _mainCamera.orthographicSize = Mathf.Lerp(startSize, _originalCameraSize, t);

                yield return null;
            }

            _mainCamera.transform.position = _originalCameraPosition;
            _mainCamera.orthographicSize = _originalCameraSize;
        }

        // Re-enable player movement
        var playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        yield return new WaitForSeconds(1f);
    }

    private IEnumerator AdjustGlowIntensity(float targetIntensity, float duration)
    {
        if (_ambientGlow == null) yield break;

        float startIntensity = _ambientGlow.intensity;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            _ambientGlow.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
            yield return null;
        }

        _ambientGlow.intensity = targetIntensity;
    }

    private IEnumerator AdjustAudioVolume(float targetVolume, float duration)
    {
        if (_audioSource == null) yield break;

        float startVolume = _audioSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            _audioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }

        _audioSource.volume = targetVolume;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = _glowColor;
        Gizmos.DrawWireSphere(transform.position, _discoveryRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawIcon(transform.position, "Lantern", true);
    }
}