using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Complete UI system for lantern abilities and skill tree
/// Handles HUD, skill tree display, and notifications
/// </summary>
public class LanternUISystem : MonoBehaviour
{
    [Header("HUD Elements")]
    [SerializeField] private GameObject _hudPanel;
    [SerializeField] private Slider _manaBar;
    [SerializeField] private TextMeshProUGUI _manaText;
    [SerializeField] private Slider _innerLightBar;
    [SerializeField] private TextMeshProUGUI _lightEssenceText;
    [SerializeField] private TextMeshProUGUI _currentLightTypeText;
    [SerializeField] private Image _currentLightTypeIcon;

    [Header("Skill Tree Panel")]
    [SerializeField] private GameObject _skillTreePanel;
    [SerializeField] private RectTransform _skillTreeContent;
    [SerializeField] private ScrollRect _skillTreeScrollRect;
    [SerializeField] private Button _closeSkillTreeButton;

    [Header("Skill Node Prefab")]
    [SerializeField] private GameObject _skillNodePrefab;
    [SerializeField] private GameObject _skillConnectionPrefab;

    [Header("Skill Details Panel")]
    [SerializeField] private GameObject _skillDetailsPanel;
    [SerializeField] private TextMeshProUGUI _skillNameText;
    [SerializeField] private TextMeshProUGUI _skillDescriptionText;
    [SerializeField] private TextMeshProUGUI _skillCostText;
    [SerializeField] private TextMeshProUGUI _skillPrerequisitesText;
    [SerializeField] private Button _unlockSkillButton;

    [Header("Notification System")]
    [SerializeField] private GameObject _notificationPanel;
    [SerializeField] private TextMeshProUGUI _notificationText;
    [SerializeField] private float _notificationDuration = 3f;

    [Header("Light Type Icons")]
    [SerializeField] private LightTypeIconData[] _lightTypeIcons;

    private EnhancedLanternController _lanternController;
    private Dictionary<string, SkillNodeUI> _skillNodes = new Dictionary<string, SkillNodeUI>();
    private LightSkill _selectedSkill;
    private Queue<string> _notificationQueue = new Queue<string>();
    private Coroutine _notificationCoroutine;

    [System.Serializable]
    public class LightTypeIconData
    {
        public EnhancedLanternController.LightType lightType;
        public Sprite icon;
        public Color iconColor = Color.white;
    }

    private void Awake()
    {
        InitializeUI();
    }

    private void Start()
    {
        _lanternController = FindFirstObjectByType<EnhancedLanternController>();
        if (_lanternController != null)
        {
            SubscribeToLanternEvents();
            SetupSkillTree();
        }

        // Initially hide skill tree
        _skillTreePanel.SetActive(false);
        _skillDetailsPanel.SetActive(false);
    }

    private void Update()
    {
        UpdateHUD();
        HandleInput();
    }

    #region Initialization

    private void InitializeUI()
    {
        // Setup button events
        if (_closeSkillTreeButton != null)
            _closeSkillTreeButton.onClick.AddListener(CloseSkillTree);

        if (_unlockSkillButton != null)
            _unlockSkillButton.onClick.AddListener(UnlockSelectedSkill);

        // Initialize notification system
        if (_notificationPanel != null)
            _notificationPanel.SetActive(false);
    }

    private void SubscribeToLanternEvents()
    {
        _lanternController.OnLanternAcquired += OnLanternAcquired;
        _lanternController.OnLightTypeChanged += OnLightTypeChanged;
        _lanternController.OnLightTypeDiscovered += OnLightTypeDiscovered;
        _lanternController.OnManaChanged += OnManaChanged;
        _lanternController.OnInnerLightChanged += OnInnerLightChanged;
        _lanternController.OnLightEssenceChanged += OnLightEssenceChanged;
        _lanternController.OnSkillUnlocked += OnSkillUnlocked;
    }

    #endregion

    #region HUD Updates

    private void UpdateHUD()
    {
        if (_lanternController == null) return;

        // Update HUD visibility
        bool showHUD = _lanternController.HasLantern;
        if (_hudPanel != null)
            _hudPanel.SetActive(showHUD);

        if (!showHUD) return;

        // Update mana bar
        if (_manaBar != null)
            _manaBar.value = _lanternController.ManaPercentage;

        if (_manaText != null)
            _manaText.text = $"{_lanternController.ManaPercentage * 100:F0}%";

        // Update inner light bar
        if (_innerLightBar != null)
            _innerLightBar.value = _lanternController.InnerLightPercentage;

        // Update light essence
        if (_lightEssenceText != null)
            _lightEssenceText.text = $"Essence: {_lanternController.LightEssencePoints}";

        // Update current light type
        if (_currentLightTypeText != null)
            _currentLightTypeText.text = _lanternController.GetCurrentLightTypeName();

        if (_currentLightTypeIcon != null)
            UpdateCurrentLightTypeIcon();
    }

    private void UpdateCurrentLightTypeIcon()
    {
        var iconData = GetLightTypeIconData(_lanternController.CurrentLightType);
        if (iconData != null)
        {
            _currentLightTypeIcon.sprite = iconData.icon;
            _currentLightTypeIcon.color = iconData.iconColor;
            _currentLightTypeIcon.gameObject.SetActive(true);
        }
        else
        {
            _currentLightTypeIcon.gameObject.SetActive(false);
        }
    }

    private LightTypeIconData GetLightTypeIconData(EnhancedLanternController.LightType lightType)
    {
        return _lightTypeIcons.FirstOrDefault(data => data.lightType == lightType);
    }

    #endregion

    #region Input Handling

    private void HandleInput()
    {
        // Toggle skill tree with Tab key
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (_skillTreePanel.activeInHierarchy)
                CloseSkillTree();
            else
                OpenSkillTree();
        }

        // Close skill tree with Escape
        if (Input.GetKeyDown(KeyCode.Escape) && _skillTreePanel.activeInHierarchy)
        {
            CloseSkillTree();
        }
    }

    #endregion

    #region Skill Tree Management

    public void OpenSkillTree()
    {
        if (_lanternController == null || !_lanternController.HasLantern) return;

        _skillTreePanel.SetActive(true);
        RefreshSkillTree();

        // Pause game time if desired
        Time.timeScale = 0f;
    }

    public void CloseSkillTree()
    {
        _skillTreePanel.SetActive(false);
        _skillDetailsPanel.SetActive(false);

        // Unpause game time
        Time.timeScale = 1f;
    }

    private void SetupSkillTree()
    {
        if (_lanternController?.SkillTree == null) return;

        // Clear existing nodes
        foreach (Transform child in _skillTreeContent)
        {
            Destroy(child.gameObject);
        }
        _skillNodes.Clear();

        // Create skill nodes
        var allSkills = _lanternController.SkillTree.GetAllSkills();
        foreach (var skill in allSkills)
        {
            CreateSkillNode(skill);
        }

        // Create connections between skills
        CreateSkillConnections(allSkills);
    }

    private void CreateSkillNode(LightSkill skill)
    {
        if (_skillNodePrefab == null) return;

        GameObject nodeObj = Instantiate(_skillNodePrefab, _skillTreeContent);
        SkillNodeUI nodeUI = nodeObj.GetComponent<SkillNodeUI>();

        if (nodeUI == null)
            nodeUI = nodeObj.AddComponent<SkillNodeUI>();

        nodeUI.Initialize(skill, this);
        _skillNodes[skill.SkillId] = nodeUI;

        // Position node based on category and UI position
        RectTransform nodeRect = nodeObj.GetComponent<RectTransform>();
        Vector2 position = CalculateNodePosition(skill);
        nodeRect.anchoredPosition = position;
    }

    private Vector2 CalculateNodePosition(LightSkill skill)
    {
        // Base positions for different categories
        Vector2 basePosition = Vector2.zero;

        switch (skill.SkillCategory)
        {
            case LightSkill.Category.Core:
                basePosition = new Vector2(0, 0);
                break;
            case LightSkill.Category.EmberSpecialization:
                basePosition = new Vector2(-400, 200);
                break;
            case LightSkill.Category.RadianceSpecialization:
                basePosition = new Vector2(-200, 200);
                break;
            case LightSkill.Category.SolarFlareSpecialization:
                basePosition = new Vector2(0, 200);
                break;
            case LightSkill.Category.MoonBeamSpecialization:
                basePosition = new Vector2(200, 200);
                break;
            case LightSkill.Category.StarlightSpecialization:
                basePosition = new Vector2(400, 200);
                break;
            case LightSkill.Category.Master:
                basePosition = new Vector2(0, 400);
                break;
        }

        // Add skill-specific offset if provided
        basePosition += skill.UIPosition;

        return basePosition;
    }

    private void CreateSkillConnections(List<LightSkill> allSkills)
    {
        if (_skillConnectionPrefab == null) return;

        foreach (var skill in allSkills)
        {
            foreach (var prerequisiteId in skill.Prerequisites)
            {
                if (_skillNodes.ContainsKey(skill.SkillId) && _skillNodes.ContainsKey(prerequisiteId))
                {
                    CreateConnection(_skillNodes[prerequisiteId], _skillNodes[skill.SkillId]);
                }
            }
        }
    }

    private void CreateConnection(SkillNodeUI fromNode, SkillNodeUI toNode)
    {
        GameObject connectionObj = Instantiate(_skillConnectionPrefab, _skillTreeContent);
        SkillConnectionUI connectionUI = connectionObj.GetComponent<SkillConnectionUI>();

        if (connectionUI == null)
            connectionUI = connectionObj.AddComponent<SkillConnectionUI>();

        connectionUI.Initialize(fromNode, toNode);
    }

    public void RefreshSkillTree()
    {
        foreach (var nodeUI in _skillNodes.Values)
        {
            nodeUI.RefreshState();
        }
    }

    public void OnSkillNodeClicked(LightSkill skill)
    {
        _selectedSkill = skill;
        ShowSkillDetails(skill);
    }

    #endregion

    #region Skill Details Panel

    private void ShowSkillDetails(LightSkill skill)
    {
        if (_skillDetailsPanel == null) return;

        _skillDetailsPanel.SetActive(true);

        // Update skill information
        if (_skillNameText != null)
            _skillNameText.text = skill.DisplayName;

        if (_skillDescriptionText != null)
            _skillDescriptionText.text = skill.Description;

        if (_skillCostText != null)
            _skillCostText.text = $"Cost: {skill.EssenceCost} Light Essence";

        // Show prerequisites
        if (_skillPrerequisitesText != null)
        {
            if (skill.Prerequisites.Length > 0)
            {
                string prereqText = "Requires: ";
                var allSkills = _lanternController.SkillTree.GetAllSkills();

                for (int i = 0; i < skill.Prerequisites.Length; i++)
                {
                    var prereqSkill = allSkills.FirstOrDefault(s => s.SkillId == skill.Prerequisites[i]);
                    if (prereqSkill != null)
                    {
                        prereqText += prereqSkill.DisplayName;
                        if (i < skill.Prerequisites.Length - 1)
                            prereqText += ", ";
                    }
                }
                _skillPrerequisitesText.text = prereqText;
            }
            else
            {
                _skillPrerequisitesText.text = "No prerequisites";
            }
        }

        // Update unlock button
        UpdateUnlockButton(skill);
    }

    private void UpdateUnlockButton(LightSkill skill)
    {
        if (_unlockSkillButton == null) return;

        bool isUnlocked = _lanternController.SkillTree.GetUnlockedSkills().Contains(skill);
        bool canUnlock = _lanternController.SkillTree.CanUnlock(skill);
        bool canAfford = _lanternController.CanAffordSkill(skill);

        if (isUnlocked)
        {
            _unlockSkillButton.GetComponentInChildren<TextMeshProUGUI>().text = "UNLOCKED";
            _unlockSkillButton.interactable = false;
        }
        else if (canUnlock && canAfford)
        {
            _unlockSkillButton.GetComponentInChildren<TextMeshProUGUI>().text = "UNLOCK";
            _unlockSkillButton.interactable = true;
        }
        else if (!canUnlock)
        {
            _unlockSkillButton.GetComponentInChildren<TextMeshProUGUI>().text = "LOCKED";
            _unlockSkillButton.interactable = false;
        }
        else if (!canAfford)
        {
            _unlockSkillButton.GetComponentInChildren<TextMeshProUGUI>().text = "INSUFFICIENT ESSENCE";
            _unlockSkillButton.interactable = false;
        }
    }

    private void UnlockSelectedSkill()
    {
        if (_selectedSkill == null || _lanternController == null) return;

        if (_lanternController.UnlockSkill(_selectedSkill))
        {
            RefreshSkillTree();
            UpdateUnlockButton(_selectedSkill);
            ShowNotification($"Unlocked: {_selectedSkill.DisplayName}!");
        }
    }

    #endregion

    #region Event Handlers

    private void OnLanternAcquired()
    {
        ShowNotification("Ancient Lantern acquired! Your journey into light begins...");
    }

    private void OnLightTypeChanged(EnhancedLanternController.LightType newLightType)
    {
        // Visual feedback for light type change
        if (_currentLightTypeIcon != null)
        {
            StartCoroutine(FlashIcon(_currentLightTypeIcon));
        }
    }

    private void OnLightTypeDiscovered(EnhancedLanternController.LightType newLightType)
    {
        var iconData = GetLightTypeIconData(newLightType);
        string lightTypeName = iconData?.lightType.ToString() ?? newLightType.ToString();
        ShowNotification($"New Light Discovered: {lightTypeName}!");
    }

    private void OnManaChanged(float manaPercentage)
    {
        // Could add visual effects for low mana warning
        if (manaPercentage < 0.2f && _manaBar != null)
        {
            StartCoroutine(FlashElement(_manaBar.gameObject, Color.red));
        }
    }

    private void OnInnerLightChanged(float innerLightPercentage)
    {
        // Visual feedback for inner light growth
        if (_innerLightBar != null)
        {
            StartCoroutine(FlashElement(_innerLightBar.gameObject, Color.yellow));
        }
    }

    private void OnLightEssenceChanged(int newAmount)
    {
        // Flash essence counter when gained
        if (_lightEssenceText != null)
        {
            StartCoroutine(FlashElement(_lightEssenceText.gameObject, Color.cyan));
        }
    }

    private void OnSkillUnlocked(LightSkill skill)
    {
        ShowNotification($"Skill Unlocked: {skill.DisplayName}!");
        RefreshSkillTree();
    }

    #endregion

    #region Notification System

    public void ShowNotification(string message)
    {
        _notificationQueue.Enqueue(message);

        if (_notificationCoroutine == null)
        {
            _notificationCoroutine = StartCoroutine(ProcessNotifications());
        }
    }

    private System.Collections.IEnumerator ProcessNotifications()
    {
        while (_notificationQueue.Count > 0)
        {
            string message = _notificationQueue.Dequeue();
            yield return StartCoroutine(DisplayNotification(message));
            yield return new WaitForSeconds(0.5f); // Brief pause between notifications
        }

        _notificationCoroutine = null;
    }

    private System.Collections.IEnumerator DisplayNotification(string message)
    {
        if (_notificationPanel == null || _notificationText == null) yield break;

        // Setup notification
        _notificationText.text = message;
        _notificationPanel.SetActive(true);

        // Slide in animation
        RectTransform notificationRect = _notificationPanel.GetComponent<RectTransform>();
        Vector2 startPos = new Vector2(0, -100);
        Vector2 endPos = Vector2.zero;

        notificationRect.anchoredPosition = startPos;

        float animTime = 0.3f;
        float elapsed = 0f;

        // Slide in
        while (elapsed < animTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / animTime;
            notificationRect.anchoredPosition = Vector2.Lerp(startPos, endPos, EaseOutBack(t));
            yield return null;
        }

        notificationRect.anchoredPosition = endPos;

        // Hold
        yield return new WaitForSecondsRealtime(_notificationDuration);

        // Slide out
        elapsed = 0f;
        startPos = endPos;
        endPos = new Vector2(0, -100);

        while (elapsed < animTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / animTime;
            notificationRect.anchoredPosition = Vector2.Lerp(startPos, endPos, EaseInBack(t));
            yield return null;
        }

        _notificationPanel.SetActive(false);
    }

    #endregion

    #region Visual Effects

    private System.Collections.IEnumerator FlashIcon(Image icon)
    {
        Color originalColor = icon.color;
        Color flashColor = Color.white;

        float flashDuration = 0.2f;
        float elapsed = 0f;

        while (elapsed < flashDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / flashDuration;
            icon.color = Color.Lerp(flashColor, originalColor, t);
            yield return null;
        }

        icon.color = originalColor;
    }

    private System.Collections.IEnumerator FlashElement(GameObject element, Color flashColor)
    {
        var graphic = element.GetComponent<Graphic>();
        if (graphic == null) yield break;

        Color originalColor = graphic.color;
        float flashDuration = 0.3f;
        float elapsed = 0f;

        while (elapsed < flashDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.PingPong(elapsed * 6f, 1f);
            graphic.color = Color.Lerp(originalColor, flashColor, t * 0.5f);
            yield return null;
        }

        graphic.color = originalColor;
    }

    #endregion

    #region Utility Functions

    private float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    private float EaseInBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return c3 * t * t * t - c1 * t * t;
    }

    #endregion

    #region Cleanup

    private void OnDestroy()
    {
        if (_lanternController != null)
        {
            _lanternController.OnLanternAcquired -= OnLanternAcquired;
            _lanternController.OnLightTypeChanged -= OnLightTypeChanged;
            _lanternController.OnLightTypeDiscovered -= OnLightTypeDiscovered;
            _lanternController.OnManaChanged -= OnManaChanged;
            _lanternController.OnInnerLightChanged -= OnInnerLightChanged;
            _lanternController.OnLightEssenceChanged -= OnLightEssenceChanged;
            _lanternController.OnSkillUnlocked -= OnSkillUnlocked;
        }
    }

    #endregion
}

/// <summary>
/// UI component for individual skill nodes in the skill tree
/// </summary>
public class SkillNodeUI : MonoBehaviour
{
    [Header("Node Components")]
    [SerializeField] private Button _nodeButton;
    [SerializeField] private Image _nodeIcon;
    [SerializeField] private Image _nodeBackground;
    [SerializeField] private TextMeshProUGUI _skillNameText;

    [Header("Node States")]
    [SerializeField] private Color _unlockedColor = Color.white;
    [SerializeField] private Color _availableColor = Color.yellow;
    [SerializeField] private Color _lockedColor = Color.gray;
    [SerializeField] private Color _cannotAffordColor = Color.red;

    private LightSkill _skill;
    private LanternUISystem _uiSystem;

    public void Initialize(LightSkill skill, LanternUISystem uiSystem)
    {
        _skill = skill;
        _uiSystem = uiSystem;

        if (_skillNameText != null)
            _skillNameText.text = skill.DisplayName;

        if (_nodeButton != null)
            _nodeButton.onClick.AddListener(OnNodeClicked);

        RefreshState();
    }

    public void RefreshState()
    {
        if (_uiSystem == null || _skill == null) return;

        var lanternController = FindFirstObjectByType<EnhancedLanternController>();
        if (lanternController == null) return;

        bool isUnlocked = lanternController.SkillTree.GetUnlockedSkills().Contains(_skill);
        bool canUnlock = lanternController.SkillTree.CanUnlock(_skill);
        bool canAfford = lanternController.CanAffordSkill(_skill);

        Color nodeColor;
        bool interactable = true;

        if (isUnlocked)
        {
            nodeColor = _unlockedColor;
        }
        else if (canUnlock && canAfford)
        {
            nodeColor = _availableColor;
        }
        else if (canUnlock && !canAfford)
        {
            nodeColor = _cannotAffordColor;
        }
        else
        {
            nodeColor = _lockedColor;
            interactable = false;
        }

        if (_nodeBackground != null)
            _nodeBackground.color = nodeColor;

        if (_nodeButton != null)
            _nodeButton.interactable = interactable;
    }

    private void OnNodeClicked()
    {
        _uiSystem?.OnSkillNodeClicked(_skill);
    }
}

/// <summary>
/// UI component for connections between skill nodes
/// </summary>
public class SkillConnectionUI : MonoBehaviour
{
    [SerializeField] private Image _connectionLine;
    [SerializeField] private Color _activeColor = Color.white;
    [SerializeField] private Color _inactiveColor = Color.gray;

    private SkillNodeUI _fromNode;
    private SkillNodeUI _toNode;

    public void Initialize(SkillNodeUI fromNode, SkillNodeUI toNode)
    {
        _fromNode = fromNode;
        _toNode = toNode;

        SetupConnection();
        RefreshState();
    }

    private void SetupConnection()
    {
        if (_fromNode == null || _toNode == null) return;

        RectTransform fromRect = _fromNode.GetComponent<RectTransform>();
        RectTransform toRect = _toNode.GetComponent<RectTransform>();
        RectTransform connectionRect = GetComponent<RectTransform>();

        // Position and rotate the connection line
        Vector2 fromPos = fromRect.anchoredPosition;
        Vector2 toPos = toRect.anchoredPosition;
        Vector2 direction = toPos - fromPos;

        connectionRect.anchoredPosition = fromPos + direction * 0.5f;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        connectionRect.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // Scale to match distance
        connectionRect.sizeDelta = new Vector2(direction.magnitude, connectionRect.sizeDelta.y);
    }

    private void RefreshState()
    {
        var lanternController = FindFirstObjectByType<EnhancedLanternController>();
        if (lanternController == null) return;

        // Connection is active if the prerequisite skill is unlocked
        var unlockedSkills = lanternController.SkillTree.GetUnlockedSkills();
        bool isActive = unlockedSkills.Any(skill => _fromNode.name.Contains(skill.SkillId));

        if (_connectionLine != null)
            _connectionLine.color = isActive ? _activeColor : _inactiveColor;
    }
}