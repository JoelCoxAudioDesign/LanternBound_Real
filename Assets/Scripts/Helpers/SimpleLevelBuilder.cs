using UnityEngine;

/// <summary>
/// Simple level building helper script that creates basic level geometry
/// Attach this to an empty GameObject and use it to quickly prototype levels
/// </summary>
public class SimpleLevelBuilder : MonoBehaviour
{
    [Header("Level Building Tools")]
    [SerializeField] private bool _buildLevel = false;
    [SerializeField] private Material _platformMaterial;
    [SerializeField] private Material _wallMaterial;
    [SerializeField] private Material _backgroundMaterial;

    [Header("Colors for Different Elements")]
    [SerializeField] private Color _platformColor = Color.gray;
    [SerializeField] private Color _wallColor = Color.black;
    [SerializeField] private Color _backgroundColor = new Color(0.2f, 0.2f, 0.3f);
    [SerializeField] private Color _interactableColor = Color.yellow;

    // This runs in the editor when you change values in inspector
    private void OnValidate()
    {
        if (_buildLevel)
        {
            _buildLevel = false;
            BuildBasicLevel();
        }
    }

    [ContextMenu("Build Basic Level")]
    public void BuildBasicLevel()
    {
        // Clear existing level geometry (optional)
        ClearExistingLevel();

        // Build the opening area layout based on your design document
        BuildStartingChamber();
        BuildDescentTunnel();
        BuildLanternShrine();
        BuildFirstPuzzleArea();
        BuildEnemyEncounterArea();
        BuildCentralChamber();
    }

    private void ClearExistingLevel()
    {
        // Remove all child objects (be careful with this!)
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    private void BuildStartingChamber()
    {
        GameObject chamber = new GameObject("Starting Chamber");
        chamber.transform.SetParent(transform);

        // Floor
        CreatePlatform(chamber.transform, new Vector3(0, 0, 0), new Vector3(10, 1, 1), "Starting Floor");

        // Walls
        CreateWall(chamber.transform, new Vector3(-5, 3, 0), new Vector3(1, 6, 1), "Left Wall");
        CreateWall(chamber.transform, new Vector3(5, 3, 0), new Vector3(1, 6, 1), "Right Wall");

        // Ceiling (partial, to allow exit)
        CreateWall(chamber.transform, new Vector3(-2, 6, 0), new Vector3(6, 1, 1), "Ceiling");

        // Player spawn point
        GameObject spawn = new GameObject("Player Spawn");
        spawn.transform.SetParent(chamber.transform);
        spawn.transform.position = new Vector3(-3, 2, 0);
        // spawn.tag = "PlayerSpawn"; // Skip tag for now to avoid error
    }

    private void BuildDescentTunnel()
    {
        GameObject tunnel = new GameObject("Descent Tunnel");
        tunnel.transform.SetParent(transform);

        // Sloped floor going down
        CreatePlatform(tunnel.transform, new Vector3(8, -1, 0), new Vector3(6, 1, 1), "Tunnel Floor 1");
        CreatePlatform(tunnel.transform, new Vector3(14, -3, 0), new Vector3(6, 1, 1), "Tunnel Floor 2");
        CreatePlatform(tunnel.transform, new Vector3(20, -5, 0), new Vector3(6, 1, 1), "Tunnel Floor 3");

        // Tunnel walls
        CreateWall(tunnel.transform, new Vector3(8, 1, 0), new Vector3(6, 1, 1), "Tunnel Ceiling 1");
        CreateWall(tunnel.transform, new Vector3(14, -1, 0), new Vector3(6, 1, 1), "Tunnel Ceiling 2");
        CreateWall(tunnel.transform, new Vector3(20, -3, 0), new Vector3(6, 1, 1), "Tunnel Ceiling 3");
    }

    private void BuildLanternShrine()
    {
        GameObject shrineArea = new GameObject("Lantern Shrine Area");
        shrineArea.transform.SetParent(transform);

        // Circular chamber
        CreatePlatform(shrineArea.transform, new Vector3(25, -8, 0), new Vector3(12, 1, 1), "Shrine Chamber Floor");

        // Shrine pedestal in center
        GameObject pedestal = CreateInteractable(shrineArea.transform, new Vector3(25, -6, 0), new Vector3(2, 2, 1), "Lantern Pedestal");

        // Add the actual lantern shrine component
        if (pedestal.GetComponent<LanternShrine>() == null)
        {
            pedestal.AddComponent<LanternShrine>();
        }
    }

    private void BuildFirstPuzzleArea()
    {
        GameObject puzzleArea = new GameObject("First Puzzle Area");
        puzzleArea.transform.SetParent(transform);

        // Main platform
        CreatePlatform(puzzleArea.transform, new Vector3(40, -8, 0), new Vector3(16, 1, 1), "Puzzle Main Platform");

        // Hidden platforms that will be revealed (make them slightly transparent)
        GameObject hiddenPlatform1 = CreatePlatform(puzzleArea.transform, new Vector3(38, -5, 0), new Vector3(4, 1, 1), "Hidden Platform 1");
        GameObject hiddenPlatform2 = CreatePlatform(puzzleArea.transform, new Vector3(42, -3, 0), new Vector3(4, 1, 1), "Hidden Platform 2");

        // Add RevealablePlatform components
        if (hiddenPlatform1.GetComponent<RevealablePlatform>() == null)
        {
            // Add a 2D collider first (required by RevealablePlatform)
            hiddenPlatform1.AddComponent<BoxCollider2D>();
            hiddenPlatform1.AddComponent<RevealablePlatform>();
        }
        if (hiddenPlatform2.GetComponent<RevealablePlatform>() == null)
        {
            // Add a 2D collider first (required by RevealablePlatform)
            hiddenPlatform2.AddComponent<BoxCollider2D>();
            hiddenPlatform2.AddComponent<RevealablePlatform>();
        }

        // Make them start invisible
        SetPlatformTransparency(hiddenPlatform1, 0.3f);
        SetPlatformTransparency(hiddenPlatform2, 0.3f);

        // Shrine to activate puzzle
        GameObject shrine = CreateInteractable(puzzleArea.transform, new Vector3(45, -6, 0), new Vector3(1.5f, 1.5f, 1), "Puzzle Shrine");
        if (shrine.GetComponent<LanternShrine>() == null)
        {
            shrine.AddComponent<LanternShrine>();
        }
    }

    private void BuildEnemyEncounterArea()
    {
        GameObject enemyArea = new GameObject("Enemy Encounter Area");
        enemyArea.transform.SetParent(transform);

        // Long corridor
        CreatePlatform(enemyArea.transform, new Vector3(60, -8, 0), new Vector3(20, 1, 1), "Enemy Corridor Floor");
        CreateWall(enemyArea.transform, new Vector3(60, -5, 0), new Vector3(20, 1, 1), "Enemy Corridor Ceiling");

        // Patrol points for enemies
        GameObject patrolPoint1 = new GameObject("Patrol Point 1");
        patrolPoint1.transform.SetParent(enemyArea.transform);
        patrolPoint1.transform.position = new Vector3(52, -6, 0);

        GameObject patrolPoint2 = new GameObject("Patrol Point 2");
        patrolPoint2.transform.SetParent(enemyArea.transform);
        patrolPoint2.transform.position = new Vector3(68, -6, 0);

        // Enemy spawn (you'll add the enemy component manually)
        GameObject enemySpawn = new GameObject("Enemy Spawn");
        enemySpawn.transform.SetParent(enemyArea.transform);
        enemySpawn.transform.position = new Vector3(60, -6, 0);
        // enemySpawn.tag = "EnemySpawn"; // Skip tag for now to avoid error
    }

    private void BuildCentralChamber()
    {
        GameObject centralChamber = new GameObject("Central Chamber");
        centralChamber.transform.SetParent(transform);

        // Large circular floor
        CreatePlatform(centralChamber.transform, new Vector3(85, -8, 0), new Vector3(16, 1, 1), "Central Floor");

        // Multiple shrines for final puzzle
        GameObject shrine1 = CreateInteractable(centralChamber.transform, new Vector3(80, -6, 0), new Vector3(1.5f, 1.5f, 1), "Central Shrine 1");
        GameObject shrine2 = CreateInteractable(centralChamber.transform, new Vector3(85, -6, 0), new Vector3(1.5f, 1.5f, 1), "Central Shrine 2");
        GameObject shrine3 = CreateInteractable(centralChamber.transform, new Vector3(90, -6, 0), new Vector3(1.5f, 1.5f, 1), "Central Shrine 3");

        // Add shrine components
        foreach (GameObject shrine in new GameObject[] { shrine1, shrine2, shrine3 })
        {
            if (shrine.GetComponent<LanternShrine>() == null)
            {
                shrine.AddComponent<LanternShrine>();
            }
        }

        // Exit paths (blocked initially)
        CreateWall(centralChamber.transform, new Vector3(95, -6, 0), new Vector3(1, 4, 1), "Exit Door 1");
        CreateWall(centralChamber.transform, new Vector3(85, -3, 0), new Vector3(4, 1, 1), "Exit Door 2");
    }

    private GameObject CreatePlatform(Transform parent, Vector3 position, Vector3 scale, string name)
    {
        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.name = name;
        platform.transform.SetParent(parent);
        platform.transform.position = position;
        platform.transform.localScale = scale;

        // Set up collider for platforms
        platform.layer = LayerMask.NameToLayer("Ground");
        if (platform.layer == -1) platform.layer = 3; // Default ground layer

        // Color it
        Renderer renderer = platform.GetComponent<Renderer>();
        if (renderer != null)
        {
            if (_platformMaterial != null)
            {
                renderer.sharedMaterial = _platformMaterial;
            }
            else
            {
                renderer.sharedMaterial.color = _platformColor;
            }
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

        // Set up collider for walls
        wall.layer = LayerMask.NameToLayer("Ground");
        if (wall.layer == -1) wall.layer = 3; // Default ground layer

        // Color it
        Renderer renderer = wall.GetComponent<Renderer>();
        if (renderer != null)
        {
            if (_wallMaterial != null)
            {
                renderer.sharedMaterial = _wallMaterial;
            }
            else
            {
                renderer.sharedMaterial.color = _wallColor;
            }
        }

        return wall;
    }

    private GameObject CreateInteractable(Transform parent, Vector3 position, Vector3 scale, string name)
    {
        GameObject interactable = GameObject.CreatePrimitive(PrimitiveType.Cube);
        interactable.name = name;
        interactable.transform.SetParent(parent);
        interactable.transform.position = position;
        interactable.transform.localScale = scale;

        // Color it to stand out
        Renderer renderer = interactable.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial.color = _interactableColor;
        }

        return interactable;
    }

    private void SetPlatformTransparency(GameObject platform, float alpha)
    {
        Renderer renderer = platform.GetComponent<Renderer>();
        if (renderer != null)
        {
            Color color = renderer.material.color;
            color.a = alpha;
            renderer.material.color = color;

            // Make material transparent
            renderer.material.SetFloat("_Mode", 3);
            renderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            renderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            renderer.material.SetInt("_ZWrite", 0);
            renderer.material.DisableKeyword("_ALPHATEST_ON");
            renderer.material.EnableKeyword("_ALPHABLEND_ON");
            renderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            renderer.material.renderQueue = 3000;
        }
    }
}