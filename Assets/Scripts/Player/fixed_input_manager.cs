using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Fixed InputManager that properly handles the new Input System
/// Includes better error handling and debugging capabilities
/// </summary>
public class FixedInputManager : MonoBehaviour
{
    [Header("Debug Info")]
    [SerializeField] private bool _showDebugInfo = true;

    public static PlayerInput PlayerInput;
    public static Animator animator;

    // Movement inputs
    public static Vector2 Movement;
    public static bool JumpWasPressed;
    public static bool JumpIsHeld;
    public static bool JumpWasReleased;
    public static bool RunIsHeld;
    public static bool DashWasPressed;

    // Lantern inputs
    public static bool LanternTogglePressed;
    public static Vector2 MousePosition;
    public static Vector2 RightStickInput;

    // Input Actions - cached for performance
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _runAction;
    private InputAction _dashAction;
    private InputAction _lanternToggleAction;
    private InputAction _mousePositionAction;
    private InputAction _rightStickAction;

    // Debug variables
    private bool _inputSystemInitialized = false;

    private void Awake()
    {
        InitializeInputSystem();
    }

    private void InitializeInputSystem()
    {
        // Get PlayerInput component
        PlayerInput = GetComponent<PlayerInput>();
        if (PlayerInput == null)
        {
            Debug.LogError($"[InputManager] No PlayerInput component found on {gameObject.name}! Input will not work.");
            return;
        }

        // Get Animator if present
        animator = GetComponent<Animator>();

        // Check if actions are assigned
        if (PlayerInput.actions == null)
        {
            Debug.LogError($"[InputManager] No Input Actions assigned to PlayerInput on {gameObject.name}!");
            return;
        }

        // Cache all input actions with error handling
        bool success = CacheInputActions();

        if (success)
        {
            _inputSystemInitialized = true;
            Debug.Log($"[InputManager] ✓ Input system initialized successfully on {gameObject.name}");
        }
        else
        {
            Debug.LogError($"[InputManager] Failed to initialize input system on {gameObject.name}");
        }
    }

    private bool CacheInputActions()
    {
        bool allActionsFound = true;

        // Core movement actions
        _moveAction = PlayerInput.actions["Move"];
        if (_moveAction == null)
        {
            Debug.LogError("[InputManager] 'Move' action not found!");
            allActionsFound = false;
        }

        _jumpAction = PlayerInput.actions["Jump"];
        if (_jumpAction == null)
        {
            Debug.LogError("[InputManager] 'Jump' action not found!");
            allActionsFound = false;
        }

        _runAction = PlayerInput.actions["Run"];
        if (_runAction == null)
        {
            Debug.LogError("[InputManager] 'Run' action not found!");
            allActionsFound = false;
        }

        _dashAction = PlayerInput.actions["Dash"];
        if (_dashAction == null)
        {
            Debug.LogError("[InputManager] 'Dash' action not found!");
            allActionsFound = false;
        }

        // Lantern actions (optional - may not exist in all input action maps)
        try
        {
            _lanternToggleAction = PlayerInput.actions["LanternToggle"];
        }
        catch
        {
            Debug.LogWarning("[InputManager] 'LanternToggle' action not found. Lantern toggle will not work.");
        }

        try
        {
            _mousePositionAction = PlayerInput.actions["MousePosition"];
        }
        catch
        {
            Debug.LogWarning("[InputManager] 'MousePosition' action not found. Using fallback mouse input.");
        }

        try
        {
            _rightStickAction = PlayerInput.actions["RightStick"];
        }
        catch
        {
            Debug.LogWarning("[InputManager] 'RightStick' action not found. Controller lantern aiming won't work.");
        }

        return allActionsFound;
    }

    private void Update()
    {
        if (!_inputSystemInitialized)
        {
            // Try to reinitialize if it failed before
            InitializeInputSystem();
            return;
        }

        UpdateInputValues();

        if (_showDebugInfo)
        {
            DebugInputValues();
        }
    }

    private void UpdateInputValues()
    {
        // Movement input
        Movement = _moveAction?.ReadValue<Vector2>() ?? Vector2.zero;

        // Jump inputs
        JumpWasPressed = _jumpAction?.WasPressedThisFrame() ?? false;
        JumpIsHeld = _jumpAction?.IsPressed() ?? false;
        JumpWasReleased = _jumpAction?.WasReleasedThisFrame() ?? false;

        // Run input
        RunIsHeld = _runAction?.IsPressed() ?? false;

        // Dash input
        DashWasPressed = _dashAction?.WasPressedThisFrame() ?? false;

        // Lantern inputs
        LanternTogglePressed = _lanternToggleAction?.WasPressedThisFrame() ?? false;

        // Mouse position for lantern aiming
        if (_mousePositionAction != null)
        {
            MousePosition = _mousePositionAction.ReadValue<Vector2>();
        }
        else
        {
            MousePosition = Input.mousePosition; // Fallback to old input system
        }

        // Right stick for controller lantern aiming
        RightStickInput = _rightStickAction?.ReadValue<Vector2>() ?? Vector2.zero;
    }

    private void DebugInputValues()
    {
        // Only show debug info if there's actual input
        if (Movement.magnitude > 0.1f || JumpWasPressed || JumpIsHeld || RunIsHeld || DashWasPressed)
        {
            Debug.Log($"[InputManager] Movement: {Movement}, Jump: {JumpIsHeld}, Run: {RunIsHeld}, Dash: {DashWasPressed}");
        }
    }

    // Public method to manually reinitialize input system
    public void ReinitializeInputSystem()
    {
        _inputSystemInitialized = false;
        InitializeInputSystem();
    }

    // Validation method to check if input system is working
    public bool IsInputSystemWorking()
    {
        return _inputSystemInitialized && PlayerInput != null && PlayerInput.actions != null;
    }

    private void OnEnable()
    {
        if (_inputSystemInitialized)
        {
            Debug.Log("[InputManager] Input actions enabled");
        }
    }

    private void OnDisable()
    {
        if (_inputSystemInitialized)
        {
            Debug.Log("[InputManager] Input actions disabled");
        }
    }

    private void OnDestroy()
    {
        // Clean up static references
        PlayerInput = null;
        animator = null;
    }
}