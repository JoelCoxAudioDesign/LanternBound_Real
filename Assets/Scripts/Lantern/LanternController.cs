using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanternController : MonoBehaviour
{
    [Header("Lantern Settings")]
    [SerializeField] private float _beamRange = 10f;
    [SerializeField] private float _beamWidth = 30f; // Angle in degrees
    [SerializeField] private LayerMask _lightInteractionLayers = -1;
    [SerializeField] private int _raycastResolution = 20; // Number of rays for beam detection

    [Header("Input Settings")]
    [SerializeField] private bool _useMouseAiming = true;
    [SerializeField] private string _aimHorizontalAxis = "RightStickX";
    [SerializeField] private string _aimVerticalAxis = "RightStickY";

    [Header("Visual Settings")]
    [SerializeField] private LineRenderer _beamVisual;
    [SerializeField] private Light _lightSource;
    [SerializeField] private Color _beamColor = Color.yellow;
    [SerializeField] private float _beamIntensity = 1f;

    [Header("Debug")]
    [SerializeField] private bool _showDebugRays = false;

    // Properties
    public bool IsLanternActive { get; private set; }
    public Vector2 BeamDirection { get; private set; }
    public List<ILightInteractable> CurrentlyIlluminated { get; private set; }

    // Events
    public System.Action<ILightInteractable> OnObjectIlluminated;
    public System.Action<ILightInteractable> OnObjectLeftLight;
    public System.Action<Vector2> OnBeamDirectionChanged;

    // Private variables
    private Camera _playerCamera;
    private List<ILightInteractable> _previouslyIlluminated;
    private Vector2 _lastBeamDirection;

    private void Awake()
    {
        CurrentlyIlluminated = new List<ILightInteractable>();
        _previouslyIlluminated = new List<ILightInteractable>();

        _playerCamera = Camera.main;
        if (_playerCamera == null)
            _playerCamera = FindFirstObjectByType<Camera>();

        SetupVisuals();
    }

    private void Start()
    {
        // Start with lantern active
        ActivateLantern();
    }

    private void Update()
    {
        if (IsLanternActive)
        {
            UpdateBeamDirection();
            PerformLightDetection();
            UpdateVisuals();
        }
    }

    private void SetupVisuals()
    {
        // Setup LineRenderer if not assigned
        if (_beamVisual == null)
        {
            _beamVisual = GetComponent<LineRenderer>();
            if (_beamVisual == null)
            {
                GameObject beamObject = new GameObject("BeamVisual");
                beamObject.transform.SetParent(transform);
                _beamVisual = beamObject.AddComponent<LineRenderer>();
            }
        }

        if (_beamVisual != null)
        {
            _beamVisual.material = new Material(Shader.Find("Sprites/Default"));
            _beamVisual.material.color = _beamColor;
            _beamVisual.startWidth = 0.1f;
            _beamVisual.endWidth = 0.5f;
            _beamVisual.positionCount = 2;
        }

        // Setup Light component if not assigned
        if (_lightSource == null)
        {
            _lightSource = GetComponent<Light>();
            if (_lightSource == null)
            {
                _lightSource = gameObject.AddComponent<Light>();
            }
        }

        if (_lightSource != null)
        {
            _lightSource.type = LightType.Spot;
            _lightSource.color = _beamColor;
            _lightSource.intensity = _beamIntensity;
            _lightSource.range = _beamRange;
            _lightSource.spotAngle = _beamWidth;
        }
    }

    public void ActivateLantern()
    {
        IsLanternActive = true;
        if (_beamVisual != null) _beamVisual.enabled = true;
        if (_lightSource != null) _lightSource.enabled = true;
    }

    public void DeactivateLantern()
    {
        IsLanternActive = false;
        if (_beamVisual != null) _beamVisual.enabled = false;
        if (_lightSource != null) _lightSource.enabled = false;

        // Clear all illuminated objects
        ClearIlluminatedObjects();
    }

    public void ToggleLantern()
    {
        if (IsLanternActive)
            DeactivateLantern();
        else
            ActivateLantern();
    }

    private void UpdateBeamDirection()
    {
        Vector2 newDirection;

        if (_useMouseAiming && _playerCamera != null)
        {
            // Mouse aiming - convert screen position to world direction
            Vector3 mouseWorldPos = _playerCamera.ScreenToWorldPoint(new Vector3(InputManager.MousePosition.x, InputManager.MousePosition.y, _playerCamera.nearClipPlane));
            mouseWorldPos.z = 0f;
            newDirection = ((Vector2)mouseWorldPos - (Vector2)transform.position).normalized;
        }
        else
        {
            // Controller aiming using right stick
            Vector2 stickInput = InputManager.RightStickInput;

            if (stickInput.magnitude > 0.1f) // Dead zone
            {
                newDirection = stickInput.normalized;
            }
            else
            {
                // If no input, keep current direction or default to right
                newDirection = BeamDirection.magnitude > 0 ? BeamDirection : Vector2.right;
            }
        }

        // Only update if direction changed significantly
        if (Vector2.Distance(newDirection, _lastBeamDirection) > 0.01f)
        {
            BeamDirection = newDirection;
            _lastBeamDirection = newDirection;
            OnBeamDirectionChanged?.Invoke(BeamDirection);
        }
    }

    private void PerformLightDetection()
    {
        // Clear previous frame's illuminated objects
        _previouslyIlluminated.Clear();
        _previouslyIlluminated.AddRange(CurrentlyIlluminated);
        CurrentlyIlluminated.Clear();

        // Calculate beam cone
        float halfAngle = _beamWidth * 0.5f;
        Vector2 startPosition = transform.position;

        // Cast multiple rays within the beam cone
        for (int i = 0; i < _raycastResolution; i++)
        {
            float t = (float)i / (_raycastResolution - 1);
            float currentAngle = Mathf.Lerp(-halfAngle, halfAngle, t);

            // Rotate beam direction by current angle
            Vector2 rayDirection = RotateVector2(BeamDirection, currentAngle);

            // Perform raycast
            RaycastHit2D hit = Physics2D.Raycast(startPosition, rayDirection, _beamRange, _lightInteractionLayers);

            if (_showDebugRays)
            {
                Debug.DrawRay(startPosition, rayDirection * _beamRange, hit.collider != null ? Color.green : Color.red);
            }

            if (hit.collider != null)
            {
                ILightInteractable lightInteractable = hit.collider.GetComponent<ILightInteractable>();
                if (lightInteractable != null && !CurrentlyIlluminated.Contains(lightInteractable))
                {
                    CurrentlyIlluminated.Add(lightInteractable);
                }
            }
        }

        // Handle illumination events
        ProcessIlluminationEvents();
    }

    private void ProcessIlluminationEvents()
    {
        // Check for newly illuminated objects
        foreach (var illuminated in CurrentlyIlluminated)
        {
            if (!_previouslyIlluminated.Contains(illuminated))
            {
                illuminated.OnIlluminated(this);
                OnObjectIlluminated?.Invoke(illuminated);
            }
        }

        // Check for objects that left the light
        foreach (var previously in _previouslyIlluminated)
        {
            if (!CurrentlyIlluminated.Contains(previously))
            {
                previously.OnLeftLight(this);
                OnObjectLeftLight?.Invoke(previously);
            }
        }
    }

    private void UpdateVisuals()
    {
        if (_beamVisual != null)
        {
            Vector3 startPos = transform.position;
            Vector3 endPos = startPos + (Vector3)(BeamDirection * _beamRange);

            _beamVisual.SetPosition(0, startPos);
            _beamVisual.SetPosition(1, endPos);
        }

        if (_lightSource != null)
        {
            // Update light direction
            float angle = Mathf.Atan2(BeamDirection.y, BeamDirection.x) * Mathf.Rad2Deg;
            _lightSource.transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
        }
    }

    private void ClearIlluminatedObjects()
    {
        foreach (var illuminated in CurrentlyIlluminated)
        {
            illuminated.OnLeftLight(this);
            OnObjectLeftLight?.Invoke(illuminated);
        }
        CurrentlyIlluminated.Clear();
    }

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

    // Public methods for external control
    public void SetBeamRange(float range)
    {
        _beamRange = range;
        if (_lightSource != null)
            _lightSource.range = range;
    }

    public void SetBeamWidth(float width)
    {
        _beamWidth = width;
        if (_lightSource != null)
            _lightSource.spotAngle = width;
    }

    public void SetBeamColor(Color color)
    {
        _beamColor = color;
        if (_beamVisual != null && _beamVisual.material != null)
            _beamVisual.material.color = color;
        if (_lightSource != null)
            _lightSource.color = color;
    }

    public bool IsObjectIlluminated(ILightInteractable obj)
    {
        return CurrentlyIlluminated.Contains(obj);
    }

    private void OnDrawGizmosSelected()
    {
        if (!IsLanternActive) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.2f);

        // Draw beam cone
        float halfAngle = _beamWidth * 0.5f;
        Vector3 startPos = transform.position;

        Vector3 rightEdge = RotateVector2(BeamDirection, halfAngle) * _beamRange;
        Vector3 leftEdge = RotateVector2(BeamDirection, -halfAngle) * _beamRange;

        Gizmos.DrawLine(startPos, startPos + rightEdge);
        Gizmos.DrawLine(startPos, startPos + leftEdge);
        Gizmos.DrawLine(startPos + rightEdge, startPos + leftEdge);
    }
}