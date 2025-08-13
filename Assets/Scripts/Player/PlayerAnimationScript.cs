using UnityEngine;

/// <summary>
/// Fixed animation controller that properly reads grounding state
/// Works with both the original PlayerMovement and PlayerMovement
/// </summary>
public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator _animator;
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private PlayerMovement _PlayerMovement; // For the fixed version

    [Header("Animation Settings")]
    [SerializeField] private float _speedDamping = 5f;
    [SerializeField] private float _walkSpeedThreshold = 0.1f;
    [SerializeField] private bool _debugAnimationValues = false;

    [Header("Current Animation State - Debug")]
    [SerializeField] private string _currentStateName;
    [SerializeField] private float _currentSpeed;
    [SerializeField] private bool _isGrounded = true;
    [SerializeField] private bool _isRunning;

    // Cached reflection fields for original PlayerMovement
    private System.Reflection.FieldInfo _isGroundedField;
    private bool _hasGroundedField = false;

    private void Awake()
    {
        // Get components
        _animator = _animator ?? GetComponent<Animator>();

        // Try to get either version of player movement
        if (_playerMovement == null)
            _playerMovement = GetComponent<PlayerMovement>();

        if (_PlayerMovement == null)
            _PlayerMovement = GetComponent<PlayerMovement>();

        if (_animator == null)
        {
            Debug.LogError("FixedPlayerAnimationController: No Animator component found!");
            enabled = false;
            return;
        }

        if (_playerMovement == null && _PlayerMovement == null)
        {
            Debug.LogError("FixedPlayerAnimationController: No PlayerMovement component found!");
            enabled = false;
            return;
        }

        // Only use reflection if we're using the original PlayerMovement
        if (_playerMovement != null && _PlayerMovement == null)
        {
            TryToFindGroundedField();
        }

        Debug.Log("✓ FixedPlayerAnimationController initialized");
    }

    private void TryToFindGroundedField()
    {
        // Try common field names for grounded state
        string[] possibleFieldNames = { "_isGrounded", "isGrounded", "_grounded", "grounded" };

        foreach (string fieldName in possibleFieldNames)
        {
            _isGroundedField = typeof(PlayerMovement).GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            if (_isGroundedField != null)
            {
                _hasGroundedField = true;
                Debug.Log($"✓ Found grounded field: {fieldName}");
                return;
            }
        }

        Debug.LogWarning("Could not find grounded field in PlayerMovement. Will use fallback method.");
        _hasGroundedField = false;
    }

    private void Update()
    {
        UpdateAnimatorParameters();

        if (_debugAnimationValues)
        {
            UpdateDebugInfo();
        }
    }

    private void UpdateAnimatorParameters()
    {
        // Get input data
        Vector2 movement = InputManager.Movement;
        float inputMagnitude = movement.magnitude;
        bool isRunning = InputManager.RunIsHeld;
        bool jumpPressed = InputManager.JumpWasPressed;

        // Get PlayerMovement data
        float horizontalVelocity = GetHorizontalVelocity();
        float verticalVelocity = GetVerticalVelocity();
        bool isGrounded = GetIsGrounded();

        // Calculate animation speed
        float targetSpeed = CalculateAnimationSpeed(inputMagnitude, isRunning);

        // Smooth the speed transition
        _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, Time.deltaTime * _speedDamping);

        // Store for debug display
        _isGrounded = isGrounded;
        _isRunning = isRunning && inputMagnitude > _walkSpeedThreshold;

        // Update animator parameters safely
        SetAnimatorFloat("Speed", _currentSpeed);
        SetAnimatorFloat("HorizontalInput", movement.x);
        SetAnimatorFloat("VerticalInput", movement.y);
        SetAnimatorFloat("HorizontalVelocity", horizontalVelocity);
        SetAnimatorFloat("VerticalVelocity", verticalVelocity);

        SetAnimatorBool("IsGrounded", isGrounded);
        SetAnimatorBool("IsRunning", _isRunning);
        SetAnimatorBool("IsMoving", inputMagnitude > _walkSpeedThreshold);

        // Handle triggers
        if (jumpPressed && isGrounded)
        {
            SetAnimatorTrigger("JumpTrigger");
            SetAnimatorTrigger("Jump");
            SetAnimatorTrigger("DoJump");
        }

        // Debug output
        if (_debugAnimationValues && (_currentSpeed > 0.1f || jumpPressed))
        {
            Debug.Log($"Animation Update - Speed: {_currentSpeed:F2}, IsGrounded: {isGrounded}, IsMoving: {inputMagnitude > _walkSpeedThreshold}, Input: {inputMagnitude:F2}");
        }
    }

    private float GetHorizontalVelocity()
    {
        if (_PlayerMovement != null)
            return _PlayerMovement.HorizontalVelocity;
        else if (_playerMovement != null)
            return _playerMovement.HorizontalVelocity;
        return 0f;
    }

    private float GetVerticalVelocity()
    {
        if (_PlayerMovement != null)
            return _PlayerMovement.VerticalVelocity;
        else if (_playerMovement != null)
            return _playerMovement.VerticalVelocity;
        return 0f;
    }

    private float CalculateAnimationSpeed(float inputMagnitude, bool isRunning)
    {
        if (inputMagnitude < _walkSpeedThreshold)
        {
            return 0f; // Idle
        }

        // Return appropriate speed based on input and running state
        if (isRunning)
        {
            return 2f; // Run speed
        }
        else
        {
            return 1f; // Walk speed
        }
    }

    private bool GetIsGrounded()
    {
        // Method 1: Use PlayerMovement if available (preferred)
        if (_PlayerMovement != null)
        {
            return _PlayerMovement._isGrounded;
        }

        // Method 2: Try to get grounded state from original PlayerMovement using reflection
        if (_playerMovement != null && _hasGroundedField && _isGroundedField != null)
        {
            try
            {
                return (bool)_isGroundedField.GetValue(_playerMovement);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to get grounded state: {e.Message}");
                _hasGroundedField = false; // Disable further attempts
            }
        }

        // Method 3: Fallback method - check if vertical velocity is near zero and not jumping
        float verticalVelocity = GetVerticalVelocity();
        return Mathf.Abs(verticalVelocity) < 2f; // Allow some tolerance for ground detection
    }

    // Safe parameter setting methods
    private void SetAnimatorFloat(string paramName, float value)
    {
        if (_animator == null) return;

        try
        {
            if (HasParameter(paramName, AnimatorControllerParameterType.Float))
            {
                _animator.SetFloat(paramName, value);
            }
        }
        catch (System.Exception e)
        {
            if (_debugAnimationValues)
                Debug.LogWarning($"Failed to set float parameter '{paramName}': {e.Message}");
        }
    }

    private void SetAnimatorBool(string paramName, bool value)
    {
        if (_animator == null) return;

        try
        {
            if (HasParameter(paramName, AnimatorControllerParameterType.Bool))
            {
                _animator.SetBool(paramName, value);
            }
        }
        catch (System.Exception e)
        {
            if (_debugAnimationValues)
                Debug.LogWarning($"Failed to set bool parameter '{paramName}': {e.Message}");
        }
    }

    private void SetAnimatorTrigger(string paramName)
    {
        if (_animator == null) return;

        try
        {
            if (HasParameter(paramName, AnimatorControllerParameterType.Trigger))
            {
                _animator.SetTrigger(paramName);
                if (_debugAnimationValues)
                    Debug.Log($"Triggered: {paramName}");
            }
        }
        catch (System.Exception e)
        {
            if (_debugAnimationValues)
                Debug.LogWarning($"Failed to set trigger parameter '{paramName}': {e.Message}");
        }
    }

    private bool HasParameter(string paramName, AnimatorControllerParameterType type)
    {
        if (_animator == null) return false;

        foreach (AnimatorControllerParameter param in _animator.parameters)
        {
            if (param.name == paramName && param.type == type)
                return true;
        }
        return false;
    }

    private void UpdateDebugInfo()
    {
        if (_animator == null) return;

        var currentState = _animator.GetCurrentAnimatorStateInfo(0);

        if (currentState.IsName("Idle"))
            _currentStateName = "Idle";
        else if (currentState.IsName("Run"))
            _currentStateName = "Run";
        else if (currentState.IsName("Jump"))
            _currentStateName = "Jump";
        else if (currentState.IsName("Attack"))
            _currentStateName = "Attack";
        else
            _currentStateName = $"State Hash: {currentState.shortNameHash}";
    }

    // Context menu methods for testing
    [ContextMenu("Test Idle Animation")]
    public void TestIdleAnimation()
    {
        SetAnimatorFloat("Speed", 0f);
        SetAnimatorBool("IsMoving", false);
        Debug.Log("Forced Idle animation");
    }

    [ContextMenu("Test Walk Animation")]
    public void TestWalkAnimation()
    {
        SetAnimatorFloat("Speed", 1f);
        SetAnimatorBool("IsMoving", true);
        SetAnimatorBool("IsRunning", false);
        Debug.Log("Forced Walk animation");
    }

    [ContextMenu("Test Run Animation")]
    public void TestRunAnimation()
    {
        SetAnimatorFloat("Speed", 2f);
        SetAnimatorBool("IsMoving", true);
        SetAnimatorBool("IsRunning", true);
        Debug.Log("Forced Run animation");
    }

    [ContextMenu("Test Grounded State")]
    public void TestGroundedState()
    {
        bool isGrounded = GetIsGrounded();
        Debug.Log($"Current Grounded State: {isGrounded}");

        if (_PlayerMovement != null)
            Debug.Log($"PlayerMovement.IsGrounded: {_PlayerMovement._isGrounded}");

        if (_playerMovement != null)
            Debug.Log($"PlayerMovement Vertical Velocity: {_playerMovement.VerticalVelocity}");
    }

    [ContextMenu("Check Animator Parameters")]
    public void CheckAnimatorParameters()
    {
        if (_animator == null) return;

        Debug.Log("🎭 Current Animator Parameters:");
        foreach (AnimatorControllerParameter param in _animator.parameters)
        {
            string currentValue = "";
            try
            {
                switch (param.type)
                {
                    case AnimatorControllerParameterType.Float:
                        currentValue = _animator.GetFloat(param.name).ToString("F2");
                        break;
                    case AnimatorControllerParameterType.Bool:
                        currentValue = _animator.GetBool(param.name).ToString();
                        break;
                    case AnimatorControllerParameterType.Int:
                        currentValue = _animator.GetInteger(param.name).ToString();
                        break;
                    case AnimatorControllerParameterType.Trigger:
                        currentValue = "(trigger)";
                        break;
                }
            }
            catch
            {
                currentValue = "(error)";
            }

            Debug.Log($"  - {param.name} ({param.type}): {currentValue}");
        }

        // Check transition conditions
        Debug.Log("");
        Debug.Log("🔍 Checking critical parameters:");

        bool hasSpeed = HasParameter("Speed", AnimatorControllerParameterType.Float);
        bool hasIsMoving = HasParameter("IsMoving", AnimatorControllerParameterType.Bool);
        bool hasIsGrounded = HasParameter("IsGrounded", AnimatorControllerParameterType.Bool);

        Debug.Log($"Speed parameter: {(hasSpeed ? "✓" : "❌")}");
        Debug.Log($"IsMoving parameter: {(hasIsMoving ? "✓" : "❌")}");
        Debug.Log($"IsGrounded parameter: {(hasIsGrounded ? "✓" : "❌")}");

        if (!hasSpeed)
        {
            Debug.LogError("❌ Missing 'Speed' parameter! Add it to your Animator Controller.");
        }

        if (!hasIsGrounded)
        {
            Debug.LogError("❌ Missing 'IsGrounded' parameter! Add it to your Animator Controller.");
        }
    }

    private void OnGUI()
    {
        if (!_debugAnimationValues) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 250));
        GUILayout.Box("Animation Debug (Fixed)", GUILayout.Width(290));

        GUILayout.Label($"Current State: {_currentStateName}");
        GUILayout.Label($"Speed Parameter: {_currentSpeed:F2}");
        GUILayout.Label($"Is Grounded: {_isGrounded}");
        GUILayout.Label($"Is Running: {_isRunning}");

        // Show input
        Vector2 input = InputManager.Movement;
        GUILayout.Label($"Input Magnitude: {input.magnitude:F2}");
        GUILayout.Label($"Above Walk Threshold: {input.magnitude > _walkSpeedThreshold}");

        // Show which movement script is active
        if (_PlayerMovement != null)
        {
            GUILayout.Label($"Using: PlayerMovement");
            GUILayout.Label($"H Velocity: {_PlayerMovement.HorizontalVelocity:F2}");
            GUILayout.Label($"V Velocity: {_PlayerMovement.VerticalVelocity:F2}");
            GUILayout.Label($"Fixed IsGrounded: {_PlayerMovement._isGrounded}");
        }
        else if (_playerMovement != null)
        {
            GUILayout.Label($"Using: Original PlayerMovement");
            GUILayout.Label($"H Velocity: {_playerMovement.HorizontalVelocity:F2}");
            GUILayout.Label($"V Velocity: {_playerMovement.VerticalVelocity:F2}");
        }

        if (GUILayout.Button("Force Jump Trigger"))
        {
            SetAnimatorTrigger("JumpTrigger");
        }

        if (GUILayout.Button("Test Grounded Detection"))
        {
            TestGroundedState();
        }

        GUILayout.EndArea();
    }
}