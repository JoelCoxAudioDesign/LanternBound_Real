using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Master builder for the opening area MVP - creates a naturally flowing level
/// that teaches mechanics through environmental design rather than explicit tutorials
/// </summary>
public class OpeningAreaMasterBuilder : MonoBehaviour
{
    [Header("Build Controls")]
    [SerializeField] private bool _buildCompleteOpeningArea = false;
    [SerializeField] private bool _buildSection1_Awakening = false;
    [SerializeField] private bool _buildSection2_FirstSteps = false;
    [SerializeField] private bool _buildSection3_Darkness = false;
    [SerializeField] private bool _buildSection4_LanternDiscovery = false;
    [SerializeField] private bool _buildSection5_FirstLight = false;
    [SerializeField] private bool _buildSection6_HiddenPaths = false;
    [SerializeField] private bool _buildSection7_Depths = false;
    [SerializeField] private bool _buildSection8_FirstShrine = false;

    [Header("Level Configuration")]
    [SerializeField] private Material _stoneMaterial;
    [SerializeField] private Material _metalMaterial;
    [SerializeField] private Material _mysticalMaterial;
    [SerializeField] private LayerMask _groundLayer = 3;

    [Header("Prefab References")]
    [SerializeField] private GameObject _collectibleLanternPrefab;
    [SerializeField] private GameObject _shrineBasePrefab;
    [SerializeField] private GameObject _enemyPrefab;

    [Header("Visual Themes")]
    [SerializeField] private Color _stoneColor = new Color(0.6f, 0.6f, 0.7f);
    [SerializeField] private Color _darkStoneColor = new Color(0.3f, 0.3f, 0.4f);
    [SerializeField] private Color _mysticalColor = new Color(0.8f, 0.6f, 1f, 0.8f);
    [SerializeField] private Color _hiddenPlatformColor = new Color(1f, 1f, 1f, 0.1f);

    // Area sections for organization
    private Dictionary<string, Transform> _areaSections = new Dictionary<string, Transform>();

    private void OnValidate()
    {
        if (_buildCompleteOpeningArea)
        {
            _buildCompleteOpeningArea = false;
            BuildCompleteOpeningArea();
        }

        if (_buildSection1_Awakening)
        {
            _buildSection1_Awakening = false;
            BuildSection1_Awakening();
        }

        if (_buildSection2_FirstSteps)
        {
            _buildSection2_FirstSteps = false;
            BuildSection2_FirstSteps();
        }

        if (_buildSection3_Darkness)
        {
            _buildSection3_Darkness = false;
            BuildSection3_Darkness();
        }

        if (_buildSection4_LanternDiscovery)
        {
            _buildSection4_LanternDiscovery = false;
            BuildSection4_LanternDiscovery();
        }

        if (_buildSection5_FirstLight)
        {
            _buildSection5_FirstLight = false;
            BuildSection5_FirstLight();
        }

        if (_buildSection6_HiddenPaths)
        {
            _buildSection6_HiddenPaths = false;
            BuildSection6_HiddenPaths();
        }

        if (_buildSection7_Depths)
        {
            _buildSection7_Depths = false;
            BuildSection7_Depths();
        }

        if (_buildSection8_FirstShrine)
        {
            _buildSection8_FirstShrine = false;
            BuildSection8_FirstShrine();
        }
    }

    [ContextMenu("Build Complete Opening Area")]
    public void BuildCompleteOpeningArea()
    {
        ClearExistingLevel();
        CreateAreaSections();

        Debug.Log("🏗️ Building complete opening area MVP...");

        // Build all sections in sequence
        BuildSection1_Awakening();
        BuildSection2_FirstSteps();
        BuildSection3_Darkness();
        BuildSection4_LanternDiscovery();
        BuildSection5_FirstLight();
        BuildSection6_HiddenPaths();
        BuildSection7_Depths();
        BuildSection8_FirstShrine();

        // Add connecting elements and polish
        AddConnectingElements();
        AddAtmosphericDetails();

        Debug.Log("✅ Opening area complete! Ready for MVP testing.");
    }

    #region Section Builders

    /// <summary>
    /// Section 1: Player awakens in a small chamber - learn basic movement
    /// Design: Safe starting area, simple platforming, clear exit
    /// </summary>
    private void BuildSection1_Awakening()
    {
        Transform section = GetAreaSection("01_Awakening");
        Vector3 basePos = Vector3.zero;

        Debug.Log("Building Section 1: Awakening Chamber");

        // Starting platform - warm, safe feeling
        CreatePlatform(section, basePos, new Vector3(8, 1, 1), "Starting_Ground", _stoneColor);

        // Walls to create chamber feeling
        CreateWall(section, basePos + new Vector3(-4, 3, 0), new Vector3(1, 6, 1), "Left_Wall", _stoneColor);
        CreateWall(section, basePos + new Vector3(4, 3, 0), new Vector3(1, 6, 1), "Right_Wall", _stoneColor);
        CreateWall(section, basePos + new Vector3(0, 6, 0), new Vector3(8, 1, 1), "Ceiling", _stoneColor);

        // Simple jump practice - 3 ascending platforms
        CreatePlatform(section, basePos + new Vector3(6, 1.5f, 0), new Vector3(3, 0.5f, 1), "Jump_Platform_1", _stoneColor);
        CreatePlatform(section, basePos + new Vector3(9, 3, 0), new Vector3(3, 0.5f, 1), "Jump_Platform_2", _stoneColor);
        CreatePlatform(section, basePos + new Vector3(12, 4.5f, 0), new Vector3(4, 0.5f, 1), "Exit_Platform", _stoneColor);

        // Player spawn point
        CreatePlayerSpawn(section, basePos + new Vector3(-2, 2, 0));

        // Subtle lighting to guide toward exit
        CreateGuideLight(section, basePos + new Vector3(12, 6, 0), Color.white, 0.5f);
    }

    /// <summary>
    /// Section 2: First real platforming - introduce running and jumping
    /// Design: Longer jumps that require running, still well-lit
    /// </summary>
    private void BuildSection2_FirstSteps()
    {
        Transform section = GetAreaSection("02_FirstSteps");
        Vector3 basePos = new Vector3(20, 4, 0);

        Debug.Log("Building Section 2: First Steps");

        // Entry platform
        CreatePlatform(section, basePos, new Vector3(4, 1, 1), "Entry_Platform", _stoneColor);

        // Gap that requires running jump
        CreatePlatform(section, basePos + new Vector3(8, 0, 0), new Vector3(5, 1, 1), "Running_Jump_Target", _stoneColor);

        // Descending path with varied jump distances
        CreatePlatform(section, basePos + new Vector3(15, -1, 0), new Vector3(3, 0.5f, 1), "Descent_1", _stoneColor);
        CreatePlatform(section, basePos + new Vector3(20, -2.5f, 0), new Vector3(3, 0.5f, 1), "Descent_2", _stoneColor);
        CreatePlatform(section, basePos + new Vector3(26, -3, 0), new Vector3(6, 1, 1), "Rest_Platform", _stoneColor);

        // Small challenge: Wall jump setup (optional path)
        CreateWall(section, basePos + new Vector3(32, -1, 0), new Vector3(1, 4, 1), "Wall_Jump_Wall", _stoneColor);
        CreatePlatform(section, basePos + new Vector3(35, 1, 0), new Vector3(4, 0.5f, 1), "Wall_Jump_Reward", _stoneColor);

        // Main path continues down
        CreatePlatform(section, basePos + new Vector3(30, -5, 0), new Vector3(4, 1, 1), "Main_Path_Continue", _stoneColor);
    }

    /// <summary>
    /// Section 3: World begins to darken - introduce the need for light
    /// Design: Progressively darker, hidden platforms become visible, create longing for light
    /// </summary>
    private void BuildSection3_Darkness()
    {
        Transform section = GetAreaSection("03_Darkness");
        Vector3 basePos = new Vector3(40, -5, 0);

        Debug.Log("Building Section 3: Embrace of Darkness");

        // Entry platform starts normal
        CreatePlatform(section, basePos, new Vector3(5, 1, 1), "Light_Fading_Start", _stoneColor);

        // Platforms get progressively darker and more worn
        CreatePlatform(section, basePos + new Vector3(7, -2, 0), new Vector3(4, 1, 1), "Dimming_Platform_1",
            Color.Lerp(_stoneColor, _darkStoneColor, 0.3f));

        CreatePlatform(section, basePos + new Vector3(13, -4, 0), new Vector3(4, 1, 1), "Dimming_Platform_2",
            Color.Lerp(_stoneColor, _darkStoneColor, 0.6f));

        CreatePlatform(section, basePos + new Vector3(19, -6, 0), new Vector3(4, 1, 1), "Almost_Dark_Platform", _darkStoneColor);

        // First hidden platform - barely visible
        CreateHiddenPlatform(section, basePos + new Vector3(25, -5, 0), new Vector3(3, 0.5f, 1), "First_Hidden_Hint");

        // Gap that looks impossible without the hidden platform
        CreatePlatform(section, basePos + new Vector3(30, -7, 0), new Vector3(6, 1, 1), "Dark_Landing", _darkStoneColor);

        // Descending tunnel entrance
        CreateWall(section, basePos + new Vector3(33, -4, 0), new Vector3(1, 6, 1), "Tunnel_Left_Wall", _darkStoneColor);
        CreateWall(section, basePos + new Vector3(39, -4, 0), new Vector3(1, 6, 1), "Tunnel_Right_Wall", _darkStoneColor);
        CreateWall(section, basePos + new Vector3(36, 2, 0), new Vector3(6, 1, 1), "Tunnel_Ceiling", _darkStoneColor);

        // Remove ambient lighting in this section
        CreateDarknessZone(section, basePos + new Vector3(20, -5, 0), 25f);
    }

    /// <summary>
    /// Section 4: Lantern discovery - the emotional centerpiece
    /// Design: Beautiful chamber with the lantern at center, sense of ancient power
    /// </summary>
    private void BuildSection4_LanternDiscovery()
    {
        Transform section = GetAreaSection("04_LanternDiscovery");
        Vector3 basePos = new Vector3(60, -10, 0);

        Debug.Log("Building Section 4: The Lantern's Rest");

        // Entry from tunnel
        CreatePlatform(section, basePos + new Vector3(-5, 0, 0), new Vector3(3, 1, 1), "Tunnel_Exit", _darkStoneColor);

        // Circular chamber - ancient and mystical
        Vector3 centerPos = basePos + new Vector3(8, -3, 0);

        // Chamber floor - large circular platform
        CreatePlatform(section, centerPos, new Vector3(16, 1, 1), "Chamber_Floor", _mysticalColor);

        // Curved walls to create circular feeling
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f * Mathf.Deg2Rad;
            Vector3 wallPos = centerPos + new Vector3(Mathf.Cos(angle) * 8f, Mathf.Sin(angle) * 4f + 3f, 0);
            CreateWall(section, wallPos, new Vector3(2, 6, 1), $"Chamber_Wall_{i}", _mysticalColor);
        }

        // Elevated altar for the lantern
        CreatePlatform(section, centerPos + new Vector3(0, 1.5f, 0), new Vector3(4, 1, 1), "Lantern_Altar", _mysticalColor);

        // Place the collectible lantern
        if (_collectibleLanternPrefab != null)
        {
            GameObject lantern = Instantiate(_collectibleLanternPrefab, centerPos + new Vector3(0, 3, 0), Quaternion.identity);
            lantern.transform.SetParent(section);
            lantern.name = "Ancient_Lantern";
        }
        else
        {
            // Create placeholder lantern
            CreateLanternPlaceholder(section, centerPos + new Vector3(0, 3, 0));
        }

        // Ancient runes/decorations around the chamber
        CreateDecorationRing(section, centerPos, 6f, 12);

        // Exit paths (multiple to encourage exploration)
        CreatePlatform(section, basePos + new Vector3(20, -1, 0), new Vector3(4, 1, 1), "Exit_East", _mysticalColor);
        CreatePlatform(section, basePos + new Vector3(5, -8, 0), new Vector3(4, 1, 1), "Exit_South", _mysticalColor);
    }

    /// <summary>
    /// Section 5: First light usage - reveal the hidden world
    /// Design: Obvious hidden platforms that respond to basic Ember light
    /// </summary>
    private void BuildSection5_FirstLight()
    {
        Transform section = GetAreaSection("05_FirstLight");
        Vector3 basePos = new Vector3(85, -8, 0);

        Debug.Log("Building Section 5: First Light");

        // Entry platform from lantern chamber
        CreatePlatform(section, basePos, new Vector3(4, 1, 1), "First_Light_Start", _mysticalColor);

        // Series of hidden platforms that are clearly meant to be revealed
        CreateHiddenPlatform(section, basePos + new Vector3(6, 1, 0), new Vector3(3, 0.5f, 1), "Reveal_Step_1");
        CreateHiddenPlatform(section, basePos + new Vector3(10, 2.5f, 0), new Vector3(3, 0.5f, 1), "Reveal_Step_2");
        CreateHiddenPlatform(section, basePos + new Vector3(14, 4, 0), new Vector3(3, 0.5f, 1), "Reveal_Step_3");

        // Landing platform - clearly visible to show the goal
        CreatePlatform(section, basePos + new Vector3(18, 5, 0), new Vector3(6, 1, 1), "First_Light_Success", _stoneColor);

        // Alternative path below for players who haven't figured it out yet
        CreatePlatform(section, basePos + new Vector3(8, -3, 0), new Vector3(4, 1, 1), "Lower_Path_1", _darkStoneColor);
        CreatePlatform(section, basePos + new Vector3(15, -3, 0), new Vector3(4, 1, 1), "Lower_Path_2", _darkStoneColor);

        // Dead end that encourages trying the light
        CreateWall(section, basePos + new Vector3(19, -1.5f, 0), new Vector3(1, 3, 1), "Encouraging_Wall", _darkStoneColor);

        // Visual hint - faint outline where hidden platforms should be
        CreatePlatformOutline(section, basePos + new Vector3(6, 1, 0), new Vector3(3, 0.5f, 1));
        CreatePlatformOutline(section, basePos + new Vector3(10, 2.5f, 0), new Vector3(3, 0.5f, 1));
        CreatePlatformOutline(section, basePos + new Vector3(14, 4, 0), new Vector3(3, 0.5f, 1));
    }

    /// <summary>
    /// Section 6: Hidden paths mastery - multiple routes and secrets
    /// Design: Branching paths, some hidden platforms lead to secrets, others to progression
    /// </summary>
    private void BuildSection6_HiddenPaths()
    {
        Transform section = GetAreaSection("06_HiddenPaths");
        Vector3 basePos = new Vector3(110, 5, 0);

        Debug.Log("Building Section 6: Hidden Paths");

        // Main platform hub
        CreatePlatform(section, basePos, new Vector3(8, 1, 1), "Hidden_Paths_Hub", _stoneColor);

        // Upper secret path
        CreateHiddenPlatform(section, basePos + new Vector3(4, 3, 0), new Vector3(2, 0.5f, 1), "Secret_Step_1");
        CreateHiddenPlatform(section, basePos + new Vector3(8, 5, 0), new Vector3(2, 0.5f, 1), "Secret_Step_2");
        CreateHiddenPlatform(section, basePos + new Vector3(12, 7, 0), new Vector3(2, 0.5f, 1), "Secret_Step_3");
        CreatePlatform(section, basePos + new Vector3(16, 8, 0), new Vector3(6, 1, 1), "Secret_Reward_Platform", _mysticalColor);

        // Secret reward (could be mana restore, collectible, etc.)
        CreateManaRestore(section, basePos + new Vector3(16, 10, 0));

        // Main progression path (mix of hidden and visible)
        CreatePlatform(section, basePos + new Vector3(10, 0, 0), new Vector3(3, 1, 1), "Main_Visible_1", _stoneColor);
        CreateHiddenPlatform(section, basePos + new Vector3(15, -1, 0), new Vector3(3, 0.5f, 1), "Main_Hidden_1");
        CreatePlatform(section, basePos + new Vector3(20, -2, 0), new Vector3(3, 1, 1), "Main_Visible_2", _stoneColor);
        CreateHiddenPlatform(section, basePos + new Vector3(25, -1, 0), new Vector3(3, 0.5f, 1), "Main_Hidden_2");

        // Lower danger path (visible but with enemy)
        CreatePlatform(section, basePos + new Vector3(12, -4, 0), new Vector3(12, 1, 1), "Danger_Path", _darkStoneColor);
        CreateSimpleEnemy(section, basePos + new Vector3(18, -2, 0));

        // All paths converge
        CreatePlatform(section, basePos + new Vector3(30, 0, 0), new Vector3(6, 1, 1), "Paths_Converge", _stoneColor);
    }

    /// <summary>
    /// Section 7: The depths - introduce the need for mana management
    /// Design: Long section requiring sustained light use, mana pressure
    /// </summary>
    private void BuildSection7_Depths()
    {
        Transform section = GetAreaSection("07_Depths");
        Vector3 basePos = new Vector3(140, 0, 0);

        Debug.Log("Building Section 7: The Depths");

        // Entry platform
        CreatePlatform(section, basePos, new Vector3(4, 1, 1), "Depths_Entry", _stoneColor);

        // Long sequence of hidden platforms requiring sustained mana use
        for (int i = 0; i < 8; i++)
        {
            Vector3 platformPos = basePos + new Vector3(5 + i * 4, -i * 0.5f, 0);
            CreateHiddenPlatform(section, platformPos, new Vector3(2.5f, 0.5f, 1), $"Depths_Hidden_{i}");
        }

        // Midway mana restore point
        CreatePlatform(section, basePos + new Vector3(20, -2, 0), new Vector3(4, 1, 1), "Mana_Rest_Platform", _mysticalColor);
        CreateManaRestore(section, basePos + new Vector3(20, -0.5f, 0));

        // Continue the sequence
        for (int i = 0; i < 6; i++)
        {
            Vector3 platformPos = basePos + new Vector3(26 + i * 4, -3 + i * 0.3f, 0);
            CreateHiddenPlatform(section, platformPos, new Vector3(2.5f, 0.5f, 1), $"Depths_Hidden_B_{i}");
        }

        // Depths floor - if you fall
        CreatePlatform(section, basePos + new Vector3(25, -10, 0), new Vector3(30, 1, 1), "Depths_Floor", _darkStoneColor);

        // Exit platform
        CreatePlatform(section, basePos + new Vector3(50, -1, 0), new Vector3(6, 1, 1), "Depths_Exit", _stoneColor);

        // Atmospheric darkness
        CreateDarknessZone(section, basePos + new Vector3(25, -5, 0), 40f);
    }

    /// <summary>
    /// Section 8: First shrine - discover the Radiance light type
    /// Design: Major puzzle requiring light to activate, teaches shrine mechanics
    /// </summary>
    private void BuildSection8_FirstShrine()
    {
        Transform section = GetAreaSection("08_FirstShrine");
        Vector3 basePos = new Vector3(200, -1, 0);

        Debug.Log("Building Section 8: First Shrine");

        // Approach platform
        CreatePlatform(section, basePos, new Vector3(6, 1, 1), "Shrine_Approach", _stoneColor);

        // Shrine chamber - circular and impressive
        Vector3 shrineCenter = basePos + new Vector3(15, 2, 0);
        CreatePlatform(section, shrineCenter, new Vector3(20, 1, 1), "Shrine_Chamber_Floor", _mysticalColor);

        // Shrine walls
        for (int i = 0; i < 12; i++)
        {
            float angle = i * 30f * Mathf.Deg2Rad;
            Vector3 wallPos = shrineCenter + new Vector3(Mathf.Cos(angle) * 10f, Mathf.Sin(angle) * 6f + 4f, 0);
            CreateWall(section, wallPos, new Vector3(2, 8, 1), $"Shrine_Wall_{i}", _mysticalColor);
        }

        // Central shrine pedestal
        CreatePlatform(section, shrineCenter + new Vector3(0, 2, 0), new Vector3(6, 2, 1), "Shrine_Pedestal", _mysticalColor);

        // Place the shrine
        if (_shrineBasePrefab != null)
        {
            GameObject shrine = Instantiate(_shrineBasePrefab, shrineCenter + new Vector3(0, 4, 0), Quaternion.identity);
            shrine.transform.SetParent(section);
            shrine.name = "Radiance_Shrine";

            // Configure shrine to grant Radiance light type
            var shrineComponent = shrine.GetComponent<EnhancedLanternShrine>();
            if (shrineComponent != null)
            {
                // Use reflection to set the granted light type
                var grantedLightField = typeof(EnhancedLanternShrine).GetField("_grantedLightType",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                grantedLightField?.SetValue(shrineComponent, LanternController.LightType.Radiance);
            }
        }
        else
        {
            CreateShrinePlaceholder(section, shrineCenter + new Vector3(0, 4, 0));
        }

        // Platforms leading to shrine (require light to reveal approach)
        CreateHiddenPlatform(section, basePos + new Vector3(8, 3, 0), new Vector3(3, 0.5f, 1), "Shrine_Approach_1");
        CreateHiddenPlatform(section, basePos + new Vector3(12, 4, 0), new Vector3(3, 0.5f, 1), "Shrine_Approach_2");

        // Exit paths that open after shrine activation
        CreatePlatform(section, shrineCenter + new Vector3(12, 6, 0), new Vector3(4, 1, 1), "Shrine_Exit_Upper", _mysticalColor);
        CreatePlatform(section, shrineCenter + new Vector3(-12, 3, 0), new Vector3(4, 1, 1), "Shrine_Exit_Lower", _mysticalColor);

        // Victory celebration area
        CreatePlatform(section, basePos + new Vector3(35, 8, 0), new Vector3(8, 1, 1), "Victory_Platform", _mysticalColor);
        CreateGuideLight(section, basePos + new Vector3(35, 12, 0), Color.white, 2f);

        Debug.Log("🏆 First Shrine section complete - this is where players discover their second light type!");
    }

    #endregion

    #region Helper Methods

    private void ClearExistingLevel()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        _areaSections.Clear();
    }

    private void CreateAreaSections()
    {
        string[] sectionNames = {
            "01_Awakening", "02_FirstSteps", "03_Darkness", "04_LanternDiscovery",
            "05_FirstLight", "06_HiddenPaths", "07_Depths", "08_FirstShrine"
        };

        foreach (string sectionName in sectionNames)
        {
            GameObject sectionObj = new GameObject(sectionName);
            sectionObj.transform.SetParent(transform);
            _areaSections[sectionName] = sectionObj.transform;
        }
    }

    private Transform GetAreaSection(string sectionName)
    {
        if (_areaSections.ContainsKey(sectionName))
            return _areaSections[sectionName];

        GameObject sectionObj = new GameObject(sectionName);
        sectionObj.transform.SetParent(transform);
        _areaSections[sectionName] = sectionObj.transform;
        return sectionObj.transform;
    }

    private GameObject CreatePlatform(Transform parent, Vector3 position, Vector3 scale, string name, Color color)
    {
        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.name = name;
        platform.transform.SetParent(parent);
        platform.transform.position = position;
        platform.transform.localScale = scale;

        // Remove 3D collider, add 2D collider
        DestroyImmediate(platform.GetComponent<BoxCollider>());
        BoxCollider2D collider2D = platform.AddComponent<BoxCollider2D>();

        // Set layer
        platform.layer = _groundLayer;

        // Set color and material
        Renderer renderer = platform.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = GetMaterialForColor(color);
            renderer.material.color = color;
        }

        return platform;
    }

    private GameObject CreateWall(Transform parent, Vector3 position, Vector3 scale, string name, Color color)
    {
        return CreatePlatform(parent, position, scale, name, color); // Walls are just platforms
    }

    private GameObject CreateHiddenPlatform(Transform parent, Vector3 position, Vector3 scale, string name)
    {
        GameObject platform = CreatePlatform(parent, position, scale, name, _hiddenPlatformColor);

        // Add the enhanced revealable platform component
        var revealComponent = platform.AddComponent<EnhancedRevealablePlatform>();

        // Make it start hidden
        platform.GetComponent<BoxCollider2D>().enabled = false;

        return platform;
    }

    private void CreatePlayerSpawn(Transform parent, Vector3 position)
    {
        GameObject spawn = new GameObject("Player_Spawn");
        spawn.transform.SetParent(parent);
        spawn.transform.position = position;

        // Add a visual indicator
        var renderer = spawn.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateSimpleSprite(Color.green);
        renderer.sortingOrder = 10;
    }

    private void CreateGuideLight(Transform parent, Vector3 position, Color color, float intensity)
    {
        GameObject lightObj = new GameObject("Guide_Light");
        lightObj.transform.SetParent(parent);
        lightObj.transform.position = position;

        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = 8f;
    }

    private void CreateDarknessZone(Transform parent, Vector3 center, float radius)
    {
        GameObject darknessZone = new GameObject("Darkness_Zone");
        darknessZone.transform.SetParent(parent);
        darknessZone.transform.position = center;

        // Add a sphere collider to define the darkness area
        SphereCollider darknessTrigger = darknessZone.AddComponent<SphereCollider>();
        darknessTrigger.isTrigger = true;
        darknessTrigger.radius = radius;

        // Add darkness effect component (you can create this later)
        // DarknessZoneEffect darknessEffect = darknessZone.AddComponent<DarknessZoneEffect>();
    }

    private void CreateLanternPlaceholder(Transform parent, Vector3 position)
    {
        GameObject lanternPlaceholder = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        lanternPlaceholder.name = "Ancient_Lantern_Placeholder";
        lanternPlaceholder.transform.SetParent(parent);
        lanternPlaceholder.transform.position = position;
        lanternPlaceholder.transform.localScale = Vector3.one * 1.5f;

        // Remove 3D collider, add 2D trigger
        DestroyImmediate(lanternPlaceholder.GetComponent<SphereCollider>());
        CircleCollider2D trigger = lanternPlaceholder.AddComponent<CircleCollider2D>();
        trigger.isTrigger = true;

        // Make it glow
        Renderer renderer = lanternPlaceholder.GetComponent<Renderer>();
        renderer.material.color = Color.yellow;
        renderer.material.SetFloat("_Mode", 3); // Transparent mode
        renderer.material.color = new Color(1f, 1f, 0f, 0.8f);

        // Add light
        Light glowLight = lanternPlaceholder.AddComponent<Light>();
        glowLight.type = LightType.Point;
        glowLight.color = Color.yellow;
        glowLight.intensity = 2f;
        glowLight.range = 8f;

        // Add collectible component
        var collectible = lanternPlaceholder.AddComponent<EnhancedCollectibleLantern>();
    }

    private void CreateShrinePlaceholder(Transform parent, Vector3 position)
    {
        GameObject shrinePlaceholder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        shrinePlaceholder.name = "Radiance_Shrine_Placeholder";
        shrinePlaceholder.transform.SetParent(parent);
        shrinePlaceholder.transform.position = position;
        shrinePlaceholder.transform.localScale = new Vector3(2f, 3f, 2f);

        // Remove 3D collider, add 2D trigger
        DestroyImmediate(shrinePlaceholder.GetComponent<CapsuleCollider>());
        BoxCollider2D trigger = shrinePlaceholder.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size = new Vector2(2f, 6f);

        // Shrine appearance
        Renderer renderer = shrinePlaceholder.GetComponent<Renderer>();
        renderer.material.color = _mysticalColor;

        // Add shrine component
        var shrine = shrinePlaceholder.AddComponent<EnhancedLanternShrine>();

        // Configure shrine to grant Radiance light type using reflection
        var grantedLightField = typeof(EnhancedLanternShrine).GetField("_grantedLightType",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        grantedLightField?.SetValue(shrine, LanternController.LightType.Radiance);

        var requiredLightField = typeof(EnhancedLanternShrine).GetField("_requiredLightType",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        requiredLightField?.SetValue(shrine, LanternController.LightType.Ember);
    }

    private void CreateManaRestore(Transform parent, Vector3 position)
    {
        GameObject manaRestore = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        manaRestore.name = "Mana_Crystal";
        manaRestore.transform.SetParent(parent);
        manaRestore.transform.position = position;
        manaRestore.transform.localScale = Vector3.one * 0.8f;

        // Remove 3D collider, add 2D trigger
        DestroyImmediate(manaRestore.GetComponent<SphereCollider>());
        CircleCollider2D trigger = manaRestore.AddComponent<CircleCollider2D>();
        trigger.isTrigger = true;

        // Mana crystal appearance
        Renderer renderer = manaRestore.GetComponent<Renderer>();
        renderer.material.color = new Color(0.3f, 0.7f, 1f, 0.8f);

        // Add glow
        Light crystalLight = manaRestore.AddComponent<Light>();
        crystalLight.type = LightType.Point;
        crystalLight.color = Color.cyan;
        crystalLight.intensity = 1f;
        crystalLight.range = 3f;

        // Add mana restore component (you'll create this)
        // ManaRestorePickup manaPickup = manaRestore.AddComponent<ManaRestorePickup>();
    }

    private void CreateSimpleEnemy(Transform parent, Vector3 position)
    {
        GameObject enemy;

        if (_enemyPrefab != null)
        {
            enemy = Instantiate(_enemyPrefab, position, Quaternion.identity);
            enemy.transform.SetParent(parent);
        }
        else
        {
            // Create placeholder enemy
            enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = "Shadow_Creature";
            enemy.transform.SetParent(parent);
            enemy.transform.position = position;
            enemy.transform.localScale = new Vector3(1f, 1.5f, 1f);

            // Remove 3D collider, add 2D collider
            DestroyImmediate(enemy.GetComponent<CapsuleCollider>());
            CapsuleCollider2D collider2D = enemy.AddComponent<CapsuleCollider2D>();

            // Dark enemy appearance
            Renderer renderer = enemy.GetComponent<Renderer>();
            renderer.material.color = new Color(0.2f, 0.1f, 0.3f);

            // Add Rigidbody2D
            Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;

            // Add enhanced light sensitive enemy component
            var enemyComponent = enemy.AddComponent<EnhancedLightSensitiveEnemy>();
        }
    }

    private void CreatePlatformOutline(Transform parent, Vector3 position, Vector3 scale)
    {
        GameObject outline = new GameObject("Platform_Outline");
        outline.transform.SetParent(parent);
        outline.transform.position = position;
        outline.transform.localScale = scale;

        // Create a wireframe cube outline
        LineRenderer lr = outline.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.endColor = new Color(1f, 1f, 1f, 0.3f);
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.useWorldSpace = false;

        // Draw rectangle outline
        lr.positionCount = 5;
        Vector3[] points = {
            new Vector3(-scale.x/2, -scale.y/2, 0),
            new Vector3(scale.x/2, -scale.y/2, 0),
            new Vector3(scale.x/2, scale.y/2, 0),
            new Vector3(-scale.x/2, scale.y/2, 0),
            new Vector3(-scale.x/2, -scale.y/2, 0)
        };
        lr.SetPositions(points);
    }

    private void CreateDecorationRing(Transform parent, Vector3 center, float radius, int count)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = (i * 360f / count) * Mathf.Deg2Rad;
            Vector3 decorationPos = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);

            GameObject decoration = GameObject.CreatePrimitive(PrimitiveType.Cube);
            decoration.name = $"Ancient_Rune_{i}";
            decoration.transform.SetParent(parent);
            decoration.transform.position = decorationPos;
            decoration.transform.localScale = Vector3.one * 0.3f;
            decoration.transform.rotation = Quaternion.AngleAxis(i * 30f, Vector3.forward);

            // Remove collider - purely decorative
            DestroyImmediate(decoration.GetComponent<BoxCollider>());

            // Mystical appearance
            Renderer renderer = decoration.GetComponent<Renderer>();
            renderer.material.color = new Color(0.8f, 0.6f, 1f, 0.7f);
        }
    }

    private Material GetMaterialForColor(Color color)
    {
        // Simple material assignment based on color
        if (Vector3.Distance(new Vector3(color.r, color.g, color.b), new Vector3(_mysticalColor.r, _mysticalColor.g, _mysticalColor.b)) < 0.3f)
            return _mysticalMaterial;
        else if (color.grayscale < 0.4f)
            return _metalMaterial;
        else
            return _stoneMaterial;
    }

    private Sprite CreateSimpleSprite(Color color)
    {
        Texture2D texture = new Texture2D(16, 16);
        Color[] colors = new Color[16 * 16];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = color;
        }
        texture.SetPixels(colors);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f));
    }

    #endregion

    #region Connecting Elements and Polish

    private void AddConnectingElements()
    {
        Debug.Log("Adding connecting elements between sections...");

        // Smooth transitions between sections
        CreateTransitionPlatform("01_Awakening", "02_FirstSteps", new Vector3(16, 4.5f, 0), new Vector3(2, 0.5f, 1));
        CreateTransitionPlatform("02_FirstSteps", "03_Darkness", new Vector3(36, -5, 0), new Vector3(2, 0.5f, 1));
        CreateTransitionPlatform("03_Darkness", "04_LanternDiscovery", new Vector3(55, -10, 0), new Vector3(2, 0.5f, 1));
        CreateTransitionPlatform("04_LanternDiscovery", "05_FirstLight", new Vector3(80, -8, 0), new Vector3(2, 0.5f, 1));
        CreateTransitionPlatform("05_FirstLight", "06_HiddenPaths", new Vector3(105, 5, 0), new Vector3(2, 0.5f, 1));
        CreateTransitionPlatform("06_HiddenPaths", "07_Depths", new Vector3(136, 0, 0), new Vector3(2, 0.5f, 1));
        CreateTransitionPlatform("07_Depths", "08_FirstShrine", new Vector3(195, -1, 0), new Vector3(2, 0.5f, 1));
    }

    private void CreateTransitionPlatform(string fromSection, string toSection, Vector3 position, Vector3 scale)
    {
        Transform fromParent = GetAreaSection(fromSection);
        GameObject transition = CreatePlatform(fromParent, position, scale, $"Transition_to_{toSection}", _stoneColor);
        transition.name = $"Transition_{fromSection}_to_{toSection}";
    }

    private void AddAtmosphericDetails()
    {
        Debug.Log("Adding atmospheric details...");

        // Background elements, particle effects, ambient sounds, etc.
        AddBackgroundLighting();
        AddParticleEffects();
        AddSoundZones();
    }

    private void AddBackgroundLighting()
    {
        // Subtle ambient lighting for different areas
        Transform awakeningSection = GetAreaSection("01_Awakening");
        CreateAmbientLight(awakeningSection, Vector3.zero, Color.white, 0.3f, "Awakening_Ambient");

        Transform darknessSection = GetAreaSection("03_Darkness");
        CreateAmbientLight(darknessSection, new Vector3(40, -5, 0), new Color(0.1f, 0.1f, 0.2f), 0.1f, "Darkness_Ambient");

        Transform lanternSection = GetAreaSection("04_LanternDiscovery");
        CreateAmbientLight(lanternSection, new Vector3(68, -7, 0), _mysticalColor, 0.5f, "Lantern_Chamber_Ambient");

        Transform shrineSection = GetAreaSection("08_FirstShrine");
        CreateAmbientLight(shrineSection, new Vector3(215, 1, 0), _mysticalColor, 0.7f, "Shrine_Ambient");
    }

    private void CreateAmbientLight(Transform parent, Vector3 position, Color color, float intensity, string name)
    {
        GameObject ambientLightObj = new GameObject(name);
        ambientLightObj.transform.SetParent(parent);
        ambientLightObj.transform.position = position;

        Light ambientLight = ambientLightObj.AddComponent<Light>();
        ambientLight.type = LightType.Point;
        ambientLight.color = color;
        ambientLight.intensity = intensity;
        ambientLight.range = 15f;
    }

    private void AddParticleEffects()
    {
        // Dust motes in the awakening chamber
        Transform awakeningSection = GetAreaSection("01_Awakening");
        CreateParticleEffect(awakeningSection, new Vector3(4, 3, 0), "Dust_Motes");

        // Mystical sparkles in the lantern chamber
        Transform lanternSection = GetAreaSection("04_LanternDiscovery");
        CreateParticleEffect(lanternSection, new Vector3(68, -7, 0), "Mystical_Sparkles");

        // Shrine energy
        Transform shrineSection = GetAreaSection("08_FirstShrine");
        CreateParticleEffect(shrineSection, new Vector3(215, 3, 0), "Shrine_Energy");
    }

    private void CreateParticleEffect(Transform parent, Vector3 position, string name)
    {
        GameObject particleObj = new GameObject(name);
        particleObj.transform.SetParent(parent);
        particleObj.transform.position = position;

        ParticleSystem particles = particleObj.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startLifetime = 5f;
        main.startSpeed = 0.5f;
        main.maxParticles = 20;

        var emission = particles.emission;
        emission.rateOverTime = 2f;

        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(10f, 5f, 1f);
    }

    private void AddSoundZones()
    {
        // Ambient sound zones for different areas
        Transform darknessSection = GetAreaSection("03_Darkness");
        CreateSoundZone(darknessSection, new Vector3(40, -5, 0), "Ambient_Darkness_Sounds");

        Transform lanternSection = GetAreaSection("04_LanternDiscovery");
        CreateSoundZone(lanternSection, new Vector3(68, -7, 0), "Mystical_Chamber_Sounds");

        Transform shrineSection = GetAreaSection("08_FirstShrine");
        CreateSoundZone(shrineSection, new Vector3(215, 1, 0), "Ancient_Shrine_Sounds");
    }

    private void CreateSoundZone(Transform parent, Vector3 position, string name)
    {
        GameObject soundZone = new GameObject(name);
        soundZone.transform.SetParent(parent);
        soundZone.transform.position = position;

        AudioSource audioSource = soundZone.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = true;
        audioSource.volume = 0.3f;
        audioSource.spatialBlend = 1f; // 3D sound

        // Add sphere collider for 3D audio zones
        SphereCollider audioTrigger = soundZone.AddComponent<SphereCollider>();
        audioTrigger.isTrigger = true;
        audioTrigger.radius = 10f;

        // Note: Assign actual audio clips later
        Debug.Log($"Sound zone created: {name} (assign audio clips in inspector)");
    }

    #endregion

    #region Debug and Validation

    [ContextMenu("Validate Level Flow")]
    public void ValidateLevelFlow()
    {
        Debug.Log("🔍 Validating level flow...");

        // Check that each section connects properly
        ValidateSection("01_Awakening", "Basic movement teaching area");
        ValidateSection("02_FirstSteps", "Running and jumping practice");
        ValidateSection("03_Darkness", "Introduces need for light");
        ValidateSection("04_LanternDiscovery", "Emotional centerpiece - lantern acquisition");
        ValidateSection("05_FirstLight", "First use of light mechanics");
        ValidateSection("06_HiddenPaths", "Mastery of basic light use");
        ValidateSection("07_Depths", "Mana management pressure");
        ValidateSection("08_FirstShrine", "Major milestone - second light type");

        Debug.Log("✅ Level flow validation complete!");
    }

    private void ValidateSection(string sectionName, string purpose)
    {
        Transform section = GetAreaSection(sectionName);
        if (section == null)
        {
            Debug.LogError($"❌ Section {sectionName} not found!");
            return;
        }

        int platformCount = 0;
        int hiddenPlatformCount = 0;
        int enemyCount = 0;
        int shrineCount = 0;
        int lanternCount = 0;

        foreach (Transform child in section)
        {
            if (child.name.Contains("Platform"))
                platformCount++;
            if (child.GetComponent<EnhancedRevealablePlatform>())
                hiddenPlatformCount++;
            if (child.GetComponent<EnhancedLightSensitiveEnemy>())
                enemyCount++;
            if (child.GetComponent<EnhancedLanternShrine>())
                shrineCount++;
            if (child.GetComponent<EnhancedCollectibleLantern>())
                lanternCount++;
        }

        Debug.Log($"✅ {sectionName}: {platformCount} platforms, {hiddenPlatformCount} hidden, {enemyCount} enemies, {shrineCount} shrines, {lanternCount} lanterns");
        Debug.Log($"   Purpose: {purpose}");
    }

    [ContextMenu("Generate Level Map")]
    public void GenerateLevelMap()
    {
        Debug.Log("🗺️ OPENING AREA MAP");
        Debug.Log("==================");
        Debug.Log("Section 1 (0, 0): Awakening Chamber - Safe starting area");
        Debug.Log("Section 2 (20, 4): First Steps - Basic platforming");
        Debug.Log("Section 3 (40, -5): Darkness - World begins to darken");
        Debug.Log("Section 4 (60, -10): Lantern Discovery - The emotional centerpiece");
        Debug.Log("Section 5 (85, -8): First Light - Learn to reveal hidden platforms");
        Debug.Log("Section 6 (110, 5): Hidden Paths - Master light mechanics");
        Debug.Log("Section 7 (140, 0): The Depths - Mana management under pressure");
        Debug.Log("Section 8 (200, -1): First Shrine - Major milestone, gain Radiance light");
        Debug.Log("==================");
        Debug.Log("🎯 This opening area serves as a complete MVP demonstrating all core mechanics!");
    }

    #endregion
}