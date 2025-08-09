using UnityEngine;

/// <summary>
/// Fixes sprite layer z-positioning issues when character flips
/// Specifically handles head and helmet components that have z-axis positioning
/// </summary>
public class SpriteLayerFix : MonoBehaviour
{
    [Header("Sprite Layer Configuration")]
    [SerializeField] private Transform[] _headComponents;
    [SerializeField] private Transform[] _helmetComponents;
    [SerializeField] private Transform[] _otherLayeredComponents;

    [Header("Z-Position Settings")]
    [SerializeField] private float _headZPositionFacingLeft = 0f;
    [SerializeField] private float _headZPositionFacingRight = 0f;
    [SerializeField] private float _helmetZPositionFacingLeft = 0f;
    [SerializeField] private float _helmetZPositionFacingRight = 0f;

    [Header("Auto-Detection")]
    [SerializeField] private bool _autoDetectHeadComponents = true;
    [SerializeField] private string[] _headComponentNames = { "head", "Head", "HEAD" };
    [SerializeField] private string[] _helmetComponentNames = { "helmet", "Helmet", "HELMET", "hat", "Hat" };

    [Header("Debug")]
    [SerializeField] private bool _showDebugInfo = false;
    [SerializeField] private bool _isFacingRight = true;

    private void Awake()
    {
        if (_autoDetectHeadComponents)
        {
            AutoDetectComponents();
        }

        // Store initial z-positions based on current facing direction
        StoreInitialZPositions();
    }

    private void AutoDetectComponents()
    {
        Debug.Log("🔍 Auto-detecting head and helmet components...");

        // Find all child transforms
        Transform[] allChildren = GetComponentsInChildren<Transform>();

        System.Collections.Generic.List<Transform> headList = new System.Collections.Generic.List<Transform>();
        System.Collections.Generic.List<Transform> helmetList = new System.Collections.Generic.List<Transform>();

        foreach (Transform child in allChildren)
        {
            if (child == transform) continue; // Skip self

            string childName = child.name.ToLower();

            // Check for head components
            foreach (string headName in _headComponentNames)
            {
                if (childName.Contains(headName.ToLower()))
                {
                    headList.Add(child);
                    Debug.Log($"✓ Found head component: {child.name}");
                    break;
                }
            }

            // Check for helmet components
            foreach (string helmetName in _helmetComponentNames)
            {
                if (childName.Contains(helmetName.ToLower()))
                {
                    helmetList.Add(child);
                    Debug.Log($"✓ Found helmet component: {child.name}");
                    break;
                }
            }
        }

        _headComponents = headList.ToArray();
        _helmetComponents = helmetList.ToArray();

        Debug.Log($"Auto-detection complete: {_headComponents.Length} head components, {_helmetComponents.Length} helmet components");
    }

    private void StoreInitialZPositions()
    {
        // Store the current z-positions as the "facing right" positions
        if (_headComponents != null && _headComponents.Length > 0)
        {
            _headZPositionFacingRight = _headComponents[0].localPosition.z;
            Debug.Log($"Stored head z-position for facing right: {_headZPositionFacingRight}");
        }

        if (_helmetComponents != null && _helmetComponents.Length > 0)
        {
            _helmetZPositionFacingRight = _helmetComponents[0].localPosition.z;
            Debug.Log($"Stored helmet z-position for facing right: {_helmetZPositionFacingRight}");
        }
    }

    /// <summary>
    /// Call this method when the character turns to fix z-positioning
    /// </summary>
    /// <param name="facingRight">True if character is now facing right</param>
    public void OnCharacterTurn(bool facingRight)
    {
        _isFacingRight = facingRight;

        if (_showDebugInfo)
        {
            Debug.Log($"Character turned, now facing: {(facingRight ? "Right" : "Left")}");
        }

        FixHeadPositions(facingRight);
        FixHelmetPositions(facingRight);
        FixOtherComponentPositions(facingRight);
    }

    private void FixHeadPositions(bool facingRight)
    {
        if (_headComponents == null) return;

        float targetZ = facingRight ? _headZPositionFacingRight : _headZPositionFacingLeft;

        foreach (Transform headComponent in _headComponents)
        {
            if (headComponent == null) continue;

            Vector3 pos = headComponent.localPosition;
            pos.z = targetZ;
            headComponent.localPosition = pos;

            if (_showDebugInfo)
            {
                Debug.Log($"Fixed {headComponent.name} z-position to {targetZ}");
            }
        }
    }

    private void FixHelmetPositions(bool facingRight)
    {
        if (_helmetComponents == null) return;

        float targetZ = facingRight ? _helmetZPositionFacingRight : _helmetZPositionFacingLeft;

        foreach (Transform helmetComponent in _helmetComponents)
        {
            if (helmetComponent == null) continue;

            Vector3 pos = helmetComponent.localPosition;
            pos.z = targetZ;
            helmetComponent.localPosition = pos;

            if (_showDebugInfo)
            {
                Debug.Log($"Fixed {helmetComponent.name} z-position to {targetZ}");
            }
        }
    }

    private void FixOtherComponentPositions(bool facingRight)
    {
        if (_otherLayeredComponents == null) return;

        foreach (Transform component in _otherLayeredComponents)
        {
            if (component == null) continue;

            // For other components, you might want to flip their z-position
            Vector3 pos = component.localPosition;
            pos.z = facingRight ? Mathf.Abs(pos.z) : -Mathf.Abs(pos.z);
            component.localPosition = pos;

            if (_showDebugInfo)
            {
                Debug.Log($"Fixed {component.name} z-position to {pos.z}");
            }
        }
    }

    [ContextMenu("Auto-Detect Components")]
    public void AutoDetectComponentsManual()
    {
        AutoDetectComponents();
    }

    [ContextMenu("Test Turn Right")]
    public void TestTurnRight()
    {
        OnCharacterTurn(true);
    }

    [ContextMenu("Test Turn Left")]
    public void TestTurnLeft()
    {
        OnCharacterTurn(false);
    }

    [ContextMenu("Store Current Z-Positions")]
    public void StoreCurrentZPositions()
    {
        if (_headComponents != null && _headComponents.Length > 0)
        {
            if (_isFacingRight)
                _headZPositionFacingRight = _headComponents[0].localPosition.z;
            else
                _headZPositionFacingLeft = _headComponents[0].localPosition.z;
        }

        if (_helmetComponents != null && _helmetComponents.Length > 0)
        {
            if (_isFacingRight)
                _helmetZPositionFacingRight = _helmetComponents[0].localPosition.z;
            else
                _helmetZPositionFacingLeft = _helmetComponents[0].localPosition.z;
        }

        Debug.Log($"Stored z-positions for facing {(_isFacingRight ? "right" : "left")}");
    }

    [ContextMenu("Log All Child Components")]
    public void LogAllChildComponents()
    {
        Debug.Log("🔍 All child components and their z-positions:");

        Transform[] allChildren = GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            if (child == transform) continue;
            Debug.Log($"- {child.name}: z = {child.localPosition.z}");
        }
    }

    private void OnGUI()
    {
        if (!_showDebugInfo) return;

        GUILayout.BeginArea(new Rect(10, Screen.height - 150, 300, 140));
        GUILayout.Box("Sprite Layer Debug", GUILayout.Width(290));

        GUILayout.Label($"Facing: {(_isFacingRight ? "Right" : "Left")}");
        GUILayout.Label($"Head Components: {(_headComponents?.Length ?? 0)}");
        GUILayout.Label($"Helmet Components: {(_helmetComponents?.Length ?? 0)}");

        if (GUILayout.Button("Turn Right"))
            TestTurnRight();
        if (GUILayout.Button("Turn Left"))
            TestTurnLeft();

        GUILayout.EndArea();
    }
}