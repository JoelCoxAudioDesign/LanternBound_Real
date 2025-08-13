﻿using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Complete Lantern Controller with skill tree progression system
/// Manages light types, inner light growth, and mana systems
/// </summary>
public class EnhancedLanternController : MonoBehaviour
{
    [Header("Light Type System")]
    [SerializeField] private LightType _currentLightType = LightType.None;
    [SerializeField] private List<LightType> _discoveredLightTypes = new List<LightType>();
    [SerializeField] private LightTypeData[] _lightTypeConfigurations;

    [Header("Inner Light Progression")]
    [SerializeField] private float _innerLightStrength = 0.1f;
    [SerializeField] private float _maxInnerLightStrength = 5.0f;
    [SerializeField] private int _lightEssencePoints = 0; // Currency for upgrades
    [SerializeField] private LightSkillTree _skillTree;

    [Header("Mana System")]
    [SerializeField] private float _currentMana = 100f;
    [SerializeField] private float _maxMana = 100f;
    [SerializeField] private float _manaRegenRate = 10f;
    [SerializeField] private bool _allowManaRegen = true;

    [Header("Lantern Beam")]
    [SerializeField] private Transform _lanternBeamOrigin;
    [SerializeField] private LineRenderer _beamRenderer;
    [SerializeField] private Light _spotLight;
    [SerializeField] private LayerMask _lightInteractionLayers = -1;

    [Header("Character Glow")]
    [SerializeField] private Light _characterAmbientLight;
    [SerializeField] private ParticleSystem _innerLightParticles;
    [SerializeField] private SpriteRenderer _characterGlow;

    [Header("Input")]
    [SerializeField] private bool _useMouseAiming = true;
    [SerializeField] private bool _useControllerAiming = true;

    [Header("Audio & Effects")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private ParticleSystem _lightEmissionEffect;

    // Public Properties
    public bool IsLanternActive { get; private set; }
    public bool HasLantern { get; private set; }
    public LightType CurrentLightType => _currentLightType;
    public float ManaPercentage => _maxMana > 0 ? _currentMana / _maxMana : 0f;
    public float InnerLightPercentage => _maxInnerLightStrength > 0 ? _innerLightStrength / _maxInnerLightStrength : 0f;
    public int LightEssencePoints => _lightEssencePoints;
    public LightSkillTree SkillTree => _skillTree;

    // Events
    public System.Action<LightType> OnLightTypeChanged;
    public System.Action<float> OnManaChanged;
    public System.Action<float> OnInnerLightChanged;
    public System.Action<int> OnLightEssenceChanged;
    public System.Action<ILightInteractable> OnObjectIlluminated;
    public System.Action<ILightInteractable> OnObjectLeftLight;
    public System.Action OnLanternAcquired;
    public System.Action<LightType> OnLightTypeDiscovered;
    public System.Action<LightSkill> OnSkillUnlocked;

    // Current illuminated objects
    private List<ILightInteractable> _currentlyIlluminated = new List<ILightInteractable>();
    private List<ILightInteractable> _previouslyIlluminated = new List<ILightInteractable>();

    // Light type definitions
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

    [System.Serializable]
    public class LightTypeData
    {
        public LightType lightType;
        public string displayName;
        [TextArea(2, 3)]
        public string description;

        [Header("Beam Properties")]
        public float baseRange = 10f;
        public float baseWidth = 30f; // Degrees
        public Color lightColor = Color.white;
        public float baseIntensity = 1f;

        [Header("Mana Costs")]
        public float activationCost = 5f;
        public float sustainCost = 2f;
        public bool requiresContinuousMana = true;

        [Header("Special Properties")]
        public bool canRevealHidden = true;
        public bool canRepelEnemies = true;
        public bool canActivateShrines = true;
        public bool hasPiercing = false;
        public float baseDamage = 0f;

        [Header("Unlock Requirements")]
        public int requiredEssencePoints = 0;
        public LightType[] requiredLightTypes;
        public string unlockCondition = "";

        [Header("Audio")]
        public AudioClip activationSound;
        public AudioClip sustainSound;
        public AudioClip deactivationSound;
    }

    private void Awake()
    {
        InitializeLanternSystem();
    }

    private void Update()
    {
        HandleInput();
        UpdateManaSystem();
        if (IsLanternActive)
        {
            UpdateLanternBeam();
        }
        UpdateInnerLightEffects();
    }

    #region Initialization

    private void InitializeLanternSystem()
    {
        // Setup initial state
        IsLanternActive = false;
        HasLantern = false;

        // Initialize skill tree
        if (_skillTree == null)
            _skillTree = new LightSkillTree();
        _skillTree.Initialize(this);

        // Setup audio
        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();

        // Setup beam components
        SetupBeamVisuals();

        // Update effects
        UpdateInnerLightEffects();

        Debug.Log("Lantern system initialized");
    }

    private void SetupBeamVisuals()
    {
        // Setup beam renderer
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

        // Setup spotlight
        if (_spotLight == null)
        {
            _spotLight = GetComponent<Light>();
            if (_spotLight == null)
                _spotLight = gameObject.AddComponent<Light>();
        }

        _spotLight.type = UnityEngine.LightType.Spot;
        EnableBeamVisuals(false);
    }

    #endregion

    #region Input Handling

    private void HandleInput()
    {
        if (!HasLantern) return;

        // Lantern toggle
        if (InputManager.LanternTogglePressed)
        {
            if (IsLanternActive)
                DeactivateLantern();
            else
                ActivateLantern();
        }

        // Cycle light types (mouse wheel or controller)
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0f && _discoveredLightTypes.Count > 1)
        {
            CycleLightType(scrollInput > 0f);
        }

        // Number key shortcuts for light types
        for (int i = 1; i <= 8; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                int lightIndex = i - 1;
                if (lightIndex < _discoveredLightTypes.Count)
                {
                    SwitchToLightType(_discoveredLightTypes[lightIndex]);
                }
            }
        }
    }

    #endregion

    #region Lantern Acquisition and Light Discovery

    /// <summary>
    /// Call when player first finds the lantern
    /// </summary>
    public void AcquireLantern()
    {
        if (HasLantern) return;

        HasLantern = true;

        // Start with Ember light
        DiscoverLightType(LightType.Ember, true);

        // Initial inner light boost
        IncreaseInnerLight(0.5f);

        // Grant some starting essence points
        AddLightEssence(10);

        OnLanternAcquired?.Invoke();

        Debug.Log("✨ Ancient Lantern acquired! Your inner light awakens...");
    }

    /// <summary>
    /// Discover a new light type
    /// </summary>
    public void DiscoverLightType(LightType newLightType, bool autoSwitch = false)
    {
        if (_discoveredLightTypes.Contains(newLightType)) return;

        var lightData = GetLightTypeData(newLightType);
        if (lightData == null) return;

        // Check unlock requirements
        if (!CanUnlockLightType(newLightType))
        {
            Debug.Log($"Cannot unlock {lightData.displayName} yet. Requirements not met.");
            return;
        }

        _discoveredLightTypes.Add(newLightType);

        if (autoSwitch || _currentLightType == LightType.None)
        {
            SwitchToLightType(newLightType);
        }

        // Increase inner light
        IncreaseInnerLight(0.3f);

        // Grant essence points
        AddLightEssence(15);

        // Play discovery effects
        PlayLightDiscoveryEffect(newLightType);

        OnLightTypeDiscovered?.Invoke(newLightType);

        Debug.Log($"🌟 New light discovered: {lightData.displayName}!");
    }

    private bool CanUnlockLightType(LightType lightType)
    {
        var lightData = GetLightTypeData(lightType);
        if (lightData == null) return false;

        // Check essence points
        if (_lightEssencePoints < lightData.requiredEssencePoints)
            return false;

        // Check required light types
        if (lightData.requiredLightTypes != null)
        {
            foreach (var requiredType in lightData.requiredLightTypes)
            {
                if (!_discoveredLightTypes.Contains(requiredType))
                    return false;
            }
        }

        return true;
    }

    #endregion

    #region Light Type Management

    public void SwitchToLightType(LightType lightType)
    {
        if (!_discoveredLightTypes.Contains(lightType)) return;

        _currentLightType = lightType;
        UpdateBeamProperties();
        OnLightTypeChanged?.Invoke(lightType);

        var lightData = GetLightTypeData(lightType);
        Debug.Log($"Switched to {lightData?.displayName ?? lightType.ToString()}");
    }

    private void CycleLightType(bool forward = true)
    {
        if (_discoveredLightTypes.Count <= 1) return;

        int currentIndex = _discoveredLightTypes.IndexOf(_currentLightType);
        int nextIndex;

        if (forward)
            nextIndex = (currentIndex + 1) % _discoveredLightTypes.Count;
        else
            nextIndex = (currentIndex - 1 + _discoveredLightTypes.Count) % _discoveredLightTypes.Count;

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

    #region Inner Light and Progression

    private void IncreaseInnerLight(float amount)
    {
        float oldStrength = _innerLightStrength;
        _innerLightStrength = Mathf.Min(_innerLightStrength + amount, _maxInnerLightStrength);

        OnInnerLightChanged?.Invoke(InnerLightPercentage);

        // Increase max mana based on inner light growth
        float manaIncrease = (amount / _maxInnerLightStrength) * 50f;
        _maxMana += manaIncrease;
        _currentMana = _maxMana; // Restore to full

        Debug.Log($"Inner Light increased: {oldStrength:F2} → {_innerLightStrength:F2}");
    }

    public void AddLightEssence(int amount)
    {
        _lightEssencePoints += amount;
        OnLightEssenceChanged?.Invoke(_lightEssencePoints);
        Debug.Log($"Light Essence gained: +{amount} (Total: {_lightEssencePoints})");
    }

    public bool SpendLightEssence(int amount)
    {
        if (_lightEssencePoints >= amount)
        {
            _lightEssencePoints -= amount;
            OnLightEssenceChanged?.Invoke(_lightEssencePoints);
            return true;
        }
        return false;
    }

    #endregion

    #region Lantern Activation

    public void ActivateLantern()
    {
        if (!HasLantern || IsLanternActive || _currentLightType == LightType.None) return;

        var lightData = GetLightTypeData(_currentLightType);
        if (lightData == null) return;

        // Check mana cost
        float activationCost = GetModifiedActivationCost(lightData);
        if (_currentMana < activationCost)
        {
            Debug.Log("Not enough mana to activate lantern!");
            return;
        }

        // Consume mana
        ConsumeMana(activationCost);

        IsLanternActive = true;
        UpdateBeamProperties();
        EnableBeamVisuals(true);
        PlayLightActivationEffect(lightData);

        Debug.Log($"Lantern activated: {lightData.displayName}");
    }

    public void DeactivateLantern()
    {
        if (!IsLanternActive) return;

        IsLanternActive = false;
        ClearIlluminatedObjects();
        EnableBeamVisuals(false);

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
                float sustainCost = GetModifiedSustainCost(lightData) * Time.deltaTime;

                if (_currentMana >= sustainCost)
                {
                    ConsumeMana(sustainCost);
                }
                else
                {
                    DeactivateLantern();
                    Debug.Log("Lantern deactivated - out of mana!");
                }
            }
        }

        // Regenerate mana
        if (_allowManaRegen && _currentMana < _maxMana)
        {
            float regenRate = GetModifiedManaRegen();
            _currentMana = Mathf.Min(_currentMana + regenRate * Time.deltaTime, _maxMana);
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

    #region Beam System

    private void UpdateLanternBeam()
    {
        Vector2 beamDirection = GetBeamDirection();
        PerformLightDetection(beamDirection);
        UpdateBeamVisuals(beamDirection);
    }

    private Vector2 GetBeamDirection()
    {
        // Mouse aiming
        if (_useMouseAiming && Input.mousePosition != Vector3.zero)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(InputManager.MousePosition);
            mouseWorldPos.z = 0f;
            Vector2 direction = ((Vector2)mouseWorldPos - (Vector2)transform.position);
            if (direction.magnitude > 0.1f)
                return direction.normalized;
        }

        // Controller aiming
        if (_useControllerAiming && InputManager.RightStickInput.magnitude > 0.3f)
        {
            return InputManager.RightStickInput.normalized;
        }

        // Default to character facing direction
        return transform.right;
    }

    private void PerformLightDetection(Vector2 beamDirection)
    {
        var lightData = GetLightTypeData(_currentLightType);
        if (lightData == null) return;

        // Clear previous illumination
        _previouslyIlluminated.Clear();
        _previouslyIlluminated.AddRange(_currentlyIlluminated);
        _currentlyIlluminated.Clear();

        // Get modified beam properties
        float range = GetModifiedBeamRange(lightData);
        float width = GetModifiedBeamWidth(lightData);

        // Cast rays within beam cone
        float halfAngle = width * 0.5f;
        int rayCount = Mathf.Max(5, Mathf.RoundToInt(width / 5f));

        for (int i = 0; i < rayCount; i++)
        {
            float t = rayCount > 1 ? (float)i / (rayCount - 1) : 0.5f;
            float currentAngle = Mathf.Lerp(-halfAngle, halfAngle, t);
            Vector2 rayDirection = RotateVector2(beamDirection, currentAngle);

            RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDirection, range, _lightInteractionLayers);

            if (hit.collider != null)
            {
                var lightInteractable = hit.collider.GetComponent<ILightInteractable>();
                if (lightInteractable != null && !_currentlyIlluminated.Contains(lightInteractable))
                {
                    _currentlyIlluminated.Add(lightInteractable);
                }
            }
        }

        ProcessIlluminationEvents();
    }

    private void ProcessIlluminationEvents()
    {
        // Newly illuminated objects
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

    #region Skill Tree Integration

    private float GetModifiedBeamRange(LightTypeData lightData)
    {
        float baseRange = lightData.baseRange;
        float modifier = _skillTree.GetModifier(LightSkill.SkillType.BeamRange, _currentLightType);
        return baseRange * (1f + modifier);
    }

    private float GetModifiedBeamWidth(LightTypeData lightData)
    {
        float baseWidth = lightData.baseWidth;
        float modifier = _skillTree.GetModifier(LightSkill.SkillType.BeamWidth, _currentLightType);
        return baseWidth * (1f + modifier);
    }

    private float GetModifiedActivationCost(LightTypeData lightData)
    {
        float baseCost = lightData.activationCost;
        float modifier = _skillTree.GetModifier(LightSkill.SkillType.ManaEfficiency, _currentLightType);
        return baseCost * (1f - modifier);
    }

    private float GetModifiedSustainCost(LightTypeData lightData)
    {
        float baseCost = lightData.sustainCost;
        float modifier = _skillTree.GetModifier(LightSkill.SkillType.ManaEfficiency, _currentLightType);
        return baseCost * (1f - modifier);
    }

    private float GetModifiedManaRegen()
    {
        float baseRegen = _manaRegenRate;
        float modifier = _skillTree.GetModifier(LightSkill.SkillType.ManaRegeneration, LightType.None);
        return baseRegen * (1f + modifier);
    }

    #endregion

    #region Visual Effects

    private void UpdateBeamProperties()
    {
        var lightData = GetLightTypeData(_currentLightType);
        if (lightData == null) return;

        if (_spotLight != null)
        {
            _spotLight.color = lightData.lightColor;
            _spotLight.intensity = GetModifiedBeamIntensity(lightData);
            _spotLight.range = GetModifiedBeamRange(lightData);
            _spotLight.spotAngle = GetModifiedBeamWidth(lightData);
        }

        if (_beamRenderer != null)
        {
            _beamRenderer.material.color = lightData.lightColor;
        }
    }

    private float GetModifiedBeamIntensity(LightTypeData lightData)
    {
        float baseIntensity = lightData.baseIntensity;
        float innerLightBonus = _innerLightStrength / _maxInnerLightStrength;
        float skillModifier = _skillTree.GetModifier(LightSkill.SkillType.LightIntensity, _currentLightType);
        return baseIntensity * (1f + innerLightBonus * 0.5f) * (1f + skillModifier);
    }

    private void UpdateInnerLightEffects()
    {
        // Character ambient light
        if (_characterAmbientLight != null)
        {
            float targetIntensity = 0.2f + (_innerLightStrength / _maxInnerLightStrength) * 0.8f;
            _characterAmbientLight.intensity = Mathf.Lerp(_characterAmbientLight.intensity, targetIntensity, Time.deltaTime * 2f);

            Color targetColor = Color.Lerp(new Color(0.8f, 0.6f, 0.4f), Color.white, _innerLightStrength / _maxInnerLightStrength);
            _characterAmbientLight.color = targetColor;
        }

        // Inner light particles
        if (_innerLightParticles != null)
        {
            var emission = _innerLightParticles.emission;
            emission.rateOverTime = (_innerLightStrength / _maxInnerLightStrength) * 10f;
        }

        // Character glow
        if (_characterGlow != null)
        {
            Color glowColor = _characterGlow.color;
            glowColor.a = (_innerLightStrength / _maxInnerLightStrength) * 0.3f;
            _characterGlow.color = glowColor;
        }
    }

    private void UpdateBeamVisuals(Vector2 direction)
    {
        if (_beamRenderer == null) return;

        var lightData = GetLightTypeData(_currentLightType);
        if (lightData == null) return;

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + (Vector3)(direction * GetModifiedBeamRange(lightData));

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
        var lightData = GetLightTypeData(lightType);
        if (lightData == null) return;

        // Visual burst effect
        if (_lightEmissionEffect != null)
        {
            var main = _lightEmissionEffect.main;
            main.startColor = lightData.lightColor;
            var burst = _lightEmissionEffect.emission;
            burst.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 20)
            });
            _lightEmissionEffect.Play();
        }

        // Increase character glow temporarily
        StartCoroutine(DiscoveryGlowEffect(lightData.lightColor));
    }

    private IEnumerator DiscoveryGlowEffect(Color lightColor)
    {
        if (_characterAmbientLight == null) yield break;

        Color originalColor = _characterAmbientLight.color;
        float originalIntensity = _characterAmbientLight.intensity;

        // Bright flash
        _characterAmbientLight.color = lightColor;
        _characterAmbientLight.intensity = originalIntensity * 3f;

        yield return new WaitForSeconds(0.5f);

        // Fade back
        float elapsed = 0f;
        float duration = 2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            _characterAmbientLight.color = Color.Lerp(lightColor, originalColor, t);
            _characterAmbientLight.intensity = Mathf.Lerp(originalIntensity * 3f, originalIntensity, t);

            yield return null;
        }

        _characterAmbientLight.color = originalColor;
        _characterAmbientLight.intensity = originalIntensity;
    }

    #endregion

    #region Utilities

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

    public bool HasDiscovered(LightType lightType)
    {
        return _discoveredLightTypes.Contains(lightType);
    }

    public string GetCurrentLightTypeName()
    {
        var lightData = GetLightTypeData(_currentLightType);
        return lightData?.displayName ?? "Unknown";
    }

    public bool CanAffordSkill(LightSkill skill)
    {
        return _lightEssencePoints >= skill.EssenceCost;
    }

    public bool UnlockSkill(LightSkill skill)
    {
        if (_skillTree.CanUnlock(skill) && SpendLightEssence(skill.EssenceCost))
        {
            _skillTree.UnlockSkill(skill);
            OnSkillUnlocked?.Invoke(skill);
            return true;
        }
        return false;
    }

    public List<LightType> GetDiscoveredLightTypes()
    {
        return new List<LightType>(_discoveredLightTypes);
    }

    #endregion
}
}