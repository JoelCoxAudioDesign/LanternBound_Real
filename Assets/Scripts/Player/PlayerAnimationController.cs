using UnityEngine;
using System.Reflection;

/// <summary>
/// Bridges your PlayerMovement system with the Animator Controller
/// Translates movement data into animation parameters
/// </summary>
public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator _animator;
    [SerializeField] private PlayerMovement _playerMovement;

    [Header("Animation Settings")]
    [SerializeField] private float _speedDamping = 0.1f;
    [SerializeField] private float _runSpeedThreshold = 0.5f;
    [SerializeField] private bool _flipSpriteWithMovement = true;
    [SerializeField] private bool _debugAnimationValues = false;

    [Header("Current Animation State - Debug")]
    [SerializeField] private string _currentStateName;
    [SerializeField] private float _currentSpeed;
    [SerializeField] private bool _isGrounded;
    [SerializeField] private bool _isRunning;

    // Cached reflection fields for accessing private PlayerMovement data
    private FieldInfo _isGroundedField;
    private FieldInfo _isFacingRightField;

    private void Awake()
    {
        // Get components
        _animator = _animator ?? GetComponent<Animator>();
        _playerMovement = _playerMovement ?? GetComponent<PlayerMovement>();

        if (_animator == null)
        {
            Debug.LogError("PlayerAnimationController: No Animator component found!");
            enabled = false;
            return;
        }

        if (_playerMovement == null)
        {
            Debug.LogError("PlayerAnimationController: No PlayerMovement component found!");
            enabled = false;
            return;
        }

        // Cache reflection fields for accessing private PlayerMovement data
        CacheReflectionFields();

        Debug.Log("✓ PlayerAnimationController initialized");
    }

    private void CacheReflectionFields()
    {
        // Cache field info for better performance
        _isGroundedField = typeof(PlayerMovement).GetField("_isGrounded",
            BindingFlags.NonPublic | BindingFlags.Instance);

        _isFacingRightField = typeof(PlayerMovement).GetField("_isFacingRight",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (_isGroundedField == null)
            Debug.LogWarning("PlayerAnimationController: Could not find _isGrounded field in PlayerMovement");

        if (_isFacingRightField == null)
            Debug.LogWarning("PlayerAnimationController: Could not find _isFacingRight field in PlayerMovement");
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
        float horizontalVelocity = _playerMovement.HorizontalVelocity;
        float verticalVelocity = _playerMovement.VerticalVelocity;
        bool isGrounded = GetIsGrounded();
        bool isFacingRight = GetIsFacingRight();

        // Calculate animation speed based on actual movement
        float animationSpeed = CalculateAnimationSpeed(horizontalVelocity, isRunning);

        // Store for debug display
        _currentSpeed = animationSpeed;
        _isGrounded = isGrounded;
        _isRunning = isRunning && inputMagnitude > 0.1f;

        // Update animator parameters with safe parameter setting
        SetAnimatorFloat("Speed", animationSpeed);
        SetAnimatorFloat("HorizontalInput", movement.x);
        SetAnimatorFloat("VerticalInput", movement.y);
        SetAnimatorFloat("HorizontalVelocity", horizontalVelocity);
        SetAnimatorFloat("VerticalVelocity", verticalVelocity);

        SetAnimatorBool("IsGrounded", isGrounded);
        SetAnimatorBool("IsRunning", _isRunning);
        SetAnimatorBool("IsMoving", inputMagnitude > 0.1f);
        SetAnimatorBool("IsFacingRight", isFacingRight);

        // Handle triggers
        if (jumpPressed && isGrounded)
        {
            SetAnimatorTrigger("JumpTrigger");
            SetAnimatorTrigger("Jump");
            SetAnimatorTrigger("DoJump");
        }

        // Handle sprite flipping
        if (_flipSpriteWithMovement)
        {
            HandleSpriteFlipping(movement.x);
        }
    }

    private float CalculateAnimationSpeed(float horizontalVelocity, bool isRunning)
    {
        if (_playerMovement.MoveStats == null) return 0f;

        float absVelocity = Mathf.Abs(horizontalVelocity);

        if (absVelocity < 0.1f)
        {
            return 0f; // Idle
        }

        // Normalize speed based on movement stats
        float maxSpeed = isRunning ? _playerMovement.MoveStats.MaxRunSpeed : _playerMovement.MoveStats.MaxWalkSpeed;
        float normalizedSpeed = absVelocity / maxSpeed;

        // Return 1.0 for walk, 2.0 for run (you can adjust these multipliers)
        return isRunning ? Mathf.Clamp(normalizedSpeed * 2f, 0f, 2f) : Mathf.Clamp(normalizedSpeed, 0f, 1f);
    }

    private bool GetIsGrounded()
    {
        if (_isGroundedField != null)
        {
            try
            {
                return (bool)_isGroundedField.GetValue(_playerMovement);
            }
            catch
            {
                Debug.LogWarning("Failed to get grounded state from PlayerMovement");
            }
        }

        // Fallback: assume grounded if vertical velocity is near zero
        return Mathf.Abs(_playerMovement.VerticalVelocity) < 0.1f;
    }

    private bool GetIsFacingRight()
    {
        if (_isFacingRightField != null)
        {
            try
            {
                return (bool)_isFacingRightField.GetValue(_playerMovement);
            }
            catch
            {
                Debug.LogWarning("Failed to get facing direction from PlayerMovement");
            }
        }

        // Fallback: use horizontal velocity direction
        return _playerMovement.HorizontalVelocity >= 0f;
    }

    private void HandleSpriteFlipping(float horizontalInput)
    {
        if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            Vector3 scale = transform.localScale;
            scale.x = horizontalInput > 0 ? 1f : -1f;
            transform.localScale = scale;
        }
    }

    // Safe parameter setting methods with error handling
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

    // Debug GUI for runtime monitoring
    private void OnGUI()
    {
        if (!_debugAnimationValues || _animator == null) return;

        GUILayout.BeginArea(new Rect(10, 10, 250, 200));
        GUILayout.Box("Animation Debug", GUILayout.Width(240));

        GUILayout.Label($"Current State: {_currentStateName}");
        GUILayout.Label($"Speed Parameter: {_currentSpeed:F2}");
        GUILayout.Label($"Is Grounded: {_isGrounded}");
        GUILayout.Label($"Is Running: {_isRunning}");
        GUILayout.Label($"Horizontal Velocity: {_playerMovement.HorizontalVelocity:F2}");
        GUILayout.Label($"Vertical Velocity: {_playerMovement.VerticalVelocity:F2}");

        // Show input
        Vector2 input = InputManager.Movement;
        GUILayout.Label($"Input: ({input.x:F1}, {input.y:F1})");

        GUILayout.EndArea();
    }

    // Context menu methods for testing
    [ContextMenu("Test Idle Animation")]
    public void TestIdleAnimation()
    {
        SetAnimatorFloat("Speed", 0f);
        SetAnimatorBool("IsMoving", false);
    }

    [ContextMenu("Test Run Animation")]
    public void TestRunAnimation()
    {
        SetAnimatorFloat("Speed", 1.5f);
        SetAnimatorBool("IsMoving", true);
        SetAnimatorBool("IsRunning", true);
    }

    [ContextMenu("Test Jump Animation")]
    public void TestJumpAnimation()
    {
        SetAnimatorTrigger("JumpTrigger");
    }

    [ContextMenu("Log Available Parameters")]
    public void LogAvailableParameters()
    {
        if (_animator == null) return;

        Debug.Log("🎭 Available Animator Parameters:");
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
    }

    // Animation event receivers (call these from animation events)
    public void OnJumpStart()
    {
        Debug.Log("Jump animation started");
    }

    public void OnJumpApex()
    {
        Debug.Log("Jump apex reached");
    }

    public void OnJumpLanding()
    {
        Debug.Log("Jump landing");
        SetAnimatorBool("IsGrounded", true);
    }

    public void OnAttackStart()
    {
        Debug.Log("Attack animation started");
    }

    public void OnAttackHit()
    {
        Debug.Log("Attack hit point");
    }

    public void OnAttackEnd()
    {
        Debug.Log("Attack animation ended");
    }
}