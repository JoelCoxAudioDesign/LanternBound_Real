using UnityEngine;

/// <summary>
/// Fixes integration issues between PlayerMovement and PlayerAnimationController
/// Handles sprite flipping conflicts and ensures movement works with animations
/// </summary>
public class MovementAnimationFix : MonoBehaviour
{
    [Header("Debug Info")]
    [SerializeField] private bool _showMovementDebug = true;
    [SerializeField] private bool _showAnimationDebug = true;

    [Header("Fix Options")]
    [SerializeField] private bool _fixMovementIssue = false;
    [SerializeField] private bool _fixSpriteFlipping = false;
    [SerializeField] private bool _disableAnimationFlipping = false;

    [Header("Current Status")]
    [SerializeField] private bool _playerMovementEnabled;
    [SerializeField] private bool _animationControllerEnabled;
    [SerializeField] private Vector3 _currentScale;
    [SerializeField] private Vector2 _currentVelocity;

    private PlayerMovement _playerMovement;
    private PlayerAnimationController _animationController;
    private Rigidbody2D _rigidbody;
    private SpriteRenderer _spriteRenderer;

    private void OnValidate()
    {
        if (_fixMovementIssue)
        {
            _fixMovementIssue = false;
            FixMovementIssue();
        }

        if (_fixSpriteFlipping)
        {
            _fixSpriteFlipping = false;
            FixSpriteFlipping();
        }

        if (_disableAnimationFlipping)
        {
            _disableAnimationFlipping = false;
            DisableAnimationFlipping();
        }
    }

    private void Awake()
    {
        GetComponents();
    }

    private void GetComponents()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _animationController = GetComponent<PlayerAnimationController>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (_playerMovement == null)
            Debug.LogError("No PlayerMovement component found!");
        if (_animationController == null)
            Debug.LogError("No PlayerAnimationController component found!");
        if (_rigidbody == null)
            Debug.LogError("No Rigidbody2D component found!");
    }

    private void Update()
    {
        UpdateDebugInfo();

        if (_showMovementDebug)
        {
            CheckMovementStatus();
        }
    }

    private void UpdateDebugInfo()
    {
        _playerMovementEnabled = _playerMovement != null && _playerMovement.enabled;
        _animationControllerEnabled = _animationController != null && _animationController.enabled;
        _currentScale = transform.localScale;
        _currentVelocity = _rigidbody != null ? _rigidbody.linearVelocity : Vector2.zero;
    }

    private void CheckMovementStatus()
    {
        Vector2 input = InputManager.Movement;

        if (input.magnitude > 0.1f && _currentVelocity.magnitude < 0.1f)
        {
            Debug.LogWarning("⚠️ Input detected but no movement! Checking for issues...");
            DiagnoseMovementIssue();
        }
    }

    [ContextMenu("Fix Movement Issue")]
    public void FixMovementIssue()
    {
        Debug.Log("🔧 Fixing movement issue...");

        // Check if PlayerMovement is enabled
        if (_playerMovement != null && !_playerMovement.enabled)
        {
            _playerMovement.enabled = true;
            Debug.Log("✓ Enabled PlayerMovement component");
        }

        // Check Rigidbody2D configuration
        if (_rigidbody != null)
        {
            if (_rigidbody.bodyType != RigidbodyType2D.Dynamic)
            {
                _rigidbody.bodyType = RigidbodyType2D.Dynamic;
                Debug.Log("✓ Set Rigidbody2D to Dynamic");
            }

            if (_rigidbody.gravityScale != 0f)
            {
                _rigidbody.gravityScale = 0f;
                Debug.Log("✓ Set Rigidbody2D gravity scale to 0 (PlayerMovement handles gravity)");
            }

            if (!_rigidbody.freezeRotation)
            {
                _rigidbody.freezeRotation = true;
                Debug.Log("✓ Enabled freeze rotation on Rigidbody2D");
            }
        }

        // Check if MoveStats is assigned
        if (_playerMovement != null && _playerMovement.MoveStats == null)
        {
            Debug.LogError("❌ PlayerMovementStats not assigned to PlayerMovement!");
            Debug.Log("Fix: Drag your 'Player Movement Stats' asset to the MoveStats field in PlayerMovement");
        }

        // Check collider assignments
        CheckColliderAssignments();

        Debug.Log("🏁 Movement fix attempt complete");
    }

    private void CheckColliderAssignments()
    {
        if (_playerMovement == null) return;

        // Use reflection to check collider assignments
        var feetCollField = typeof(PlayerMovement).GetField("_feetColl",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var bodyCollField = typeof(PlayerMovement).GetField("_bodyColl",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var feetColl = feetCollField?.GetValue(_playerMovement);
        var bodyColl = bodyCollField?.GetValue(_playerMovement);

        if (feetColl == null)
        {
            Debug.LogError("❌ _feetColl not assigned in PlayerMovement!");
            Debug.Log("Fix: Create a child GameObject with BoxCollider2D (isTrigger=true) for ground detection");
        }

        if (bodyColl == null)
        {
            Debug.LogError("❌ _bodyColl not assigned in PlayerMovement!");
            Debug.Log("Fix: Assign the main CapsuleCollider2D to _bodyColl in PlayerMovement");
        }
    }

    [ContextMenu("Fix Sprite Flipping")]
    public void FixSpriteFlipping()
    {
        Debug.Log("🔧 Fixing sprite flipping...");

        // Reset scale to normal
        transform.localScale = new Vector3(1f, 1f, 1f);
        Debug.Log("✓ Reset transform scale to (1,1,1)");

        // Disable flipping in PlayerAnimationController
        DisableAnimationFlipping();

        // Let PlayerMovement handle flipping instead
        Debug.Log("✓ PlayerMovement will handle sprite flipping");
        Debug.Log("Note: PlayerMovement uses transform.Rotate() for flipping, not scale");
    }

    [ContextMenu("Disable Animation Flipping")]
    public void DisableAnimationFlipping()
    {
        if (_animationController != null)
        {
            // Use reflection to disable flipping in animation controller
            var flipField = typeof(PlayerAnimationController).GetField("_flipSpriteWithMovement",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (flipField != null)
            {
                flipField.SetValue(_animationController, false);
                Debug.Log("✓ Disabled sprite flipping in PlayerAnimationController");
            }
            else
            {
                Debug.LogWarning("Could not find _flipSpriteWithMovement field. Manually set it to false in inspector.");
            }
        }
    }

    [ContextMenu("Diagnose Movement Issue")]
    public void DiagnoseMovementIssue()
    {
        Debug.Log("🔍 Diagnosing movement issue...");

        // Check input
        Vector2 input = InputManager.Movement;
        Debug.Log($"Input: {input}");

        // Check PlayerMovement
        if (_playerMovement == null)
        {
            Debug.LogError("❌ PlayerMovement component missing!");
            return;
        }

        Debug.Log($"PlayerMovement enabled: {_playerMovement.enabled}");
        Debug.Log($"PlayerMovement MoveStats assigned: {_playerMovement.MoveStats != null}");
        Debug.Log($"Current HorizontalVelocity: {_playerMovement.HorizontalVelocity}");
        Debug.Log($"Current VerticalVelocity: {_playerMovement.VerticalVelocity}");

        // Check Rigidbody2D
        if (_rigidbody == null)
        {
            Debug.LogError("❌ Rigidbody2D component missing!");
            return;
        }

        Debug.Log($"Rigidbody2D bodyType: {_rigidbody.bodyType}");
        Debug.Log($"Rigidbody2D gravityScale: {_rigidbody.gravityScale}");
        Debug.Log($"Rigidbody2D freezeRotation: {_rigidbody.freezeRotation}");
        Debug.Log($"Rigidbody2D velocity: {_rigidbody.linearVelocity}");

        // Check if Rigidbody is being overridden by constraints
        if (_rigidbody.constraints != RigidbodyConstraints2D.FreezeRotation)
        {
            Debug.LogWarning($"⚠️ Unusual Rigidbody2D constraints: {_rigidbody.constraints}");
        }

        // Test if PlayerMovement is actually running
        var lastInput = input;
        Debug.Log("Press WASD now and watch console for 3 seconds...");
        StartCoroutine(MonitorInputForSeconds(3f));
    }

    private System.Collections.IEnumerator MonitorInputForSeconds(float seconds)
    {
        float elapsed = 0f;
        Vector2 lastVelocity = Vector2.zero;

        while (elapsed < seconds)
        {
            Vector2 currentInput = InputManager.Movement;
            Vector2 currentVelocity = _rigidbody != null ? _rigidbody.linearVelocity : Vector2.zero;

            if (currentInput.magnitude > 0.1f)
            {
                Debug.Log($"[{elapsed:F1}s] Input: {currentInput}, RB Velocity: {currentVelocity}, PM HVel: {_playerMovement?.HorizontalVelocity:F2}");

                if (currentVelocity.magnitude < 0.1f && lastVelocity.magnitude < 0.1f)
                {
                    Debug.LogError("❌ Input detected but still no movement after 2 frames!");
                }
            }

            lastVelocity = currentVelocity;
            elapsed += Time.deltaTime;
            yield return null;
        }

        Debug.Log("🏁 Movement monitoring complete");
    }

    [ContextMenu("Fix Character Rotation")]
    public void FixCharacterRotation()
    {
        // Reset any weird rotations
        transform.rotation = Quaternion.identity;
        transform.localScale = new Vector3(1f, 1f, 1f);

        Debug.Log("✓ Reset character rotation and scale");
        Debug.Log("PlayerMovement should handle facing direction via Transform.Rotate()");
    }

    private void OnGUI()
    {
        if (!_showMovementDebug && !_showAnimationDebug) return;

        GUILayout.BeginArea(new Rect(Screen.width - 300, 10, 290, 300));
        GUILayout.Box("Movement & Animation Status", GUILayout.Width(280));

        if (_showMovementDebug)
        {
            GUILayout.Label("=== MOVEMENT ===");
            GUILayout.Label($"PlayerMovement: {(_playerMovementEnabled ? "✓" : "❌")}");
            GUILayout.Label($"Input: {InputManager.Movement}");
            GUILayout.Label($"RB Velocity: {_currentVelocity}");
            if (_playerMovement != null)
            {
                GUILayout.Label($"PM HVel: {_playerMovement.HorizontalVelocity:F2}");
                GUILayout.Label($"PM VVel: {_playerMovement.VerticalVelocity:F2}");
            }
        }

        if (_showAnimationDebug)
        {
            GUILayout.Label("=== ANIMATION ===");
            GUILayout.Label($"AnimationController: {(_animationControllerEnabled ? "✓" : "❌")}");
            GUILayout.Label($"Scale: {_currentScale}");
            GUILayout.Label($"Rotation: {transform.rotation.eulerAngles.y:F0}°");
        }

        // Quick fix buttons
        if (GUILayout.Button("Fix Movement"))
        {
            FixMovementIssue();
        }

        if (GUILayout.Button("Fix Sprite Flipping"))
        {
            FixSpriteFlipping();
        }

        GUILayout.EndArea();
    }
}