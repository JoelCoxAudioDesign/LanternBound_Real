using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Automatically sets up the player GameObject with all required components for movement and input
/// Run this script to fix input issues and ensure proper player configuration
/// </summary>
public class PlayerSetupManager : MonoBehaviour
{
    [Header("Player Setup")]
    [SerializeField] private bool _setupPlayerComponents = false;
    [SerializeField] private bool _validateSetup = false;
    
    [Header("Required Assets")]
    [SerializeField] private InputActionAsset _inputActionsAsset;
    [SerializeField] private PlayerMovementStats _movementStats;
    
    [Header("Player GameObject Reference")]
    [SerializeField] private GameObject _playerPrefab;
    
    private void OnValidate()
    {
        if (_setupPlayerComponents)
        {
            _setupPlayerComponents = false;
            SetupPlayerComponents();
        }
        
        if (_validateSetup)
        {
            _validateSetup = false;
            ValidatePlayerSetup();
        }
    }
    
    [ContextMenu("Setup Player Components")]
    public void SetupPlayerComponents()
    {
        GameObject player = FindOrCreatePlayer();
        if (player == null)
        {
            Debug.LogError("No player GameObject found or created!");
            return;
        }
        
        Debug.Log($"Setting up player components on: {player.name}");
        
        // Ensure player has correct tag
        if (!player.CompareTag("Player"))
        {
            player.tag = "Player";
            Debug.Log("✓ Set player tag to 'Player'");
        }
        
        // Setup required components in correct order
        SetupRigidbody2D(player);
        SetupColliders(player);
        SetupInputSystem(player);
        SetupMovementSystem(player);
        SetupLanternSystem(player);
        
        Debug.Log("✅ Player setup complete!");
    }
    
    private GameObject FindOrCreatePlayer()
    {
        // First try to find existing player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player == null)
        {
            // Try to find by name
            player = GameObject.Find("sword_man") ?? GameObject.Find("Player");
        }
        
        if (player == null && _playerPrefab != null)
        {
            // Create from prefab
            player = Instantiate(_playerPrefab);
            player.name = "Player";
            Debug.Log($"✓ Created player from prefab");
        }
        
        if (player == null)
        {
            // Create basic player GameObject
            player = new GameObject("Player");
            player.tag = "Player";
            Debug.Log($"✓ Created new player GameObject");
        }
        
        return player;
    }
    
    private void SetupRigidbody2D(GameObject player)
    {
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = player.AddComponent<Rigidbody2D>();
            Debug.Log("✓ Added Rigidbody2D");
        }
        
        // Configure rigidbody for platformer movement
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f; // PlayerMovement handles gravity
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        
        Debug.Log("✓ Configured Rigidbody2D for platformer movement");
    }
    
    private void SetupColliders(GameObject player)
    {
        // Setup main body collider
        CapsuleCollider2D bodyCollider = player.GetComponent<CapsuleCollider2D>();
        if (bodyCollider == null)
        {
            bodyCollider = player.AddComponent<CapsuleCollider2D>();
            bodyCollider.size = new Vector2(1f, 2f);
            bodyCollider.offset = new Vector2(0f, 0f);
            Debug.Log("✓ Added main CapsuleCollider2D");
        }
        
        // Setup feet collider (for ground detection)
        Transform feetTransform = player.transform.Find("FeetCollider");
        if (feetTransform == null)
        {
            GameObject feetColliderObj = new GameObject("FeetCollider");
            feetColliderObj.transform.SetParent(player.transform);
            feetColliderObj.transform.localPosition = new Vector3(0f, -1f, 0f);
            
            BoxCollider2D feetCollider = feetColliderObj.AddComponent<BoxCollider2D>();
            feetCollider.size = new Vector2(0.8f, 0.2f);
            feetCollider.isTrigger = true;
            
            Debug.Log("✓ Created feet collider for ground detection");
        }
    }
    
    private void SetupInputSystem(GameObject player)
    {
        // Add PlayerInput component
        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            playerInput = player.AddComponent<PlayerInput>();
            Debug.Log("✓ Added PlayerInput component");
        }
        
        // Assign Input Actions Asset
        if (_inputActionsAsset != null)
        {
            playerInput.actions = _inputActionsAsset;
            playerInput.defaultActionMap = "Player";
            playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
            Debug.Log("✓ Assigned Input Actions Asset");
        }
        else
        {
            Debug.LogWarning("⚠️ No Input Actions Asset assigned! Please assign it in the inspector.");
        }
        
        // Add InputManager component
        InputManager inputManager = player.GetComponent<InputManager>();
        if (inputManager == null)
        {
            inputManager = player.AddComponent<InputManager>();
            Debug.Log("✓ Added InputManager component");
        }
    }
    
    private void SetupMovementSystem(GameObject player)
    {
        // Add PlayerMovement component
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement == null)
        {
            playerMovement = player.AddComponent<PlayerMovement>();
            Debug.Log("✓ Added PlayerMovement component");
        }
        
        // Assign movement stats
        if (_movementStats != null)
        {
            var movementStatsField = typeof(PlayerMovement).GetField("MoveStats");
            if (movementStatsField != null)
            {
                movementStatsField.SetValue(playerMovement, _movementStats);
                Debug.Log("✓ Assigned PlayerMovementStats");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ No PlayerMovementStats assigned! Please assign it in the inspector.");
        }
        
        // Setup collider references in PlayerMovement
        SetupMovementColliderReferences(player, playerMovement);
    }
    
    private void SetupMovementColliderReferences(GameObject player, PlayerMovement playerMovement)
    {
        // Get collider references
        CapsuleCollider2D bodyCollider = player.GetComponent<CapsuleCollider2D>();
        Transform feetTransform = player.transform.Find("FeetCollider");
        BoxCollider2D feetCollider = feetTransform?.GetComponent<BoxCollider2D>();
        
        if (bodyCollider != null && feetCollider != null)
        {
            // Use reflection to set private fields
            var bodyCollField = typeof(PlayerMovement).GetField("_bodyColl", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var feetCollField = typeof(PlayerMovement).GetField("_feetColl", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (bodyCollField != null && feetCollField != null)
            {
                bodyCollField.SetValue(playerMovement, bodyCollider);
                feetCollField.SetValue(playerMovement, feetCollider);
                Debug.Log("✓ Assigned collider references to PlayerMovement");
            }
        }
    }
    
    private void SetupLanternSystem(GameObject player)
    {
        // Add PlayerLanternInventory component
        PlayerLanternInventory lanternInventory = player.GetComponent<PlayerLanternInventory>();
        if (lanternInventory == null)
        {
            lanternInventory = player.AddComponent<PlayerLanternInventory>();
            Debug.Log("✓ Added PlayerLanternInventory component");
        }
        
        // Note: LanternController will be added automatically when player collects a lantern
    }
    
    [ContextMenu("Validate Player Setup")]
    public void ValidatePlayerSetup()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("❌ No player found with 'Player' tag!");
            return;
        }
        
        Debug.Log("🔍 Validating player setup...");
        
        // Check required components
        bool isValid = true;
        
        if (player.GetComponent<Rigidbody2D>() == null)
        {
            Debug.LogError("❌ Missing Rigidbody2D component");
            isValid = false;
        }
        else Debug.Log("✓ Rigidbody2D found");
        
        if (player.GetComponent<PlayerInput>() == null)
        {
            Debug.LogError("❌ Missing PlayerInput component");
            isValid = false;
        }
        else Debug.Log("✓ PlayerInput found");
        
        if (player.GetComponent<InputManager>() == null)
        {
            Debug.LogError("❌ Missing InputManager component");
            isValid = false;
        }
        else Debug.Log("✓ InputManager found");
        
        if (player.GetComponent<PlayerMovement>() == null)
        {
            Debug.LogError("❌ Missing PlayerMovement component");
            isValid = false;
        }
        else Debug.Log("✓ PlayerMovement found");
        
        // Check PlayerInput configuration
        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            if (playerInput.actions == null)
            {
                Debug.LogError("❌ PlayerInput has no Input Actions assigned");
                isValid = false;
            }
            else Debug.Log("✓ Input Actions assigned to PlayerInput");
        }
        
        // Check movement stats
        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            var statsField = typeof(PlayerMovement).GetField("MoveStats");
            if (statsField?.GetValue(movement) == null)
            {
                Debug.LogError("❌ PlayerMovement has no MoveStats assigned");
                isValid = false;
            }
            else Debug.Log("✓ PlayerMovementStats assigned");
        }
        
        if (isValid)
        {
            Debug.Log("✅ Player setup validation passed!");
        }
        else
        {
            Debug.Log("❌ Player setup validation failed. Run 'Setup Player Components' to fix.");
        }
    }
    
    // Auto-find required assets
    private void Start()
    {
        if (_inputActionsAsset == null)
        {
            _inputActionsAsset = Resources.Load<InputActionAsset>("InputSystem_Actions");
            if (_inputActionsAsset == null)
            {
                // Try to find it in the project
                string[] guids = UnityEditor.AssetDatabase.FindAssets("InputSystem_Actions t:InputActionAsset");
                if (guids.Length > 0)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    _inputActionsAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
                }
            }
        }
        
        if (_movementStats == null)
        {
            _movementStats = Resources.Load<PlayerMovementStats>("Player Movement Stats");
            if (_movementStats == null)
            {
                // Try to find it in the project
                string[] guids = UnityEditor.AssetDatabase.FindAssets("Player Movement Stats t:PlayerMovementStats");
                if (guids.Length > 0)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    _movementStats = UnityEditor.AssetDatabase.LoadAssetAtPath<PlayerMovementStats>(path);
                }
            }
        }
    }
}