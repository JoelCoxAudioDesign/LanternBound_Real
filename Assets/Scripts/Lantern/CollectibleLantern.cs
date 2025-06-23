using UnityEngine;

/// <summary>
/// A lantern that can be picked up by the player
/// When collected, gives the player lantern abilities
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CollectibleLantern : MonoBehaviour
{
    [Header("Lantern Settings")]
    [SerializeField] private LanternType _lanternType = LanternType.BasicLantern;
    [SerializeField] private bool _destroyOnPickup = true;
    [SerializeField] private float _glowIntensity = 1f;

    [Header("Pickup Animation")]
    [SerializeField] private float _floatSpeed = 1f;
    [SerializeField] private float _floatHeight = 0.5f;
    [SerializeField] private float _rotateSpeed = 50f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem _glowEffect;
    [SerializeField] private AudioClip _pickupSound;
    [SerializeField] private GameObject _pickupEffect;

    public enum LanternType
    {
        BasicLantern,
        PowerfulLantern,
        MagicalLantern
    }

    private Vector3 _startPosition;
    private AudioSource _audioSource;
    private bool _collected = false;

    // Events
    public static System.Action<LanternType> OnLanternCollected;

    private void Awake()
    {
        _startPosition = transform.position;
        _audioSource = GetComponent<AudioSource>();

        // Setup collider as trigger
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;

        // Start glow effect if available
        if (_glowEffect != null)
            _glowEffect.Play();
    }

    private void Update()
    {
        if (!_collected)
        {
            AnimateLantern();
        }
    }

    private void AnimateLantern()
    {
        // Floating animation
        float newY = _startPosition.y + Mathf.Sin(Time.time * _floatSpeed) * _floatHeight;
        transform.position = new Vector3(_startPosition.x, newY, _startPosition.z);

        // Rotation animation
        transform.Rotate(0, 0, _rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_collected) return;

        // Check if it's the player
        if (other.CompareTag("Player"))
        {
            CollectLantern(other.gameObject);
        }
    }

    private void CollectLantern(GameObject player)
    {
        _collected = true;

        // Add lantern controller to player
        LanternController lanternController = player.GetComponent<LanternController>();
        if (lanternController == null)
        {
            lanternController = player.AddComponent<LanternController>();
        }

        // Configure lantern based on type
        ConfigureLanternController(lanternController);

        // Activate the lantern
        lanternController.ActivateLantern();

        // Play effects
        PlayPickupEffects();

        // Notify other systems
        OnLanternCollected?.Invoke(_lanternType);

        // Notify tutorial system
        OpeningAreaManager tutorialManager = FindFirstObjectByType<OpeningAreaManager>();
        if (tutorialManager != null)
        {
            tutorialManager.TriggerStepCompletion(OpeningAreaManager.TutorialStepType.LanternDiscovery);
        }

        // Destroy or hide the pickup
        if (_destroyOnPickup)
        {
            Destroy(gameObject, 0.5f); // Small delay for effects
        }
        else
        {
            gameObject.SetActive(false);
        }

        Debug.Log($"Player collected {_lanternType}!");
    }

    private void ConfigureLanternController(LanternController controller)
    {
        // Configure controller based on lantern type
        switch (_lanternType)
        {
            case LanternType.BasicLantern:
                controller.SetBeamRange(8f);
                controller.SetBeamWidth(25f);
                controller.SetBeamColor(Color.yellow);
                break;

            case LanternType.PowerfulLantern:
                controller.SetBeamRange(12f);
                controller.SetBeamWidth(35f);
                controller.SetBeamColor(Color.white);
                break;

            case LanternType.MagicalLantern:
                controller.SetBeamRange(15f);
                controller.SetBeamWidth(45f);
                controller.SetBeamColor(Color.cyan);
                break;
        }
    }

    private void PlayPickupEffects()
    {
        // Play sound
        if (_audioSource != null && _pickupSound != null)
        {
            _audioSource.PlayOneShot(_pickupSound);
        }

        // Stop glow effect
        if (_glowEffect != null)
        {
            _glowEffect.Stop();
        }

        // Spawn pickup effect
        if (_pickupEffect != null)
        {
            Instantiate(_pickupEffect, transform.position, transform.rotation);
        }
    }
}