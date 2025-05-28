using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Advanced level builder that creates a full tutorial level with integrated systems
/// </summary>
public class ComprehensiveLevelBuilder : MonoBehaviour
{
    [Header("Level Building")]
    [SerializeField] private bool _buildCompleteLevel = false;
    [SerializeField] private bool _setupTutorialSystem = false;

    [Header("Visual Settings")]
    [SerializeField] private Material _platformMaterial;
    [SerializeField] private Material _wallMaterial;
    [SerializeField] private Material _shrineMaterial;

    [Header("Colors")]
    [SerializeField] private Color _platformColor = Color.gray;
    [SerializeField] private Color _wallColor = Color.black;
    [SerializeField] private Color _shrineColor = Color.yellow;
    [SerializeField] private Color _hiddenPlatformColor = new Color(1f, 1f, 1f, 0.3f);

    private void OnValidate()
    {
        if (_buildCompleteLevel)
        {
            _buildCompleteLevel = false;
            BuildCompleteLevel();
        }

        if (_setupTutorialSystem)
        {
            _setupTutorialSystem = false;
            SetupTutorialSystem();
        }
    }

    [ContextMenu("Build Complete Level")]
    public void BuildCompleteLevel()
    {
        ClearLevel();

        // Build level sections
        GameObject startingArea = BuildStartingArea();
        GameObject descentArea = BuildDescentArea();
        GameObject shrineDiscoveryArea = BuildShrineDiscoveryArea();
        GameObject firstPuzzleArea = BuildFirstPuzzleArea();
        GameObject enemyArea = BuildEnemyArea();
        GameObject finalChamber = BuildFinalChamber();

        // Connect areas with tutorial triggers
        SetupTutorialTriggers();

        Debug.Log("Complete level built! Don't forget to add your player and set up the tutorial system.");
    }

    private void ClearLevel()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    private GameObject BuildStartingArea()
    {
        GameObject area = new GameObject("01_Starting_Area");
        area.transform.SetParent(transform);

        // Starting platform
        CreatePlatform(area.transform, Vector3.zero, new Vector3(12, 1, 1), "Starting Platform");

        // Walls to guide player forward
        CreateWall(area.transform, new Vector3(-6, 4, 0), new Vector3(1, 8, 1), "Left Boundary");
        CreateWall(area.transform, new Vector3(6, 4, 0), new Vector3(1, 8, 1), "Right Boundary");

        // Player spawn
        GameObject spawn = new GameObject("Player_Spawn_Point");
        spawn.transform.SetParent(area.transform);
        spawn.transform.position = new Vector3(-4, 2, 0);

        // Tutorial trigger for first message
        GameObject tutorialTrigger = CreateTutorialTrigger(area.transform, new Vector3(0, 1, 0), "Introduction");

        return area;
    }

    private GameObject BuildDescentArea()
    {
        GameObject area = new GameObject("02_Descent_Area");
        area.transform.SetParent(transform);

        // Descending platforms
        CreatePlatform(area.transform, new Vector3(10, -1, 0), new Vector3(6, 1, 1), "Descent Platform 1");
        CreatePlatform(area.transform, new Vector3(16, -3, 0), new Vector3(6, 1, 1), "Descent Platform 2");
        CreatePlatform(area.transform, new Vector3(22, -5, 0), new Vector3(6, 1, 1), "Descent Platform 3");

        // Walls to create tunnel effect
        CreateWall(area.transform, new Vector3(10, 1, 0), new Vector3(6, 1, 1), "Tunnel Ceiling 1");
        CreateWall(area.transform, new Vector3(16, -1, 0), new Vector3(6, 1, 1), "Tunnel Ceiling 2");
        CreateWall(area.transform, new Vector3(22, -3, 0), new Vector3(6, 1, 1), "Tunnel Ceiling 3");

        // Tutorial trigger for darkness message
        CreateTutorialTrigger(area.transform, new Vector3(16, -2, 0), "Darkness");

        return area;
    }

    private GameObject BuildShrineDiscoveryArea()
    {
        GameObject area = new GameObject("03_Shrine_Discovery");
        area.transform.SetParent(transform);

        // Circular chamber floor
        CreatePlatform(area.transform, new Vector3(28, -8, 0), new Vector3(14, 1, 1), "Discovery Chamber Floor");

        // Main lantern shrine (the one that gives player the lantern)
        GameObject mainShrine = CreateShrine(area.transform, new Vector3(28, -6, 0), new Vector3(2, 3, 1), "Main Lantern Shrine");

        // Add some atmospheric elements
        CreateWall(area.transform, new Vector3(28, -3, 0), new Vector3(4, 1, 1), "Shrine Canopy");

        // Tutorial trigger for lantern discovery
        CreateTutorialTrigger(area.transform, new Vector3(25, -7, 0), "LanternDiscovery");

        return area;
    }

    private GameObject BuildFirstPuzzleArea()
    {
        GameObject area = new GameObject("04_First_Puzzle");
        area.transform.SetParent(transform);

        // Main platform
        CreatePlatform(area.transform, new Vector3(45, -8, 0), new Vector3(16, 1, 1), "Puzzle Main Platform");

        // Hidden platforms that need to be revealed
        GameObject hiddenPlatform1 = CreateHiddenPlatform(area.transform, new Vector3(42, -5, 0), new Vector3(4, 1, 1), "Hidden Platform 1");
        GameObject hiddenPlatform2 = CreateHiddenPlatform(area.transform, new Vector3(48, -3, 0), new Vector3(4, 1, 1), "Hidden Platform 2");
        GameObject hiddenPlatform3 = CreateHiddenPlatform(area.transform, new Vector3(54, -1, 0), new Vector3(4, 1, 1), "Hidden Platform 3");

        // Activation shrine
        GameObject activationShrine = CreateShrine(area.transform, new Vector3(45, -6, 0), new Vector3(1.5f, 2, 1), "Platform Activation Shrine");

        // Goal platform
        CreatePlatform(area.transform, new Vector3(58, 2, 0), new Vector3(6, 1, 1), "Goal Platform");

        // Tutorial trigger for puzzle explanation
        CreateTutorialTrigger(area.transform, new Vector3(40, -7, 0), "FirstPuzzle");

        return area;
    }

    private GameObject BuildEnemyArea()
    {
        GameObject area = new GameObject("05_Enemy_Encounter");
        area.transform.SetParent(transform);

        // Long corridor
        CreatePlatform(area.transform, new Vector3(75, 2, 0), new Vector3(20, 1, 1), "Enemy Corridor");
        CreateWall(area.transform, new Vector3(75, 6, 0), new Vector3(20, 1, 1), "Corridor Ceiling");

        // Patrol points for enemy
        GameObject patrolPoint1 = new GameObject("Enemy_Patrol_Point_1");
        patrolPoint1.transform.SetParent(area.transform);
        patrolPoint1.transform.position = new Vector3(68, 4, 0);

        GameObject patrolPoint2 = new GameObject("Enemy_Patrol_Point_2");
        patrolPoint2.transform.SetParent(area.transform);
        patrolPoint2.transform.position = new Vector3(82, 4, 0);

        // Enemy spawn point
        GameObject enemySpawn = new GameObject("Enemy_Spawn_Point");
        enemySpawn.transform.SetParent(area.transform);
        enemySpawn.transform.position = new Vector3(75, 4, 0);

        // Safe alcoves
        CreatePlatform(area.transform, new Vector3(70, 5, 0), new Vector3(3, 1, 1), "Safe Alcove 1");
        CreatePlatform(area.transform, new Vector3(80, 5, 0), new Vector3(3, 1, 1), "Safe Alcove 2");

        // Tutorial trigger for enemy encounter
        CreateTutorialTrigger(area.transform, new Vector3(65, 3, 0), "EnemyEncounter");

        return area;
    }

    private GameObject BuildFinalChamber()
    {
        GameObject area = new GameObject("06_Final_Chamber");
        area.transform.SetParent(transform);

        // Large circular floor
        CreatePlatform(area.transform, new Vector3(100, 2, 0), new Vector3(18, 1, 1), "Final Chamber Floor");

        // Multiple shrines for final puzzle
        GameObject shrine1 = CreateShrine(area.transform, new Vector3(95, 4, 0), new Vector3(1.5f, 2, 1), "Final Shrine 1");
        GameObject shrine2 = CreateShrine(area.transform, new Vector3(100, 4, 0), new Vector3(1.5f, 2, 1), "Final Shrine 2");
        GameObject shrine3 = CreateShrine(area.transform, new Vector3(105, 4, 0), new Vector3(1.5f, 2, 1), "Final Shrine 3");

        // Blocked exits that open when puzzle is solved
        GameObject exitDoor1 = CreateWall(area.transform, new Vector3(110, 5, 0), new Vector3(1, 6, 1), "Exit Door 1");
        GameObject exitDoor2 = CreateWall(area.transform, new Vector3(100, 8, 0), new Vector3(6, 1, 1), "Exit Door 2");

        // Mark doors as destructible/removable
        exitDoor1.name += "_REMOVABLE";
        exitDoor2.name += "_REMOVABLE";

        // Tutorial trigger for final puzzle
        CreateTutorialTrigger(area.transform, new Vector3(92, 3, 0), "FinalPuzzle");

        return area;
    }

    private void SetupTutorialTriggers()
    {
        // Find all tutorial triggers and set them up properly
        TutorialTrigger[] triggers = FindObjectsOfType<TutorialTrigger>();
        foreach (var trigger in triggers)
        {
            // Ensure they have proper colliders
            if (trigger.GetComponent<Collider2D>() == null)
            {
                BoxCollider2D triggerCollider = trigger.gameObject.AddComponent<BoxCollider2D>();
                triggerCollider.isTrigger = true;
                triggerCollider.size = new Vector2(3, 3); // Reasonable trigger size
            }
        }
    }

    [ContextMenu("Setup Tutorial System")]
    public void SetupTutorialSystem()
    {
        // Create tutorial manager
        GameObject tutorialManager = new GameObject("Tutorial_Manager");
        tutorialManager.transform.SetParent(transform);

        OpeningAreaManager manager = tutorialManager.AddComponent<OpeningAreaManager>();

        // Create UI Canvas
        GameObject canvas = new GameObject("Tutorial_UI_Canvas");
        canvas.transform.SetParent(transform);

        Canvas canvasComponent = canvas.AddComponent<Canvas>();
        canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasComponent.sortingOrder = 100;

        canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Create tutorial UI
        GameObject tutorialUI = new GameObject("Tutorial_UI");
        tutorialUI.transform.SetParent(canvas.transform);

        TutorialUI uiComponent = tutorialUI.AddComponent<TutorialUI>();

        Debug.Log("Tutorial system created! You'll need to manually configure the tutorial steps in the OpeningAreaManager component.");
    }

    // Helper methods for creating different types of objects
    private GameObject CreatePlatform(Transform parent, Vector3 position, Vector3 scale, string name)
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
        platform.layer = LayerMask.NameToLayer("Ground");
        if (platform.layer == -1) platform.layer = 3;

        // Set color
        Renderer renderer = platform.GetComponent<Renderer>();
        if (renderer != null)
        {
            if (_platformMaterial != null)
                renderer.sharedMaterial = _platformMaterial;
            else
                renderer.sharedMaterial.color = _platformColor;
        }

        return platform;
    }

    private GameObject CreateWall(Transform parent, Vector3 position, Vector3 scale, string name)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent);
        wall.transform.position = position;
        wall.transform.localScale = scale;

        // Remove 3D collider, add 2D collider
        DestroyImmediate(wall.GetComponent<BoxCollider>());
        BoxCollider2D collider2D = wall.AddComponent<BoxCollider2D>();

        // Set layer
        wall.layer = LayerMask.NameToLayer("Ground");
        if (wall.layer == -1) wall.layer = 3;

        // Set color
        Renderer renderer = wall.GetComponent<Renderer>();
        if (renderer != null)
        {
            if (_wallMaterial != null)
                renderer.sharedMaterial = _wallMaterial;
            else
                renderer.sharedMaterial.color = _wallColor;
        }

        return wall;
    }

    private GameObject CreateShrine(Transform parent, Vector3 position, Vector3 scale, string name)
    {
        GameObject shrine = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shrine.name = name;
        shrine.transform.SetParent(parent);
        shrine.transform.position = position;
        shrine.transform.localScale = scale;

        // Remove 3D collider, add 2D trigger collider
        DestroyImmediate(shrine.GetComponent<BoxCollider>());
        BoxCollider2D collider2D = shrine.AddComponent<BoxCollider2D>();
        collider2D.isTrigger = true;

        // Add shrine component
        LanternShrine shrineComponent = shrine.AddComponent<LanternShrine>();

        // Set color
        Renderer renderer = shrine.GetComponent<Renderer>();
        if (renderer != null)
        {
            if (_shrineMaterial != null)
                renderer.sharedMaterial = _shrineMaterial;
            else
                renderer.sharedMaterial.color = _shrineColor;
        }

        return shrine;
    }

    private GameObject CreateHiddenPlatform(Transform parent, Vector3 position, Vector3 scale, string name)
    {
        GameObject platform = CreatePlatform(parent, position, scale, name);

        // Add RevealablePlatform component
        RevealablePlatform revealComponent = platform.AddComponent<RevealablePlatform>();

        // Make it start hidden and transparent
        Renderer renderer = platform.GetComponent<Renderer>();
        if (renderer != null)
        {
            Color hiddenColor = _hiddenPlatformColor;
            renderer.sharedMaterial.color = hiddenColor;
        }

        // Disable collider initially
        platform.GetComponent<BoxCollider2D>().enabled = false;

        return platform;
    }

    private GameObject CreateTutorialTrigger(Transform parent, Vector3 position, string stepName)
    {
        GameObject trigger = new GameObject($"Tutorial_Trigger_{stepName}");
        trigger.transform.SetParent(parent);
        trigger.transform.position = position;

        // Add collider
        BoxCollider2D collider = trigger.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(4, 4);

        // Add tutorial trigger component
        TutorialTrigger triggerComponent = trigger.AddComponent<TutorialTrigger>();

        return trigger;
    }
}