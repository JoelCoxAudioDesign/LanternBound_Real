using UnityEngine;

/// <summary>
/// Player Lantern Inventory - manages which lantern abilities the player has
/// This bridges between the collectible and the controller
/// </summary>
public class PlayerLanternInventory : MonoBehaviour
{
    [Header("Lantern State")]
    [SerializeField] private bool _hasLantern = false;
    [SerializeField] private CollectibleLantern _discoveredLantern;

    [Header("Components")]
    [SerializeField] private EnhancedLanternController _lanternController;
    [SerializeField] private PlayerMovement _playerMovement;

    public bool HasLantern => _hasLantern;
    public EnhancedLanternController LanternController => _lanternController;

    private void Awake()
    {
        // Get or add required components
        _lanternController = GetComponent<EnhancedLanternController>();
        _playerMovement = GetComponent<PlayerMovement>();

        // Subscribe to lantern discovery events
        CollectibleLantern.OnLanternDiscovered += OnLanternDiscovered;
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        CollectibleLantern.OnLanternDiscovered -= OnLanternDiscovered;
    }

    private void OnLanternDiscovered(CollectibleLantern lantern)
    {
        // Check if this discovery affects this player
        float distance = Vector2.Distance(transform.position, lantern.transform.position);
        if (distance <= lantern._discoveryRadius) // Access through reflection or make public
        {
            _hasLantern = true;
            _discoveredLantern = lantern;

            // Enable lantern controller if it wasn't already
            if (_lanternController != null)
            {
                _lanternController.enabled = true;
            }

            Debug.Log("Player acquired lantern abilities!");
        }
    }

    /// <summary>
    /// For save/load system - restore lantern state
    /// </summary>
    public void RestoreLanternState(bool hasLantern)
    {
        _hasLantern = hasLantern;

        if (_lanternController != null)
        {
            _lanternController.enabled = hasLantern;

            if (hasLantern)
            {
                // Restore basic lantern without the discovery sequence
                _lanternController.AcquireLantern();
            }
        }
    }

    /// <summary>
    /// Check if player can use lantern abilities
    /// </summary>
    public bool CanUseLantern()
    {
        return _hasLantern && _lanternController != null && _lanternController.HasLantern;
    }
}
