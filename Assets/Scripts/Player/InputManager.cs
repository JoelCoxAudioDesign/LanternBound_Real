using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static PlayerInput PlayerInput;

    public static Vector2 Movement;
    public static bool JumpWasPressed;
    public static bool JumpIsHeld;
    public static bool JumpWasReleased;
    public static bool RunIsHeld;
    public static bool DashWasPressed;

    public static bool LanternTogglePressed;
    public static Vector2 LanternAimDirection;

    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _runAction;
    private InputAction _dashAction;
    private InputAction _lanternToggleAction;
    private InputAction _lanternAimAction;

    private void Awake()
    {
        PlayerInput = GetComponent<PlayerInput>();

        _moveAction = PlayerInput.actions["Move"];
        _jumpAction = PlayerInput.actions["Jump"];
        _runAction = PlayerInput.actions["Run"];
        _dashAction = PlayerInput.actions["Dash"];

        _lanternToggleAction = PlayerInput.actions["LanternToggle"];
        _lanternAimAction = PlayerInput.actions["LanternAim"];
    }

    private void Update()
    {
        Movement = _moveAction.ReadValue<Vector2>();

        JumpWasPressed = _jumpAction.WasPressedThisFrame();
        JumpIsHeld = _jumpAction.IsPressed();
        JumpWasReleased = _jumpAction.WasReleasedThisFrame();

        RunIsHeld = _runAction.IsPressed();
        DashWasPressed = _dashAction.WasPressedThisFrame();

        LanternTogglePressed = _lanternToggleAction.WasPressedThisFrame();
        LanternAimDirection = _lanternAimAction.ReadValue<Vector2>();
    }
}
