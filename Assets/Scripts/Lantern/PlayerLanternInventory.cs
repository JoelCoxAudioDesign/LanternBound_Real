using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the player's lantern collection and abilities
/// Can be extended later for multiple lantern types
/// </summary>
public class PlayerLanternInventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] private bool _hasLantern = false;
    [SerializeField] private CollectibleLantern.LanternType _currentLanternType;

    [Header("Input Settings")]
    [SerializeField] private KeyCode _toggleKey = KeyCode.F;
    [SerializeField] private bool _useMouseAiming = true;

    private LanternController _lanternController;
    private List<CollectibleLantern.LanternType> _collectedLanterns;

    // Events
    public System.Action<bool> OnLanternStatusChanged;
    public System.Action<CollectibleLantern.LanternType> OnLanternTypeChanged;

    private void Awake()
    {
        _collectedLanterns = new List<CollectibleLantern.LanternType>();
        _lanternController = GetComponent<LanternController>();

        // Subscribe to lantern collection events
        CollectibleLantern.OnLanternCollected += OnLanternCollected;
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (!_hasLantern) return;

        // Toggle lantern on/off
        if (Input.GetKeyDown(_toggleKey))
        {
            ToggleLantern();
        }

        // Handle aiming if lantern is active
        if (_lanternController != null && _lanternController.IsLanternActive)
        {
            UpdateLanternAiming();
        }
    }

    private void UpdateLanternAiming()
    {
        if (!_useMouseAiming) return;

        // Get mouse position in world space
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = transform.position.z;

        // Calculate direction from player to mouse
        Vector2 aimDirection = (mouseWorldPos - transform.position).normalized;

        // Update lantern direction (you might need to modify LanternController for this)
        // For now, we'll just rotate the lantern towards the mouse
        if (_lanternController != null)
        {
            // This assumes you add a SetBeamDirection method to LanternController
            // _lanternController.SetBeamDirection(aimDirection);
        }
    }

    private void OnLanternCollected(CollectibleLantern.LanternType lanternType)
    {
        // Add to inventory
        if (!_collectedLanterns.Contains(lanternType))
        {
            _collectedLanterns.Add(lanternType);
        }

        // Set as current lantern
        _currentLanternType = lanternType;
        _hasLantern = true;

        // Notify other systems
        OnLanternStatusChanged?.Invoke(_hasLantern);
        OnLanternTypeChanged?.Invoke(_currentLanternType);

        Debug.Log($"Player now has lantern: {lanternType}");
    }

    public void ToggleLantern()
    {
        if (_lanternController != null)
        {
            _lanternController.ToggleLantern();
        }
    }

    public bool HasLantern()
    {
        return _hasLantern;
    }

    public CollectibleLantern.LanternType GetCurrentLanternType()
    {
        return _currentLanternType;
    }

    public List<CollectibleLantern.LanternType> GetCollectedLanterns()
    {
        return new List<CollectibleLantern.LanternType>(_collectedLanterns);
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        CollectibleLantern.OnLanternCollected -= OnLanternCollected;
    }
}