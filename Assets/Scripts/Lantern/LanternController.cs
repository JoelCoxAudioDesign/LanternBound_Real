using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Fixed Lantern Controller with proper access levels and naming
/// Treats light types as "weapons" and tracks inner light progression
/// </summary>
public class LanternController : MonoBehaviour
{
    [Header("Light Type System")]
    [SerializeField] private LightType _currentLightType = LightType.None;
    [SerializeField] private List<LightType> _discoveredLightTypes = new List<LightType>();
    [SerializeField] private LightTypeData[] _lightTypeConfigurations;

    [Header("Inner Light Progression")]
    [SerializeField] private float _innerLightStrength = 0.1f; // Starts weak
    [SerializeField] private float _maxInnerLightStrength = 1.0f;
    [SerializeField] private UnityEngine.Light _characterAmbientLight; // Character's inner glow
    [SerializeField] private float _ambientLightBaseIntensity = 0.2f;

    [Header("Mana System")]
    [SerializeField] private float _currentMana = 100f;
    [SerializeField] private float _maxMana = 100f;
    [SerializeField] private float _manaRegenRate = 10f; // Per second
    [SerializeField] private bool _allowManaRegen = true;

    [Header("Lantern Beam")]
    [SerializeField] private Transform _lanternBeamOrigin;
    [SerializeField] private LineRenderer _beamRenderer;
    [SerializeField] private UnityEngine.Light _spotLight;
    [SerializeField] private LayerMask _lightInteractionLayers = -1;

    [Header("Input")]
    [SerializeField] private KeyCode _toggleLanternKey = KeyCode.F;
    [SerializeField] private KeyCode _cycleLightTypeKey = KeyCode.Tab;
    [SerializeField] private bool _useMouseAiming = true;

    [Header("Audio & Effects")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private ParticleSystem _lightEmissionEffect;

    // Public Properties - Fixed access levels
    public bool IsLanternActive { get; private set; }
    public bool HasLantern { get; private set; }
    public LightType CurrentLightType => _currentLightType; // Public getter
    public float ManaPercentage => _maxMana > 0 ? _currentMana / _maxMana : 0f;
    public float InnerLightPercentage => _maxInnerLightStrength > 0 ? _innerLightStrength / _maxInnerLightStrength : 0f;

    // Events
    public System.Action<LightType> OnLightTypeChanged;
    public System.Action<float> OnManaChanged;
    public System.Action<float> OnInnerLightChanged;
    public System.Action<ILightInteractable> OnObjectIlluminated;
    public System.Action<ILightInteractable> OnObjectLeftLight;
    public System.Action OnLanternAcquired;

    // Current light interactables
    private List<ILightInteractable> _currentlyIlluminated = new List<ILightInteractable>();
    private List<ILightInteractable> _previouslyIlluminated = new List<ILightInteractable>();

    // Light type configurations
    [System.Serializable]
    public class LightTypeData
    {
        public LightType lightType;
        public string displayName;
        [TextArea(2, 3)]
        public string description;

        [Header("Beam Properties")]
        public float beamRange = 10f;
        public float beamWidth = 30f; // Degrees
        public Color lightColor = Color.white;
        public float intensity = 1f;

        [Header("Mana Cost")]
        public float activationCost = 5f; // Cost to turn on
        public float sustainCost = 2f; // Cost per second while active
        public bool requiresContinuousMana = true;

        [Header("Special Properties")]
        public bool canRevealHidden = true;
        public bool canRepelEnemies = true;
        public bool canActivateShrines = true;
        public bool hasPiercing = false; // Can go through certain materials
        public float damage = 0f; // For combat light types

        [Header("Audio")]
        public AudioClip activationSound;
        public AudioClip sustainSound;
        public AudioClip deactivationSound;
    }

    public enum LightType
    {
        None,           // No lantern
        Ember,          // First light - weak, basic
        Radiance,       // Standard bright light
        SolarFlare,     // Intense, damages enemies
        MoonBeam,       // Reveals hidden secrets
        Starlight,      // Pierces through darkness barriers
        PrismaticLight, // Multi-colored, affects different elements
        VoidLight       // Advanced - reveals AND damages
    }

    private void Awake()
    {
        // Setup initial state
        IsLanternActive = false;
        HasLantern = false;

        // Setup audio
        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();

        // Setup beam components
        SetupBeamVisuals();

        // Update inner light
        UpdateInnerLightGlow();
    }

    private void Update()
    {
        HandleInput();
        UpdateManaSystem();
        UpdateLanternBeam();
        UpdateInnerLightGlow();
    }

    private void HandleInput()
    {
        if (!HasLantern) return;

        // Toggle lantern on/off
        if (Input.GetKeyDown(_toggleLanternKey))
        {
            if (IsLanternActive)
                DeactivateLantern();
            else
                ActivateLantern();
        }

        // Cycle through discovered light types
        if (Input.GetKeyDown(_cycleLightTypeKey) && _discoveredLightTypes.Count > 1)
        {
            CycleLightType();
        }
    }

    #region Lantern Acquisition and Light Type Discovery

    /// <summary>
    /// Call this when the player first finds the lantern
    /// </summary>
    public void AcquireLantern()
    {
        if (HasLantern) return;

        HasLantern = true;

        // Start with basic Ember light
        DiscoverLightType(LightType.Ember);

        // Increase inner light strength
        IncreaseInnerLight(0.2f);

        OnLanternAcquired?.Invoke();

        Debug.Log("✨ Lantern acquired! Your inner light begins to grow...");
    }

    /// <summary>
    /// Call this when player discovers a new light type
    /// </summary>
    /// <param name="newLightType">The light type to discover</param>
    public void DiscoverLightType(LightType newLightType)
    {
        if (_discoveredLightTypes.Contains(newLightType)) return;

        _discoveredLightTypes.Add(newLightType);

        // Automatically switch to new light type
        SwitchToLightType(newLightType);

        // Increase inner light strength with each discovery
        IncreaseInnerLight(0.1f);

        // Play discovery effect
        PlayLightDiscoveryEffect(newLightType);

        Debug.Log($"🌟 New light discovered: {GetLightTypeData(newLightType)?.displayName}!");
    }

    private void IncreaseInnerLight(float amount)
    {
        _innerLightStrength = Mathf.Min(_innerLightStrength + amount, _maxInnerLightStrength);
        OnInnerLightChanged?.Invoke(InnerLightPercentage);

        // Also increase max mana slightly
        _maxMana += 10f;
        _currentMana = _maxMana; // Restore to full
    }

    #endregion

    #region Light Type Management

    public void SwitchToLightType(LightType lightType)
    {
        if (!_discoveredLightTypes.Contains(lightType)) return;

        _currentLightType = lightType;
        OnLightTypeChanged?.Invoke(lightType);

        // Update beam properties
        UpdateBeamProperties();

        var lightData = GetLightTypeData(lightType);
        Debug.Log($"Switched to {lightData?.displayName ?? lightType.ToString()}");
    }

    private void CycleLightType()
    {
        if (_discoveredLightTypes.Count <= 1) return;

        int currentIndex = _discoveredLightTypes.IndexOf(_currentLightType);
        int nextIndex = (currentIndex + 1) % _discoveredLightTypes.Count;

        SwitchToLightType(_discoveredLightTypes[nextIndex]);
    }

    private LightTypeData GetLightTypeData(LightType lightType)
    {
        foreach (var data in _lightTypeConfigurations)
        {
            if (data.lightType == lightType)
                return data;
        }
        return null;
    }

    #endregion

    #region Lantern Activation/Deactivation

    public void ActivateLantern()
    {
        if (!HasLantern || IsLanternActive || _currentLightType == LightType.None) return;

        var lightData = GetLightTypeData(_currentLightType);
        if (lightData == null) return;

        // Check mana cost
        if (_currentMana < lightData.activationCost)
        {
            Debug.Log("Not enough mana to activate lantern!");
            return;
        }

        // Consume activation mana
        ConsumeMana(lightData.activationCost);

        IsLanternActive = true;

        // Setup beam
        UpdateBeamProperties();
        EnableBeamVisuals(true);

        // Play effects
        PlayLightActivationEffect(lightData);

        Debug.Log($"Lantern activated: {lightData.displayName}");
    }

    public void DeactivateLantern()
    {
        if (!IsLanternActive) return;

        IsLanternActive = false;

        // Clear illuminated objects
        ClearIlluminatedObjects();

        // Disable visuals
        EnableBeamVisuals(false);

        // Play deactivation sound
        var lightData = GetLightTypeData(_currentLightType);
        if (lightData?.deactivationSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(lightData.deactivationSound);
        }

        Debug.Log("Lantern deactivated");
    }

    #endregion

    #region Mana System

    private void UpdateManaSystem()
    {
        if (!HasLantern) return;

        // Consume mana if lantern is active
        if (IsLanternActive)
        {
            var lightData = GetLightTypeData(_currentLightType);
            if (lightData != null && lightData.requiresContinuousMana)
            {
                float manaCost = lightData.sustainCost * Time.deltaTime;

                if (_currentMana >= manaCost)
                {
                    ConsumeMana(manaCost);
                }
                else
                {
                    // Out of mana - deactivate lantern
                    DeactivateLantern();
                    Debug.Log("Lantern deactivated - out of mana!");
                }
            }
        }

        // Regenerate mana
        if (_allowManaRegen && _currentMana < _maxMana)
        {
            _currentMana = Mathf.Min(_currentMana + _manaRegenRate * Time.deltaTime, _maxMana);
            OnManaChanged?.Invoke(ManaPercentage);
        }
    }

    private void ConsumeMana(float amount)
    {
        _currentMana = Mathf.Max(_currentMana - amount, 0f);
        OnManaChanged?.Invoke(ManaPercentage);
    }

    public void RestoreMana(float amount)
    {
        _currentMana = Mathf.Min(_currentMana + amount, _maxMana);
        OnManaChanged?.Invoke(ManaPercentage);
    }

    #endregion

    #region Beam and Interaction System

    private void UpdateLanternBeam()
    {
        if (!IsLanternActive) return;

        // Update beam direction based on mouse/input
        Vector2 beamDirection = GetBeamDirection();

        // Perform light detection
        PerformLightDetection(beamDirection);

        // Update visual beam
        UpdateBeamVisuals(beamDirection);
    }

    private Vector2 GetBeamDirection()
    {
        if (_useMouseAiming)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;
            return ((Vector2)mouseWorldPos - (Vector2)transform.position).normalized;
        }

        // Default to facing direction
        return transform.right;
    }

    private void PerformLightDetection(Vector2 beamDirection)
    {
        var lightData = GetLightTypeData(_currentLightType);
        if (lightData == null) return;

        // Clear previous frame
        _previouslyIlluminated.Clear();
        _previouslyIlluminated.AddRange(_currentlyIlluminated);
        _currentlyIlluminated.Clear();

        // Cast rays within beam cone
        float halfAngle = lightData.beamWidth * 0.5f;
        int rayCount = Mathf.RoundToInt(lightData.beamWidth / 5f); // More rays for wider beams

        for (int i = 0; i < rayCount; i++)
        {
            float t = rayCount > 1 ? (float)i / (rayCount - 1) : 0.5f;
            float currentAngle = Mathf.Lerp(-halfAngle, halfAngle, t);

            Vector2 rayDirection = RotateVector2(beamDirection, currentAngle);

            RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDirection, lightData.beamRange, _lightInteractionLayers);

            if (hit.collider != null)
            {
                var lightInteractable = hit.collider.GetComponent<ILightInteractable>();
                if (lightInteractable != null && !_currentlyIlluminated.Contains(lightInteractable))
                {
                    _currentlyIlluminated.Add(lightInteractable);
                }
            }
        }

        // Process illumination events
        ProcessIlluminationEvents();
    }

    private void ProcessIlluminationEvents()
    {
        // Objects newly illuminated
        foreach (var illuminated in _currentlyIlluminated)
        {
            if (!_previouslyIlluminated.Contains(illuminated))
            {
                illuminated.OnIlluminated(this);
                OnObjectIlluminated?.Invoke(illuminated);
            }
        }

        // Objects that left the light
        foreach (var previously in _previouslyIlluminated)
        {
            if (!_currentlyIlluminated.Contains(previously))
            {
                previously.OnLeftLight(this);
                OnObjectLeftLight?.Invoke(previously);
            }
        }
    }

    #endregion

    #region Visuals and Effects

    private void SetupBeamVisuals()
    {
        // Setup beam renderer if not assigned
        if (_beamRenderer == null)
        {
            GameObject beamObj = new GameObject("LanternBeam");
            beamObj.transform.SetParent(transform);
            _beamRenderer = beamObj.AddComponent<LineRenderer>();
        }

        _beamRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _beamRenderer.startWidth = 0.1f;
        _beamRenderer.endWidth = 1f;
        _beamRenderer.positionCount = 2;

        // Setup spotlight if not assigned - FIXED naming conflict
        if (_spotLight == null)
        {
            _spotLight = GetComponent<UnityEngine.Light>();
            if (_spotLight == null)
                _spotLight = gameObject.AddComponent<UnityEngine.Light>();
        }

        _spotLight.type = UnityEngine.LightType.Spot; // FIXED - Use UnityEngine.LightType

        EnableBeamVisuals(false);
    }

    private void UpdateBeamProperties()
    {
        var lightData = GetLightTypeData(_currentLightType);
        if (lightData == null) return;

        // Update spotlight
        if (_spotLight != null)
        {
            _spotLight.color = lightData.lightColor;
            _spotLight.intensity = lightData.intensity * _innerLightStrength;
            _spotLight.range = lightData.beamRange;
            _spotLight.spotAngle = lightData.beamWidth;
        }

        // Update line renderer
        if (_beamRenderer != null)
        {
            _beamRenderer.material.color = lightData.lightColor;
        }
    }

    private void UpdateBeamVisuals(Vector2 direction)
    {
        var lightData = GetLightTypeData(_currentLightType);
        if (lightData == null || _beamRenderer == null) return;

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + (Vector3)(direction * lightData.beamRange);

        _beamRenderer.SetPosition(0, startPos);
        _beamRenderer.SetPosition(1, endPos);

        // Update spotlight direction
        if (_spotLight != null)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            _spotLight.transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
        }
    }

    private void EnableBeamVisuals(bool enabled)
    {
        if (_beamRenderer != null)
            _beamRenderer.enabled = enabled;

        if (_spotLight != null)
            _spotLight.enabled = enabled;
    }

    private void UpdateInnerLightGlow()
    {
        if (_characterAmbientLight != null)
        {
            float targetIntensity = _ambientLightBaseIntensity + (_innerLightStrength * 0.5f);
            _characterAmbientLight.intensity = Mathf.Lerp(_characterAmbientLight.intensity, targetIntensity, Time.deltaTime * 2f);

            // Color shifts as inner light grows
            Color targetColor = Color.Lerp(new Color(0.8f, 0.6f, 0.4f), Color.white, _innerLightStrength);
            _characterAmbientLight.color = targetColor;
        }
    }

    private void PlayLightActivationEffect(LightTypeData lightData)
    {
        if (_audioSource != null && lightData.activationSound != null)
        {
            _audioSource.PlayOneShot(lightData.activationSound);
        }

        if (_lightEmissionEffect != null)
        {
            var main = _lightEmissionEffect.main;
            main.startColor = lightData.lightColor;
            _lightEmissionEffect.Play();
        }
    }

    private void PlayLightDiscoveryEffect(LightType lightType)
    {
        // Play special discovery effect - could be more elaborate
        if (_lightEmissionEffect != null)
        {
            _lightEmissionEffect.Play();
        }

        Debug.Log($"✨ {lightType} light discovered! Your inner light grows stronger...");
    }

    #endregion

    #region Utility

    private Vector2 RotateVector2(Vector2 vector, float angleInDegrees)
    {
        float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angleInRadians);
        float sin = Mathf.Sin(angleInRadians);

        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos
        );
    }

    private void ClearIlluminatedObjects()
    {
        foreach (var illuminated in _currentlyIlluminated)
        {
            illuminated.OnLeftLight(this);
            OnObjectLeftLight?.Invoke(illuminated);
        }
        _currentlyIlluminated.Clear();
    }

    #endregion

    #region Public API

    /// <summary>
    /// Check if current light type has specific property
    /// </summary>
    public bool CurrentLightHasProperty(System.Func<LightTypeData, bool> propertyCheck)
    {
        var lightData = GetLightTypeData(_currentLightType);
        return lightData != null && propertyCheck(lightData);
    }

    /// <summary>
    /// Get current light type display name
    /// </summary>
    public string GetCurrentLightTypeName()
    {
        var lightData = GetLightTypeData(_currentLightType);
        return lightData?.displayName ?? "Unknown";
    }

    /// <summary>
    /// Check if player has discovered specific light type
    /// </summary>
    public bool HasDiscovered(LightType lightType)
    {
        return _discoveredLightTypes.Contains(lightType);
    }

    #endregion
}